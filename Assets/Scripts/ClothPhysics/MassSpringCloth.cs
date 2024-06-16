/******************************************************************************************
 * ÁLVARO MORENO GARCÍA
 * GRADO EN DISEÑO Y DESARROLLO DE VIDEOJUEGOS - ANIMACIÓN 3D
 * Práctica II-Final
 * 
 * SCRIPT MODIFICADO A PARTIR DEL PROPORCIONADO EN EL AULA
 * Práctica II - básica II
 * 
 * Clase MassSpringCloth.cs: Es la que calcula las fuerzas sobre cada nodo (vértice) y 
 * halla su posición a lo largo del tiempo en función del método de integración elegido. 
 * Comunica estos cálculos a las clases Node y Spring para que se dibujen en pantalla los
 * objetos en su posición y orientación correctas dentro de la malla.
 *****************************************************************************************/
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MassSpringCloth : MonoBehaviour
{

    [Header("Simulation")] // Útil para organizar las variables en el editor
    public bool paused = false; // Indica si la animación está pausada 
    public Vector3 g = new Vector3(0f, 9.8f, 0f); // Valor de la gravedad (m/s^2)

    // Lista enumerada con los tipos de integración posibles
    public enum Integration
    {
        ExplicitEuler = 0,   // Tiene problemas de divergencia
        SymplecticEuler = 1, // Método de integración recomendado
    }

    public Integration integrationMethod; // Este será el método de integración
                                          // con el que vamos a calcular la
                                          // animación

    public float h = 0.01f; // Paso de integración (es un tiempo)

    [Header("Node and spring properties")]
    public float nodeMass = 2f; // Masa del nodo (kg)
    public float tractionRigidity = 1000f;// Constante de rigidez de muelles de tracción
    public float flexionRigidity = 600f;// Constante de rigidez de muelles de flexión

    [Header("Position offset")]
    // Arreglos de la posición de la malla en cada eje
    public float Xoffset = 1f;
    public float Yoffset = 1f;
    public float Zoffset = 1f;

    [Header("Prefabs")]
    [SerializeField] GameObject nodePrefab;
    [SerializeField] GameObject springPrefab;

    //Variables privadas
    List<Edge> edges; // Listas de aristas: cada una con 2 vértices conectados y un opuesto
    List<Node> nodes; // Lista que contiene todos los nodos
    List<Spring> springs; // Lista que contiene todos los muelles

    Mesh mesh; //Malla del objeto tela
    Vector3[] vertices; //Posiciones de vertices de la malla
    int[] triangles; // Índices de vértices de la malla


    void Start()
    {
        edges = new ();
        nodes = new ();
        springs = new ();

        mesh = GetComponent<MeshFilter>().mesh; //Se accede a la malla
        vertices = mesh.vertices; //Array de las posiciones de los vértices de la malla
        triangles = mesh.triangles; //Array de índices de los vértices de cada triángulo

        InitializeNodes(); // Inicializa los nodos a partir de los vértices y la masa

        CreateSprings(); // Crea los muelles de tracción o flexión según aristas

        InitializeSprings(); // Inicializa los muelles
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.P)) // Detectamos si se ha pulsado la tecla P
        {
            // La tecla P hace de "toggle" para pausar o quitar la pausa de la
            // animación
            paused = !paused;
        }

        // Actualiza la posición de los vértices de la malla a la par que la de los nodos
        CorrectMesh();

        // Comprueba si la masa de los nodos o la rigidez de los muelles ha cambiado
        CheckNodeSpringValues();
    }

    private void FixedUpdate()
    {
        if (paused)
            // Si está pausada la animación, no hacemos nada y regresamos
            return;

        // Según el método de integración escogido, se invoca una función u otra
        switch (integrationMethod)
        {
            case Integration.ExplicitEuler:
                IntegrateExplicitEuler();
                break;

            case Integration.SymplecticEuler:
                IntegrateSymplecticEuler();
                break;

            default:
                print("ERROR METODO INTEGRACION DESCONOCIDO");
                break;
        }

        // Recorremos la lista de muelles para recalcularlos, una vez que hemos
        // calculado la nueva posición de los nodos con el método de integración
        foreach (Spring spring in springs)
        {
            // Vector dirección del muelle, apunta de B a A            
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;
            // Nueva longitud del muelle 
            spring.length = spring.dir.magnitude;
            // Normalizamos el vector que almacena la orientación del muelle
            spring.dir = Vector3.Normalize(spring.dir);
            // Posición del punto medio del muelle: media aritmética de las
            // posiciones de los dos nodos
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;
            // Orientamos correctamente el muelle según el vector dir
            spring.rotation = Quaternion.FromToRotation(Vector3.up, spring.dir);
        }
    }

    /// <summary>
    /// Método de integración de Euler Explícito
    /// </summary>
    private void IntegrateExplicitEuler()
    {
        // Recorremos la lista de nodos para aplicar las fuerzas a cada uno de
        // ellos
        foreach (Node node in nodes)
        {
            if (!node.fixedNode) // Si el nodo no es fijo
            {
                // r_(n+1) = r_n + h * v_n
                node.pos += h * node.vel;
                node.force = -node.mass * g;

                // Amortiguamiento debido a la velocidad absoluta del nodo
                ApplyDampingNode(node);
            }
        }

        // Recorremos la lista de muelles para añadir a cada nodo la fuerza
        // elástica de cada muelle. Por la ley de acción y reacción, estas
        // fuerzas son iguales y de sentidos opuestos en los extremos de cada
        // muelle
        foreach (Spring spring in springs)
        {
            spring.nodeA.force += -spring.k * (spring.length - spring.length0)
                * spring.dir;
            spring.nodeB.force += spring.k * (spring.length - spring.length0)
                * spring.dir;
            // Amortiguamiento debido a la velocidad relativa entre los nodos del muelle
            ApplyDampingSpring(spring);
        }

        // Recorremos de nuevo la lista de nodos para calcular la nueva
        // velocidad, una vez que ya conocemos la fuerza total en cada nodo
        foreach (Node node in nodes)
        {
            if (!node.fixedNode) // Si el nodo no es fijo
            {
                // v_(n+1) = v_n + h F_n / m
                node.vel += h * node.force / node.mass;
            }
        }
    }

    /// <summary>
    ///  Método de integración de Euler Simpléctico
    /// </summary>
    private void IntegrateSymplecticEuler()
    {
        // Recorremos la lista de nodos para aplicar las fuerzas a cada uno de
        // ellos
        foreach (Node node in nodes)
        {
            node.force = -node.mass * g;

            // Amortiguamiento debido a la velocidad absoluta del nodo
            ApplyDampingNode(node);
        }

        // Recorremos la lista de muelles para añadir a cada nodo la fuerza
        // elástica de cada muelle. Por la ley de acción y reacción, estas
        // fuerzas son iguales y de sentidos opuestos en los extremos de cada
        // muelle
        foreach (Spring spring in springs)
        {
            spring.nodeA.force += -spring.k * (spring.length - spring.length0)
                * spring.dir;
            spring.nodeB.force += spring.k * (spring.length - spring.length0)
                * spring.dir;

            // Amortiguamiento debido a la velocidad relativa entre los nodos del muelle
            ApplyDampingSpring(spring);
        }

        // Recorremos de nuevo la lista de nodos para calcular la nueva
        // velocidad y la nueva posición, una vez que ya conocemos la fuerza
        // total en cada nodo
        foreach (Node node in nodes)
        {
            if (!node.fixedNode) // Si el nodo no es fijo
            {
                // v_(n+1) = v_n + h F_n / m
                node.vel += h * node.force / node.mass;
                // r_(n+1) = r_n + h * v_(n+1)
                node.pos += h * node.vel;
            }
        }
    }

    /// <summary>
    /// Este es el amortiguamiento que se aplica a cada nodo en función de su
    /// velocidad absoluta (es como la fricción con el aire)
    /// </summary>
    private void ApplyDampingNode(Node node)
    {
        node.force += -node.dAbsolute * node.vel;
    }

    /// <summary>
    /// Este es el amortiguamiento que se aplica a los dos nodos de un muelle en
    /// función de la velocidad relativa entre ellos
    /// </summary>
    private void ApplyDampingSpring(Spring spring)
    {
        //spring.nodeA.force += -spring.dRotation * (spring.nodeA.vel - spring.nodeB.vel);
        spring.nodeA.force += -spring.dDeformation
            * Vector3.Dot(spring.nodeA.vel - spring.nodeB.vel, spring.dir)
            * spring.dir;
        //spring.nodeB.force += spring.dRotation * (spring.nodeA.vel - spring.nodeB.vel);
        spring.nodeB.force += spring.dDeformation
            * Vector3.Dot(spring.nodeA.vel - spring.nodeB.vel, spring.dir)
            * spring.dir;
    }

    /// <summary>
    /// Método para inicializar los muelles
    /// </summary>
    private void InitializeSprings()
    {
        // Recorremos la lista de muelles para conocer su posición inicial, así
        // como la orientación y la longitud de reposo
        foreach (Spring spring in springs)
        {
            // Vector dirección en el instante inicial, apunta de B a A
            // con un tamaño igual a la longitud del muelle
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;
            // Establecemos la longitud natural del muelle como la distancia
            // entre ambos nodos en el instante inicial
            spring.length0 = spring.dir.magnitude;
            // Y en ese instante inicial la longitud del muelle también coincide
            // con la longitud natural
            spring.length = spring.length0;
            // Normalizamos el vector que almacena la orientación del muelle
            spring.dir = Vector3.Normalize(spring.dir);
            // Posición del punto medio del muelle: media aritmética de las
            // posiciones de los dos nodos
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;
            // Orientamos correctamente el muelle según el vector dir
            spring.rotation = Quaternion.FromToRotation(Vector3.up, spring.dir);
        }
    }

    /// <summary>
    /// Método para inicializar un nodos según la posición de su vértice y la masa
    /// </summary>
    private void InitializeNodes()
    {
        // Recorremos la lista de vertices para conocer la posición inicial de los nodos
        foreach (Vector3 vertex in vertices)
        {
            // Transforma la posición del vértice de local a global
            Vector3 vertexWorldPos = transform.TransformPoint(vertex);
            // Crea un objeto en la posición global del vértice con el prefab de un nodo
            GameObject nodeObj = Instantiate(nodePrefab, vertexWorldPos, Quaternion.identity);
            // Lo hace hijo de este objeto
            nodeObj.transform.SetParent(transform);
            // Coje su script de Node
            Node node = nodeObj.GetComponent<Node>();
            // Inicializa un nodo en ese vértice
            node.Initialize(vertexWorldPos, nodeMass);
            // Añade el nodo a la lista de nodos de la tela
            nodes.Add(node);
        }
    }

    /// <summary>
    /// Método para crear los muelles a partir de las aristas
    /// </summary>
    private void CreateSprings()
    {
        // Variables locales
        Edge thisEdge; // Arista en iteración actual
        Node nodeA, nodeB;

        // Rellena la lista de aristas con las de tracción y flexión
        CalculateEdges();

        // Cada arista
        for (int i = 0; i < edges.Count; i++)
        {
            thisEdge = edges[i]; // Esta arista

            // Los nodos que corresponden a cada vértice de esta arista
            nodeA = nodes[thisEdge.indexA];
            nodeB = nodes[thisEdge.indexB];

            // Se crea un muelle con los dos nodos que conecta y su tipo
            InitializeSpring(nodeA, nodeB, thisEdge.springType);
        }
    }

    /// <summary>
    /// Método para calcular las aristas: primero las de tracción y luego las de
    /// felxión por cada arista de tracción repetida
    /// </summary>
    private void CalculateEdges()
    {
        // Primero se añaden todas las de tracción a la lista de aristas
        GetTractionEdges();

        // Variables locales
        Edge thisEdge, nextEdge; // Arista en una iteración y su siguiente
        List <Edge> flexionEdges = new(); // Lista de aristas de flexión

        edges.Sort();//Se ordenan ascendetemente según el índice de su primer vértice

        // Cada arista de tracción
        for (int i = 0; i < edges.Count - 1; i++)
        {
            thisEdge = edges[i]; // Esta arista
            nextEdge = edges[i + 1]; // Siguiente arista

            // Si ambas tienen los mismos extremos
            if ((thisEdge.indexA == nextEdge.indexA) && (thisEdge.indexB == nextEdge.indexB))
            {
                // Elimina la siguiente ya que es la misma que esta
                edges.RemoveAt(i + 1);

                // Se crea una arista entre sus vértices opuestos que correspondería
                // a un muelle de flexión
                Edge flexionEdge = new(thisEdge.indexOther, nextEdge.indexOther,
                                        thisEdge.indexA, Spring.Type.Flexion);

                // Añade la nueva arista de flexión a la lista
                flexionEdges.Add(flexionEdge);
            }
        }

        // Añade las aristas de flexión a la lista de aristas global
        edges.AddRange(flexionEdges);
    }

    /// <summary>
    /// Método para calcular las aristas de tracción (naturales de la malla)
    /// </summary>
    private void GetTractionEdges()
    {
        // Variables locales
        int A, B, C; //Vértices (índices)

        // Cada triángulo: serían muelles de tracción
        for (int i = 0; i < triangles.Length - 2; i += 3)
        {
            // Índices de los vértices del triángulo
            A = triangles[i];
            B = triangles[i + 1];
            C = triangles[i + 2];

            // Una arista por cada combinación diferente
            // Corresponderían a muelles de tracción
            Edge tractionEdgeAB = new(A, B, C, Spring.Type.Traction);//BCD
            Edge tractionEdgeAC = new(A, C, B, Spring.Type.Traction);//BDC
            Edge tractionEdgeBC = new(B, C, A, Spring.Type.Traction);//CDB

            // Se añaden a la lista de aristas
            edges.Add(tractionEdgeAB);
            edges.Add(tractionEdgeAC);
            edges.Add(tractionEdgeBC);
        }
    }

    /// <summary>
    /// Método para inicializar un muelle según sus nodos y el tipo
    /// </summary>
    private void InitializeSpring(Node nodeA, Node nodeB, Spring.Type type)
    {
        // Calcula la posición del GameObject
        Vector3 springPos = (nodeA.pos + nodeB.pos) / 2f;
        // Crea el GameObject del muelle
        GameObject springObj = Instantiate(springPrefab, springPos, Quaternion.identity);
        // Lo hace hijo de este objeto
        springObj.transform.SetParent(transform);
        // Coje el código de Spring
        Spring spring = springObj.GetComponent<Spring>();
        // Inicializa el muelle dados los dos nodos y la rigidez según el tipo
        spring.Initialize(nodeA, nodeB, type, 
            type == Spring.Type.Traction ? tractionRigidity : flexionRigidity);
        // Añade el muelle a la lista
        springs.Add(spring);
    }

    /// <summary>
    /// Método para actualizar los vértices de la malla a la par que los nodos
    /// </summary>
    private void CorrectMesh()
    {
        // Corrección de rotación
        Quaternion rotCorrection = Quaternion.Inverse(transform.rotation);

        // Corrección de posición, según el offset de cada eje
        Vector3 posCorrection = new (transform.position.x * Xoffset,
                                     transform.position.y * Yoffset,
                                     transform.position.z * Zoffset);

        // Cada nodo
        for (int i = 0; i < nodes.Count; i++)
        {
            // Actualiza la posición del vértice con las correcciones
            vertices[i] = rotCorrection * nodes[i].pos + posCorrection;
        }

        // Actualiza la lista de vértices de la malla
        mesh.vertices = vertices;
    }

    /// <summary>
    /// Método para actualizar la masa de los nodos y la rigidez de los muelles si ha
    /// han cambiado
    /// </summary>
    private void CheckNodeSpringValues()
    {
        // Si no ha cambiado solo accede al primer nodo (no es costoso)
        if (nodes[0].mass != nodeMass)
        {
            // Cada nodo
            foreach (Node node in nodes)
            {
                // Actualiza su masa
                node.mass = nodeMass;
            }
        }

        // Si no ha cambiado solo accede al primer muelle (no es costoso)
        // Ha cambiado si su rigidez no es ni la de tracción ni la de flexión
        if (springs[0].k != tractionRigidity || springs[0].k != flexionRigidity) 
        {
            //Cada muelle
            foreach (Spring spring in springs)
            {
                // Comprueba su tipo
                if (spring.type == Spring.Type.Traction)
                {
                    // Actualiza la rigidez de tracción
                    spring.k = tractionRigidity;
                }
                else
                {
                    // Actualiza la rigidez de flexión
                    spring.k = flexionRigidity;
                }
            }
        }
    }
}