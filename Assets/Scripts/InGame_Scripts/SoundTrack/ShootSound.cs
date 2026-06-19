//---------------------------------------------------------
// Reproduce el sonido de disparo cuando el jugador dispara.
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reproduce el AudioClip de disparo cada vez que el jugador
/// activa la acción "Attack" y el cooldown lo permite.
/// El sonido se reproduce con PlayOneShot para que los disparos
/// rápidos se superpongan de forma natural.
/// </summary>
public class ShootSound : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Audio")]
    [Tooltip("AudioSource desde el que se reproduce el sonido de disparo.")]
    [SerializeField] private AudioSource _audioSource;

    [Tooltip("AudioClip del sonido de disparo.")]
    [SerializeField] private AudioClip _sfxDisparo;

    [Tooltip("Volumen del sonido de disparo (0 = silencio, 1 = máximo).")]
    [SerializeField, Range(0, 1)] private float _volumen = 0.7f;

    [Header("Cadencia")]
    [Tooltip("Tiempo mínimo en segundos entre disparos (debe coincidir con PlayerShoot.FireRate).")]
    [SerializeField] private float _fireRate = 0.2f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Instante (Time.time) en el que se podrá disparar de nuevo.</summary>
    private float _nextFireTime = 0f;

    /// <summary>Acción de Input System para disparar.</summary>
    private InputAction _attackAction;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Obtiene la acción "Attack" del Input System.
    /// </summary>
    private void Start()
    {
        _attackAction = InputSystem.actions.FindAction("Attack");
        if (_attackAction == null)
        {
            Debug.LogError("[ShootSound] Acción 'Attack' no encontrada.");
            enabled = false;
        }
    }

    /// <summary>
    /// Cada frame, reproduce el sonido de disparo si la acción está activa
    /// y ha pasado el tiempo de cadencia.
    /// </summary>
    private void Update()
    {
        if (_attackAction.IsInProgress() && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _fireRate;
            PlayShootSound();
        }
    }

    /// <summary>
    /// Detiene el AudioSource al destruirse el GameObject (cambio de escena).
    /// </summary>
    private void OnDestroy()
    {
        if (_audioSource != null)
            _audioSource.Stop();
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>Reproduce el sonido de disparo con PlayOneShot.</summary>
    private void PlayShootSound()
    {
        if (_audioSource != null && _sfxDisparo != null)
            _audioSource.PlayOneShot(_sfxDisparo, _volumen);
    }

    #endregion

} // class ShootSound
  // Marián Navarro Santoyo