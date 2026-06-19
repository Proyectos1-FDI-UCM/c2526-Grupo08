//---------------------------------------------------------
// Reproduce un sonido de pasos en bucle mientras el jugador
// se está moviendo, con variación aleatoria de pitch.
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Reproduce el clip de pasos mientras el jugador se mueve.
/// Detecta si hay movimiento comprobando la velocidad del Rigidbody2D
/// del jugador cada frame.
/// El Rigidbody2D del jugador se cachea en Awake para no llamar
/// GetComponent en Update.
/// </summary>
public class WalkSound : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Referencias")]
    [Tooltip("Script de movimiento del jugador. Si se deja vacío se busca en el mismo GameObject.")]
    [SerializeField] private PlayerMovement _playerMovement;

    [Header("Configuración de Pasos")]
    [Tooltip("AudioClip del sonido de pasos.")]
    [SerializeField] private AudioClip _walkingClip;

    [Tooltip("Magnitud máxima de variación de pitch para evitar que el sonido sea monótono.")]
    [Range(0, 2)]
    [SerializeField] private float _pitchVariation = 0.1f;

    [Tooltip("Velocidad mínima del Rigidbody2D a partir de la cual se considera que el jugador se mueve.")]
    [SerializeField] private float _moveSpeedThreshold = 0.1f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>AudioSource del jugador, cacheado en Awake.</summary>
    private AudioSource _audioSource;

    /// <summary>Pitch original del AudioSource, guardado para restaurarlo.</summary>
    private float _basePitch;

    /// <summary>Rigidbody2D del jugador, cacheado en Awake para no llamar GetComponent en Update.</summary>
    private Rigidbody2D _rb;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea el AudioSource, el Rigidbody2D y el PlayerMovement.
    /// Configura el AudioSource para reproducción en bucle.
    /// </summary>
    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _walkingClip;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _basePitch = _audioSource.pitch;

        if (_playerMovement == null)
            _playerMovement = GetComponent<PlayerMovement>();

        if (_playerMovement != null)
            _rb = _playerMovement.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Cada frame, reproduce o detiene el sonido de pasos según si el jugador se mueve.
    /// </summary>
    void Update()
    {
        if (_rb == null) return;

        bool isMoving = _rb.linearVelocity.magnitude > _moveSpeedThreshold;

        if (isMoving && !_audioSource.isPlaying)
        {
            _audioSource.pitch = _basePitch + Random.Range(-_pitchVariation, _pitchVariation);
            _audioSource.Play();
        }
        else if (!isMoving && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    #endregion

} // class WalkSound
  // Marián Navarro Santoyo