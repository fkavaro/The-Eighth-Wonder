/******************************************************************************
 * �LVARO MORENO GARC�A
 * GRADO EN DISE�O Y DESARROLLO DE VIDEOJUEGOS - ANIMACI�N 3D
 * Pr�ctica II-Final
 * 
 * SCRIPT MODIFICADO A PARTIR DEL PROPORCIONADO EN EL AULA
 * Pr�ctica II - b�sica II
 * 
 * Clase Spring.cs: Define las propiedades de los muelles el�sticos
 *****************************************************************************/
using UnityEngine;

public class Spring : MonoBehaviour
{
    public float k = 100f; // Constante de rigidez del muelle (N/m)
    public float length0; // Longitud natural del muelle (ah� la fuerza el�stica
                          // se anula)

    public float length; // Longitud del muelle en un momento dado
    public Vector3 pos; // Posici�n 3D del punto medio del muelle
    public Vector3 dir; // Vector unitario con la direcci�n del muelle que
                        // apunta de B a A

    public float defaultSize = 2f; // Longitud natural de los cilindros en
                                   // Unity (m)

    public Quaternion rotation; // Nos permitir� calcular la orientaci�n del
                                // muelle

    public Node nodeA; // Primer extremo del muelle
    public Node nodeB; // Segundo extremo del muelle

    // d = damping (amortiguamiento)
    public float dDeformation = 10f; // Factor de amortiguamiento del muelle que
                                     // proyecta la velocidad relativa de los
                                     // nodos del muelle sobre la direcci�n del mismo
    
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
    /// Dibuja una l�nea por muelle de un color seg�n su tipo
    /// </summary>
    private void OnDrawGizmos()
    {
        // Si es de tracci�n: rojo
        if (type == Spring.Type.Traction)
        {
            // Le damos color a su gizmo
            Gizmos.color = Color.red;
            // Dibuja una linea entre sus extremos de ese color
            Gizmos.DrawLine(nodeA.transform.position,
                            nodeB.transform.position);

        }
        // Si es de flexi�n: azul
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
        // El valor de "pos" se calcula en el script MassSpringCloth seg�n el
        // m�todo de integraci�n. Aqu� establecemos la transformada posici�n
        // del gameobject para que coincida con la posici�n calculada 
        transform.position = pos;

        // Modificamos tambi�n transformada de escala local del gameobject
        // para que el cilindro que representa el muelle conecte siempre los
        // dos nodos. Las componentes x y z no se modifican
        transform.localScale = new Vector3(transform.localScale.x, length / defaultSize, transform.localScale.z);

        // Giramos el cilindro que representa el muelle seg�n la rotaci�n
        // calculada en MassSpringCloth
        transform.rotation = rotation;
    }
}