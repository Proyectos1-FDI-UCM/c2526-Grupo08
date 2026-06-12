//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Marián Navarro
// Nombre del juego
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class FusibleSound : MonoBehaviour
{
    [SerializeField] private AudioClip collectSound;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    // Usamos esta bandera interna solo para evitar el ruido al cerrar el editor
    private bool _appIsRunning = false;



    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        _appIsRunning = true;
    }

    private void Update()
    {

    }
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void OnDisable()
    {
        
        if (_appIsRunning && gameObject.scene.isLoaded)
        {
            if (collectSound != null)
            {
                //Esto para que suene en 2D
                AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position, volume);
            }
        }
    }

    private void OnApplicationQuit()
    {
        _appIsRunning = false;
    }


    #endregion   

} // class FusibleSound 
// namespace
