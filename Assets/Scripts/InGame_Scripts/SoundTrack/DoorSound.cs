//---------------------------------------------------------
// Reproduce un sonido al abrir una puerta.
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Se adjunta al GameObject de la puerta.
/// Expone PlayOpenSound() para que Door.cs o SecretDoor.cs lo llamen
/// al abrir la puerta. El sonido solo se reproduce una vez (flag _hasPlayed).
/// </summary>
public class DoorSound : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Configuración de Sonido")]
    [Tooltip("AudioClip que se reproduce al abrir la puerta.")]
    [SerializeField] private AudioClip doorOpenSound;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>AudioSource del GameObject, cacheado en Awake.</summary>
    private AudioSource _audioSource;

    /// <summary>True si el sonido ya se reprodujo. Evita reproducirlo más de una vez.</summary>
    private bool _hasPlayed = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea el AudioSource y desactiva su reproducción automática al iniciar.
    /// </summary>
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource != null)
            _audioSource.playOnAwake = false;
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Reproduce el sonido de apertura de puerta una única vez.
    /// Llamado por Door.cs o SecretDoor.cs al abrir la puerta.
    /// </summary>
    public void PlayOpenSound()
    {
        if (_hasPlayed || doorOpenSound == null) return;

        _hasPlayed = true;
        AudioSource.PlayClipAtPoint(doorOpenSound, transform.position);
    }

    #endregion

} // class DoorSound
  // Marián Navarro Santoyo