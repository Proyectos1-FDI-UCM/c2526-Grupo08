//---------------------------------------------------------
// Gestiona la música de fondo del juego entre escenas.
// Singleton persistente (DontDestroyOnLoad) que cambia el clip
// de audio según la escena activa.
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton persistente que controla la música de fondo.
/// Detecta la escena activa cada frame y cambia el AudioClip
/// cuando la escena cambia, sin interrumpir el audio entre escenas
/// si el clip es el mismo.
/// </summary>
public class SoundFondo : MonoBehaviour
{
    // ---- SINGLETON ----
    #region Singleton

    private static SoundFondo _instance;

    /// <summary>Acceso global a la instancia única.</summary>
    public static SoundFondo Instance => _instance;

    /// <summary>True si hay una instancia activa.</summary>
    public static bool HasInstance() => _instance != null;

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Clips por escena")]
    [Tooltip("Música del menú principal y créditos.")]
    [SerializeField] private AudioClip clipMenuCreditos;

    [Tooltip("Música de los niveles 1 y 2.")]
    [SerializeField] private AudioClip clipNivel1y2;

    [Tooltip("Música del nivel del jefe (Level_Boss).")]
    [SerializeField] private AudioClip clipNivel3;

    #endregion

    // ---- CONSTANTES ----
    #region Constantes

    /// <summary>Nombre de la escena del menú principal.</summary>
    private const string SceneMenu = "Menu";

    /// <summary>Nombre de la escena del nivel 1.</summary>
    private const string SceneLevel1 = "Level_1";

    /// <summary>Nombre de la escena del nivel 2.</summary>
    private const string SceneLevel2 = "Level_2";

    /// <summary>Nombre de la escena del jefe.</summary>
    private const string SceneBoss = "Level_Boss";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>AudioSource que reproduce la música de fondo.</summary>
    private AudioSource _source;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Inicializa el singleton persistente entre escenas y cachea el AudioSource.
    /// Si ya existe una instancia, destruye este duplicado.
    /// </summary>
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _source = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Cada frame, comprueba si la escena activa ha cambiado y actualiza
    /// el clip de música si es necesario.
    /// </summary>
    void Update()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        AudioClip clipDeseado = null;

        if (escenaActual == SceneMenu)
            clipDeseado = clipMenuCreditos;
        else if (escenaActual == SceneLevel1 || escenaActual == SceneLevel2)
            clipDeseado = clipNivel1y2;
        else if (escenaActual == SceneBoss)
            clipDeseado = clipNivel3;

        if (clipDeseado != null && _source.clip != clipDeseado)
        {
            _source.clip = clipDeseado;
            _source.Play();
        }
    }

    #endregion

} // class SoundFondo
  // Marián Navarro Santoyo