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
public class HealthSound : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    [Header("Referencias")]
    [SerializeField] private Health _healthComponent;

    [Header("Configuración Sonido")]
    [SerializeField] private AudioClip _sonidoVendas;
    [Range(0.1f, 1f)][SerializeField] private float _volumenEfecto = 1f;

    private AudioSource _audioSource;
    private int _ultimaVida;


    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _sonidoVendas;
        _audioSource.loop = true; // Para que suene mientras dure la cura
        _audioSource.playOnAwake = false;
        _audioSource.volume = _volumenEfecto;

        if (_healthComponent == null)
            _healthComponent = GetComponent<Health>();
    }


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
    void Start()
    {
        if (_healthComponent != null)
            _ultimaVida = _healthComponent.GetCurrentHealth();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (_healthComponent == null) return;

        int vidaActual = _healthComponent.GetCurrentHealth();

        if (vidaActual > _ultimaVida)
        {
            
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
        }
        else
        {
            // Si la vida no sube, paramos el sonido de vendas
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }

        
        _ultimaVida = vidaActual;
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

} // class HealthSound 
// namespace
