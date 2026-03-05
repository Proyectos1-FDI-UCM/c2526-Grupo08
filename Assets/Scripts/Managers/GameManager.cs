//---------------------------------------------------------
// Contiene el componente GameManager
// Guillermo Jiménez Díaz, Pedro P. Gómez Martín
// Marco A. Gómez Martín
// Template-P1
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Componente responsable de la gestión global del juego. Es un singleton
/// que orquesta el funcionamiento general de la aplicación,
/// sirviendo de comunicación entre las escenas.
///
/// El GameManager ha de sobrevivir entre escenas por lo que hace uso del
/// DontDestroyOnLoad. En caso de usarlo, cada escena debería tener su propio
/// GameManager para evitar problemas al usarlo. Además, se debería producir
/// un intercambio de información entre los GameManager de distintas escenas.
/// Generalmente, esta información debería estar en un LevelManager o similar.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----

    #region Atributos del Inspector (serialized fields)

    [SerializeField] private GameObject panelDeath;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----

    #region Atributos Privados (private fields)

    /// <summary>
    /// Instancia única de la clase (singleton).
    /// </summary>
    private static GameManager _instance;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----

    #region Métodos de MonoBehaviour

    /// <summary>
    /// Método llamado en un momento temprano de la inicialización.
    /// En el momento de la carga, si ya hay otra instancia creada,
    /// nos destruimos (al GameObject completo)
    /// </summary>
    protected void Awake()
    {
        if (_instance != null)
        {
            // No somos la primera instancia. Transferimos la referencia al panelDeath
            // de la nueva escena al GameManager que sobrevive.
            TransferManagerSetup();

            DestroyImmediate(this.gameObject);
        }
        else
        {
            // Somos el primer GameManager.
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            Init();
        }
    }

    /// <summary>
    /// Método llamado cuando se destruye el componente.
    /// </summary>
    protected void OnDestroy()
    {
        if (this == _instance)
        {
            _instance = null;
        }
    }

    private void Start()
    {
        // Aseguramos que el panel esté oculto al inicio de cada escena
        HidePanelDeath();
        // Por si la escena anterior dejó el tiempo pausado (ej: muerte)
        Time.timeScale = 1f;
    }

    private void Update()
    {
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----

    #region Métodos públicos

    /// <summary>
    /// Propiedad para acceder a la única instancia de la clase.
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            Debug.Assert(_instance != null);
            return _instance;
        }
    }

    /// <summary>
    /// Devuelve cierto si la instancia del singleton está creada y
    /// falso en otro caso.
    /// </summary>
    public static bool HasInstance()
    {
        return _instance != null;
    }

    /// <summary>
    /// Método que cambia la escena actual por la indicada en el parámetro.
    /// </summary>
    /// <param name="index">Índice de la escena (en el build settings) que se cargará.</param>
    public void ChangeScene(int index)
    {
        System.GC.Collect();
        SceneManager.LoadScene(index);
        System.GC.Collect();
    }

    /// <summary>
    /// Reinicia el nivel actual. Llamado desde el botón "Reintentar" del panelDeath.
    /// Restaura el timeScale por si el juego estaba pausado al morir.
    /// </summary>
    public void RestartLevelifLose()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Vuelve al menú principal. Llamado desde el botón "Menú" del panelDeath.
    /// Restaura el timeScale por si el juego estaba pausado al morir.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main menu");
    }

    /// <summary>
    /// Actualiza el estado de la GUI según la vida actual del jugador.
    /// Si la vida llega a 0 muestra el panel de muerte y pausa el juego.
    /// </summary>
    /// <param name="currentHealth">Vida actual del jugador.</param>
    public void UpdateGUI(int currentHealth)
    {
        if (currentHealth <= 0)
        {
            ShowPanelDeath();
        }
    }

    /// <summary>
    /// Permite a otros componentes asignar el panel de muerte desde la escena activa.
    /// Útil cuando el GameManager ya existe (DontDestroyOnLoad) y se carga una nueva escena.
    /// </summary>
    /// <param name="panel">El GameObject del panel de muerte de la nueva escena.</param>
    public void SetPanelDeath(GameObject panel)
    {
        panelDeath = panel;
        HidePanelDeath();
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----

    #region Métodos Privados

    /// <summary>
    /// Muestra el panel de muerte y pausa el tiempo del juego.
    /// </summary>
    private void ShowPanelDeath()
    {
        if (panelDeath != null)
        {
            panelDeath.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[GameManager] panelDeath es null. Asegúrate de asignarlo en el Inspector o llamar a SetPanelDeath().");
        }
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Oculta el panel de muerte.
    /// </summary>
    private void HidePanelDeath()
    {
        if (panelDeath != null)
        {
            panelDeath.SetActive(false);
        }
    }

    /// <summary>
    /// Dispara la inicialización.
    /// </summary>
    private void Init()
    {
        // De momento no hay nada que inicializar
    }

    /// <summary>
    /// Transfiere la referencia al panelDeath de la nueva escena al GameManager persistente.
    /// Se llama cuando se detecta que ya existe una instancia y el nuevo GameManager
    /// (de la escena recién cargada) contiene la referencia actualizada al panel.
    /// </summary>
    private void TransferManagerSetup()
    {
        _instance.panelDeath = this.panelDeath;
    }

    #endregion

} // class GameManager