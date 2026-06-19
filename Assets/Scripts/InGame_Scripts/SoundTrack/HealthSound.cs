//---------------------------------------------------------
// Reproduce un sonido al recoger/usar una venda (curación).
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Se adjunta al prefab de la venda o al jugador.
/// Reproduce el clip de curación cuando el objeto es destruido
/// (al ser usado), usando PlayClipAtPoint para que el sonido
/// no se corte al destruirse el GameObject.
/// </summary>
public class HealthSound : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Configuración de Sonido")]
    [Tooltip("AudioClip que se reproduce al curar al jugador.")]
    [SerializeField] private AudioClip healClip;

    [Tooltip("Volumen del sonido de curación (0 = silencio, 10 = máximo).")]
    [SerializeField, Range(0, 10)] private float volume = 10f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>True mientras la aplicación está en ejecución. Evita ruido de sonido al cerrar el editor.</summary>
    private bool _isQuitting = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>Marca que la aplicación se está cerrando para evitar reproducir el sonido al salir.</summary>
    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    /// <summary>
    /// Al destruirse el objeto (venda usada), reproduce el sonido de curación
    /// en la posición del objeto si la aplicación sigue activa.
    /// </summary>
    private void OnDestroy()
    {
        if (!_isQuitting && healClip != null)
            AudioSource.PlayClipAtPoint(healClip, transform.position, volume);
    }

    #endregion

} // class HealthSound
  // Marián Navarro Santoyo