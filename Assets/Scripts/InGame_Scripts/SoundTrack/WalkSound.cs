//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Marián Navarro
// Nombre del juego
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using System.Security.Cryptography;
using UnityEngine;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class WalkSound : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    [Header("Referencias")]
    [SerializeField] private PlayerMovement _playerMovement;

    [Header("Configuración de Pasos")]
    [SerializeField] private AudioClip _walkingClip;
    [Range(0, 2)][SerializeField] private float _pitchVariation = 0.1f;

    private AudioSource _audioSource;
    private float _basePitch;


    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    // Por defecto están los típicos (Update y Start) pero:
    // - Hay que añadir todos los que sean necesarios
    // - Hay que borrar los que no se usen 

    /// <summary>
    /// Start is called on the frame when a script is enabled just before 
    /// any of the Update methods are called the first time.
    /// </summary>
    /// 

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        // Configuración automática del AudioSource
        _audioSource.clip = _walkingClip;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _basePitch = _audioSource.pitch;

        // Intentar buscar el movimiento si no se asignó en el inspector
        if (_playerMovement == null)
            _playerMovement = GetComponent<PlayerMovement>();
    }
    void Start()
    {
        
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (_playerMovement == null) return;

        // Accedemos a la velocidad del Rigidbody a través del PlayerMovement
        // o directamente si están en el mismo objeto.
        bool isMoving = _playerMovement.GetComponent<Rigidbody2D>().linearVelocity.magnitude > 0.1f;

        // Lógica de reproducción
        if (isMoving && !_audioSource.isPlaying)
        {
            // Variamos ligeramente el pitch para que no sea monótono
            _audioSource.pitch = _basePitch + Random.Range(-_pitchVariation, _pitchVariation);
            _audioSource.Play();
        }
        else if (!isMoving && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

    }
    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Documentar cada método que aparece aquí con ///<summary>
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)
    // Ejemplo: GetPlayerController

    #endregion
    
    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    #endregion   

} // class WalkSound 
// namespace
