//---------------------------------------------------------
// Reproduce un sonido al recoger un fusible.
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Se adjunta al prefab del fusible recolectable.
/// Cuando el objeto se desactiva (recogido por el jugador),
/// reproduce el clip de recogida usando PlayClipAtPoint para que
/// el sonido no se corte al desactivarse el GameObject.
/// </summary>
public class FusibleSound : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Configuración de Sonido")]
    [Tooltip("AudioClip que se reproduce al recoger el fusible.")]
    [SerializeField] private AudioClip collectSound;

    [Tooltip("Volumen del sonido de recogida (0 = silencio, 1 = máximo).")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>True mientras la aplicación está en ejecución. Evita reproducir sonido al cerrar el editor.</summary>
    private bool _appIsRunning = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>Marca que la aplicación está en ejecución.</summary>
    private void Start()
    {
        _appIsRunning = true;
    }

    /// <summary>Marca que la aplicación se está cerrando para evitar reproducir sonido al salir.</summary>
    private void OnApplicationQuit()
    {
        _appIsRunning = false;
    }

    /// <summary>
    /// Al desactivarse el objeto (fusible recogido), reproduce el sonido
    /// en 2D (desde la posición de la cámara principal) si la app sigue activa.
    /// </summary>
    private void OnDisable()
    {
        if (_appIsRunning && gameObject.scene.isLoaded && collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position, volume);
    }

    #endregion

} // class FusibleSound
  // Marián Navarro Santoyo