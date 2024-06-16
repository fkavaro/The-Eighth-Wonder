/******************************************************************************
 * ÁLVARO MORENO GARCÍA
 * GRADO EN DISEÑO Y DESARROLLO DE VIDEOJUEGOS - ANIMACIÓN 3D
 * Práctica II-Final
 * 
 * SCRIPT MODIFICADO A PARTIR DEL PROPORCIONADO EN EL AULA
 * Práctica II - básica II
 * 
 * Clase Node.cs: Define las propiedades de los nodos (masas puntuales en los 
 * extremos de cada muelle)
 *****************************************************************************/
using UnityEngine;

public class Node : MonoBehaviour
{
    public float mass = 5f; // Masa del nodo (kg)
    public bool fixedNode;  // Indica si es un nodo fijo (true) o si puede
                            // moverse (false)

    public Vector3 pos; // Posicion 3D del nodo
    public Vector3 vel; // Velocidad 3D del nodo
    public Vector3 force; // Fuerza 3D que sufre el nodo

    // d = damping (amortiguamiento)
    public float dAbsolute = 0.1f; // Factor de amortiguamiento del nodo
                                   // proporcional a su velocidad absoluta

    bool definedFixed = false; // No se ha fijado inicialmente
    public float rad = 0.01f; // Radio del gizmo esfera

    // Use this for initialization
    public void Start()
    {
        pos = transform.position; // Establecemos en el instante inicial el
                                  // valor de la posicion "pos" a partir de la
                                  // transformada position del gameobject

        // Se define que se ha fijado en el inicio
        definedFixed = true;
    }

    // Update is called once per frame
    void Update()
    {
        // El valor de "pos" se calcula en el script MassSpringCloth segun el
        // metodo de integracion. Aqui establecemos la transformada posicion
        // del gameobject para que coincida con la posicion calculada
        transform.position = pos;
    }

    // Inicializa el nodo dada posición y masa
    public void Initialize(Vector3 _pos, float _mass)
    {
        pos = _pos;
        mass = _mass;
    }

    // Es un nodo fijo si está en el trigger de un objeto con el tag Fixer
    // Funciona porque los nodos tienen otro colisionador y un Rigidbod
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fixer") && !definedFixed)
        {
            fixedNode = true;
            definedFixed = true;    
        }
    }

    /// <summary>
    /// Dibuja una esfera
    /// </summary>
    private void OnDrawGizmos()
    {
        // Le damos color a su gizmo
        Gizmos.color = Color.green;
        // Dibuja una esfera en su posición
        Gizmos.DrawSphere(pos, rad);
    }
}