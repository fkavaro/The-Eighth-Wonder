/******************************************************************************************
 * ÁLVARO MORENO GARCÍA
 * GRADO EN DISEÑO Y DESARROLLO DE VIDEOJUEGOS - ANIMACIÓN 3D
 * Práctica II-2
 * 
 * Clase SceneManager.cs: Controla la visualización de los fijadores de nodos. Estos se
 * reconocen con la etiqueta 'Fixer'. Al pulsar la tecla 'V' se interactúa/
 *****************************************************************************************/
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    // VARIABLES
    [Header("Fixers")]
    GameObject[] fixers; // Array que contiene todos los fijadores de nodos
    public bool showFixers = true; // Cambia de valor con la tecla 'V'

    // Start is called before the first frame update
    void Start()
    {
        // Inicialización de lista de fijadores
        fixers = GameObject.FindGameObjectsWithTag("Fixer");// Con todos los objetos que tengan
                                                            // la etiqueta Fixer (en la escena)

        ToggleFixersVisibility(); // Muestra o esconde los fijadores según se quiera
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.V)) // Detectamos si se ha pulsado la tecla v
        {
            // La tecla v hace de "toggle" para mostrar los fixers
            showFixers = !showFixers;
        }

        ToggleFixersVisibility(); // Muestra o esconde los fijadores según se quiera
    }

    /// <summary>
    /// Método para renderirar o no la malla de las guías
    /// </summary>
    private void ToggleFixersVisibility()
    {
        // Cambia los fixers
        foreach (GameObject fixer in fixers)
        {
            fixer.GetComponent<MeshRenderer>().enabled = showFixers;
        }
    }
}
