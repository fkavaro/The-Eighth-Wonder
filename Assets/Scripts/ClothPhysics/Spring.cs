/******************************************************************************
 * ÁLVARO MORENO GARCÍA
 * GRADO EN DISEÑO Y DESARROLLO DE VIDEOJUEGOS - ANIMACIÓN 3D
 * Práctica II-Final
 * 
 * SCRIPT MODIFICADO A PARTIR DEL PROPORCIONADO EN EL AULA
 * Práctica II - básica II
 * 
 * Clase Spring.cs: Define las propiedades de los muelles elásticos
 *****************************************************************************/
using UnityEngine;

public class Spring : MonoBehaviour
{
    public float k = 100f; // Constante de rigidez del muelle (N/m)
    public float length0; // Longitud natural del muelle (ahí la fuerza elástica
                          // se anula)

    public float length; // Longitud del muelle en un momento dado
    public Vector3 pos; // Posición 3D del punto medio del muelle
    public Vector3 dir; // Vector unitario con la dirección del muelle que
                        // apunta de B a A

    public float defaultSize = 2f; // Longitud natural de los cilindros en
                                   // Unity (m)

    public Quaternion rotation; // Nos permitirá calcular la orientación del
                                // muelle

    public Node nodeA; // Primer extremo del muelle
    public Node nodeB; // Segundo extremo del muelle

    // d = damping (amortiguamiento)
    public float dDeformation = 10f; // Factor de amortiguamiento del muelle que
                                     // proyecta la velocidad relativa de los
                                     // nodos del muelle sobre la dirección del mismo
    
    // Lista enumerada con los tipos de muelle
    public enum Type
    {
        Traction = 0,
        Flexion = 1,
    }

    public Type type; 

    public void Initialize(Node _nodeA, Node _nodeB, Type _type, float _k)
    {
        nodeA = _nodeA;
        nodeB = _nodeB;
        type = _type;
        k = _k;
    }

    /// <summary>
    /// Dibuja una línea por muelle de un color según su tipo
    /// </summary>
    private void OnDrawGizmos()
    {
        // Si es de tracción: rojo
        if (type == Spring.Type.Traction)
        {
            // Le damos color a su gizmo
            Gizmos.color = Color.red;
            // Dibuja una linea entre sus extremos de ese color
            Gizmos.DrawLine(nodeA.transform.position,
                            nodeB.transform.position);

        }
        // Si es de flexión: azul
        else
        {
            // Le damos color a su gizmo
            Gizmos.color = Color.blue;
            // Dibuja una linea entre sus extremos de ese color
            Gizmos.DrawLine(nodeA.transform.position,
                            nodeB.transform.position);
        }
    }
    ////////////////////////////////////////////////////////////////////////////

    // Update is called once per frame
    void Update()
    {
        // El valor de "pos" se calcula en el script MassSpringCloth según el
        // método de integración. Aquí establecemos la transformada posición
        // del gameobject para que coincida con la posición calculada 
        transform.position = pos;

        // Modificamos también transformada de escala local del gameobject
        // para que el cilindro que representa el muelle conecte siempre los
        // dos nodos. Las componentes x y z no se modifican
        transform.localScale = new Vector3(transform.localScale.x, length / defaultSize, transform.localScale.z);

        // Giramos el cilindro que representa el muelle según la rotación
        // calculada en MassSpringCloth
        transform.rotation = rotation;
    }
}