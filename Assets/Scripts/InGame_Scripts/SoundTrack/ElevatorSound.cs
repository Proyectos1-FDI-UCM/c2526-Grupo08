//---------------------------------------------------------
// Reproduce un sonido al activar el ascensor (al descargar la escena).
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Se adjunta al GameObject del ascensor.
/// Persiste entre escenas (DontDestroyOnLoad) y se suscribe al evento
/// sceneUnloaded: cuando la escena actual se descarga (el jugador sube
/// al siguiente nivel), reproduce el clip de apertura del ascensor.
/// El sonido solo se reproduce una vez por instancia.
/// </summary>
public class ElevatorSound : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Configuración")]
    [Tooltip("AudioClip que se reproduce al activar el ascensor (al cambiar de escena).")]
    [SerializeField] private AudioClip openSound;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>AudioSource del ascensor, cacheado en Awake.</summary>
    private AudioSource _audioSource;

    /// <summary>True si el sonido ya se reprodujo. Evita repetirlo.</summary>
    private bool _hasPlayed = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea el AudioSource y marca el objeto como persistente entre escenas.
    /// </summary>
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Se suscribe al evento de descarga de escena al activarse.</summary>
    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    /// <summary>Se desuscribe del evento al desactivarse.</summary>
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Al descargarse la escena actual, reproduce el sonido de ascensor
    /// si no se ha reproducido ya.
    /// </summary>
    private void OnSceneUnloaded(Scene current)
    {
        if (!_hasPlayed && openSound != null)
        {
            _hasPlayed = true;
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
    }

    #endregion

} // class ElevatorSound
  // Marián Navarro Santoyo