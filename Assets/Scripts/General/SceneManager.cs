/******************************************************************************************
 * �LVARO MORENO GARC�A
 * GRADO EN DISE�O Y DESARROLLO DE VIDEOJUEGOS - ANIMACI�N 3D
 * Pr�ctica II-2
 * 
 * Clase SceneManager.cs: Controla la visualizaci�n de los fijadores de nodos. Estos se
 * reconocen con la etiqueta 'Fixer'. Al pulsar la tecla 'V' se interact�a/
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
        // Inicializaci�n de lista de fijadores
        fixers = GameObject.FindGameObjectsWithTag("Fixer");// Con todos los objetos que tengan
                                                            // la etiqueta Fixer (en la escena)

        ToggleFixersVisibility(); // Muestra o esconde los fijadores seg�n se quiera
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.V)) // Detectamos si se ha pulsado la tecla v
        {
            // La tecla v hace de "toggle" para mostrar los fixers
            showFixers = !showFixers;
        }

        ToggleFixersVisibility(); // Muestra o esconde los fijadores seg�n se quiera
    }

    /// <summary>
    /// M�todo para renderirar o no la malla de las gu�as
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
