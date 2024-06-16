/******************************************************************************************
 * �LVARO MORENO GARC�A
 * GRADO EN DISE�O Y DESARROLLO DE VIDEOJUEGOS - ANIMACI�N 3D
 * Pr�ctica II-Final
 * 
 * SCRIPT MODIFICADO A PARTIR DEL PROPORCIONADO EN EL AULA
 * Pr�ctica II - b�sica II
 * 
 * Clase MassSpringCloth.cs: Es la que calcula las fuerzas sobre cada nodo (v�rtice) y 
 * halla su posici�n a lo largo del tiempo en funci�n del m�todo de integraci�n elegido. 
 * Comunica estos c�lculos a las clases Node y Spring para que se dibujen en pantalla los
 * objetos en su posici�n y orientaci�n correctas dentro de la malla.
 *****************************************************************************************/
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MassSpringCloth : MonoBehaviour
{

    [Header("Simulation")] // �til para organizar las variables en el editor
    public bool paused = false; // Indica si la animaci�n est� pausada 
    public Vector3 g = new Vector3(0f, 9.8f, 0f); // Valor de la gravedad (m/s^2)

    // Lista enumerada con los tipos de integraci�n posibles
    public enum Integration
    {
        ExplicitEuler = 0,   // Tiene problemas de divergencia
        SymplecticEuler = 1, // M�todo de integraci�n recomendado
    }

    public Integration integrationMethod; // Este ser� el m�todo de integraci�n
                                          // con el que vamos a calcular la
                                          // animaci�n

    public float h = 0.01f; // Paso de integraci�n (es un tiempo)

    [Header("Node and spring properties")]
    public float nodeMass = 2f; // Masa del nodo (kg)
    public float tractionRigidity = 1000f;// Constante de rigidez de muelles de tracci�n
    public float flexionRigidity = 600f;// Constante de rigidez de muelles de flexi�n

    [Header("Position offset")]
    // Arreglos de la posici�n de la malla en cada eje
    public float Xoffset = 1f;
    public float Yoffset = 1f;
    public float Zoffset = 1f;

    [Header("Prefabs")]
    [SerializeField] GameObject nodePrefab;
    [SerializeField] GameObject springPrefab;

    //Variables privadas
    List<Edge> edges; // Listas de aristas: cada una con 2 v�rtices conectados y un opuesto
    List<Node> nodes; // Lista que contiene todos los nodos
    List<Spring> springs; // Lista que contiene todos los muelles

    Mesh mesh; //Malla del objeto tela
    Vector3[] vertices; //Posiciones de vertices de la malla
    int[] triangles; // �ndices de v�rtices de la malla


    void Start()
    {
        edges = new ();
        nodes = new ();
        springs = new ();

        mesh = GetComponent<MeshFilter>().mesh; //Se accede a la malla
        vertices = mesh.vertices; //Array de las posiciones de los v�rtices de la malla
        triangles = mesh.triangles; //Array de �ndices de los v�rtices de cada tri�ngulo

        InitializeNodes(); // Inicializa los nodos a partir de los v�rtices y la masa

        CreateSprings(); // Crea los muelles de tracci�n o flexi�n seg�n aristas

        InitializeSprings(); // Inicializa los muelles
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.P)) // Detectamos si se ha pulsado la tecla P
        {
            // La tecla P hace de "toggle" para pausar o quitar la pausa de la
            // animaci�n
            paused = !paused;
        }

        // Actualiza la posici�n de los v�rtices de la malla a la par que la de los nodos
        CorrectMesh();

        // Comprueba si la masa de los nodos o la rigidez de los muelles ha cambiado
        CheckNodeSpringValues();
    }

    private void FixedUpdate()
    {
        if (paused)
            // Si est� pausada la animaci�n, no hacemos nada y regresamos
            return;

        // Seg�n el m�todo de integraci�n escogido, se invoca una funci�n u otra
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
        // calculado la nueva posici�n de los nodos con el m�todo de integraci�n
        foreach (Spring spring in springs)
        {
            // Vector direcci�n del muelle, apunta de B a A            
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;
            // Nueva longitud del muelle 
            spring.length = spring.dir.magnitude;
            // Normalizamos el vector que almacena la orientaci�n del muelle
            spring.dir = Vector3.Normalize(spring.dir);
            // Posici�n del punto medio del muelle: media aritm�tica de las
            // posiciones de los dos nodos
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;
            // Orientamos correctamente el muelle seg�n el vector dir
            spring.rotation = Quaternion.FromToRotation(Vector3.up, spring.dir);
        }
    }

    /// <summary>
    /// M�todo de integraci�n de Euler Expl�cito
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

        // Recorremos la lista de muelles para a�adir a cada nodo la fuerza
        // el�stica de cada muelle. Por la ley de acci�n y reacci�n, estas
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
    ///  M�todo de integraci�n de Euler Simpl�ctico
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

        // Recorremos la lista de muelles para a�adir a cada nodo la fuerza
        // el�stica de cada muelle. Por la ley de acci�n y reacci�n, estas
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
        // velocidad y la nueva posici�n, una vez que ya conocemos la fuerza
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
    /// Este es el amortiguamiento que se aplica a cada nodo en funci�n de su
    /// velocidad absoluta (es como la fricci�n con el aire)
    /// </summary>
    private void ApplyDampingNode(Node node)
    {
        node.force += -node.dAbsolute * node.vel;
    }

    /// <summary>
    /// Este es el amortiguamiento que se aplica a los dos nodos de un muelle en
    /// funci�n de la velocidad relativa entre ellos
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
    /// M�todo para inicializar los muelles
    /// </summary>
    private void InitializeSprings()
    {
        // Recorremos la lista de muelles para conocer su posici�n inicial, as�
        // como la orientaci�n y la longitud de reposo
        foreach (Spring spring in springs)
        {
            // Vector direcci�n en el instante inicial, apunta de B a A
            // con un tama�o igual a la longitud del muelle
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;
            // Establecemos la longitud natural del muelle como la distancia
            // entre ambos nodos en el instante inicial
            spring.length0 = spring.dir.magnitude;
            // Y en ese instante inicial la longitud del muelle tambi�n coincide
            // con la longitud natural
            spring.length = spring.length0;
            // Normalizamos el vector que almacena la orientaci�n del muelle
            spring.dir = Vector3.Normalize(spring.dir);
            // Posici�n del punto medio del muelle: media aritm�tica de las
            // posiciones de los dos nodos
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;
            // Orientamos correctamente el muelle seg�n el vector dir
            spring.rotation = Quaternion.FromToRotation(Vector3.up, spring.dir);
        }
    }

    /// <summary>
    /// M�todo para inicializar un nodos seg�n la posici�n de su v�rtice y la masa
    /// </summary>
    private void InitializeNodes()
    {
        // Recorremos la lista de vertices para conocer la posici�n inicial de los nodos
        foreach (Vector3 vertex in vertices)
        {
            // Transforma la posici�n del v�rtice de local a global
            Vector3 vertexWorldPos = transform.TransformPoint(vertex);
            // Crea un objeto en la posici�n global del v�rtice con el prefab de un nodo
            GameObject nodeObj = Instantiate(nodePrefab, vertexWorldPos, Quaternion.identity);
            // Lo hace hijo de este objeto
            nodeObj.transform.SetParent(transform);
            // Coje su script de Node
            Node node = nodeObj.GetComponent<Node>();
            // Inicializa un nodo en ese v�rtice
            node.Initialize(vertexWorldPos, nodeMass);
            // A�ade el nodo a la lista de nodos de la tela
            nodes.Add(node);
        }
    }

    /// <summary>
    /// M�todo para crear los muelles a partir de las aristas
    /// </summary>
    private void CreateSprings()
    {
        // Variables locales
        Edge thisEdge; // Arista en iteraci�n actual
        Node nodeA, nodeB;

        // Rellena la lista de aristas con las de tracci�n y flexi�n
        CalculateEdges();

        // Cada arista
        for (int i = 0; i < edges.Count; i++)
        {
            thisEdge = edges[i]; // Esta arista

            // Los nodos que corresponden a cada v�rtice de esta arista
            nodeA = nodes[thisEdge.indexA];
            nodeB = nodes[thisEdge.indexB];

            // Se crea un muelle con los dos nodos que conecta y su tipo
            InitializeSpring(nodeA, nodeB, thisEdge.springType);
        }
    }

    /// <summary>
    /// M�todo para calcular las aristas: primero las de tracci�n y luego las de
    /// felxi�n por cada arista de tracci�n repetida
    /// </summary>
    private void CalculateEdges()
    {
        // Primero se a�aden todas las de tracci�n a la lista de aristas
        GetTractionEdges();

        // Variables locales
        Edge thisEdge, nextEdge; // Arista en una iteraci�n y su siguiente
        List <Edge> flexionEdges = new(); // Lista de aristas de flexi�n

        edges.Sort();//Se ordenan ascendetemente seg�n el �ndice de su primer v�rtice

        // Cada arista de tracci�n
        for (int i = 0; i < edges.Count - 1; i++)
        {
            thisEdge = edges[i]; // Esta arista
            nextEdge = edges[i + 1]; // Siguiente arista

            // Si ambas tienen los mismos extremos
            if ((thisEdge.indexA == nextEdge.indexA) && (thisEdge.indexB == nextEdge.indexB))
            {
                // Elimina la siguiente ya que es la misma que esta
                edges.RemoveAt(i + 1);

                // Se crea una arista entre sus v�rtices opuestos que corresponder�a
                // a un muelle de flexi�n
                Edge flexionEdge = new(thisEdge.indexOther, nextEdge.indexOther,
                                        thisEdge.indexA, Spring.Type.Flexion);

                // A�ade la nueva arista de flexi�n a la lista
                flexionEdges.Add(flexionEdge);
            }
        }

        // A�ade las aristas de flexi�n a la lista de aristas global
        edges.AddRange(flexionEdges);
    }

    /// <summary>
    /// M�todo para calcular las aristas de tracci�n (naturales de la malla)
    /// </summary>
    private void GetTractionEdges()
    {
        // Variables locales
        int A, B, C; //V�rtices (�ndices)

        // Cada tri�ngulo: ser�an muelles de tracci�n
        for (int i = 0; i < triangles.Length - 2; i += 3)
        {
            // �ndices de los v�rtices del tri�ngulo
            A = triangles[i];
            B = triangles[i + 1];
            C = triangles[i + 2];

            // Una arista por cada combinaci�n diferente
            // Corresponder�an a muelles de tracci�n
            Edge tractionEdgeAB = new(A, B, C, Spring.Type.Traction);//BCD
            Edge tractionEdgeAC = new(A, C, B, Spring.Type.Traction);//BDC
            Edge tractionEdgeBC = new(B, C, A, Spring.Type.Traction);//CDB

            // Se a�aden a la lista de aristas
            edges.Add(tractionEdgeAB);
            edges.Add(tractionEdgeAC);
            edges.Add(tractionEdgeBC);
        }
    }

    /// <summary>
    /// M�todo para inicializar un muelle seg�n sus nodos y el tipo
    /// </summary>
    private void InitializeSpring(Node nodeA, Node nodeB, Spring.Type type)
    {
        // Calcula la posici�n del GameObject
        Vector3 springPos = (nodeA.pos + nodeB.pos) / 2f;
        // Crea el GameObject del muelle
        GameObject springObj = Instantiate(springPrefab, springPos, Quaternion.identity);
        // Lo hace hijo de este objeto
        springObj.transform.SetParent(transform);
        // Coje el c�digo de Spring
        Spring spring = springObj.GetComponent<Spring>();
        // Inicializa el muelle dados los dos nodos y la rigidez seg�n el tipo
        spring.Initialize(nodeA, nodeB, type, 
            type == Spring.Type.Traction ? tractionRigidity : flexionRigidity);
        // A�ade el muelle a la lista
        springs.Add(spring);
    }

    /// <summary>
    /// M�todo para actualizar los v�rtices de la malla a la par que los nodos
    /// </summary>
    private void CorrectMesh()
    {
        // Correcci�n de rotaci�n
        Quaternion rotCorrection = Quaternion.Inverse(transform.rotation);

        // Correcci�n de posici�n, seg�n el offset de cada eje
        Vector3 posCorrection = new (transform.position.x * Xoffset,
                                     transform.position.y * Yoffset,
                                     transform.position.z * Zoffset);

        // Cada nodo
        for (int i = 0; i < nodes.Count; i++)
        {
            // Actualiza la posici�n del v�rtice con las correcciones
            vertices[i] = rotCorrection * nodes[i].pos + posCorrection;
        }

        // Actualiza la lista de v�rtices de la malla
        mesh.vertices = vertices;
    }

    /// <summary>
    /// M�todo para actualizar la masa de los nodos y la rigidez de los muelles si ha
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
        // Ha cambiado si su rigidez no es ni la de tracci�n ni la de flexi�n
        if (springs[0].k != tractionRigidity || springs[0].k != flexionRigidity) 
        {
            //Cada muelle
            foreach (Spring spring in springs)
            {
                // Comprueba su tipo
                if (spring.type == Spring.Type.Traction)
                {
                    // Actualiza la rigidez de tracci�n
                    spring.k = tractionRigidity;
                }
                else
                {
                    // Actualiza la rigidez de flexi�n
                    spring.k = flexionRigidity;
                }
            }
        }
    }
}