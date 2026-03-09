//---------------------------------------------------------
// Gestor de escena. Un LevelManager por cada escena de juego.
// Gestiona todo lo local: panel de muerte, pausa, reinicio,
// y guardado de checkpoint al completar la planta.
// Guillermo Jiménez Díaz, Pedro P. Gómez Martín — Template-P1
// Alexia Pérez Santana — No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton local de escena (sin DontDestroyOnLoad).
/// Responsabilidades:
///   · Mostrar/ocultar el panel de muerte cuando el jugador muere.
///   · Pausar y reanudar el juego (Time.timeScale).
///   · Ejecutar reinicio del nivel o vuelta al menú desde los botones del panel.
///   · Restaurar el estado del jugador desde el checkpoint al iniciar la escena.
///   · Guardar el checkpoint en GameManager al completar la planta.
///
/// Los botones del panelDeath deben apuntar a ESTE componente,
/// que siempre existe en la escena y nunca es una referencia rota.
/// </summary>
public class LevelManager : MonoBehaviour
{
    // ---- SINGLETON LOCAL ----
    #region Singleton local de escena

    private static LevelManager _instance;

    /// <summary>Acceso global al LevelManager de la escena activa.</summary>
    public static LevelManager Instance
    {
        get
        {
            Debug.Assert(_instance != null, "[LevelManager] No hay instancia en esta escena.");
            return _instance;
        }
    }

    /// <summary>Devuelve true si hay un LevelManager activo en la escena.</summary>
    public static bool HasInstance() => _instance != null;

    protected void Awake()
    {
        if (_instance != null)
        {
            Debug.LogWarning("[LevelManager] Duplicado detectado. Solo debe haber un LevelManager por escena.");
            Destroy(gameObject);
            return;
        }
        _instance = this;
        Init();
    }

    protected void OnDestroy()
    {
        if (this == _instance)
            _instance = null;
    }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Panel de muerte")]
    [Tooltip("GameObject del panel de muerte de esta escena. " +
             "Los botones del panel deben apuntar a LevelManager, no a GameManager.")]
    [SerializeField] private GameObject panelDeath;

    [Header("Referencias al jugador")]
    [Tooltip("Componente Health del jugador en esta escena.")]
    [SerializeField] private Health playerHealth;

    [Tooltip("Componente Inventory del jugador en esta escena.")]
    [SerializeField] private Inventory playerInventory;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        // Asegurar que el panel está oculto y el juego corre al iniciar
        if (panelDeath != null)
            panelDeath.SetActive(false);

        Time.timeScale = 1f;

        // Restaurar estado del jugador desde el checkpoint guardado
        RestoreFromCheckpoint();
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS — LLAMADOS POR Health ----
    #region Métodos públicos — Muerte del jugador

    /// <summary>
    /// Muestra el panel de muerte y pausa el juego.
    /// Llamado por Health cuando la vida del jugador llega a 0.
    /// </summary>
    public void OnPlayerDeath()
    {
        if (panelDeath != null)
            panelDeath.SetActive(true);
        else
            Debug.LogWarning("[LevelManager] panelDeath no está asignado en el Inspector.");

        Time.timeScale = 0f;
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS — BOTONES DEL PANEL ----
    #region Métodos públicos — Botones del panelDeath

    /// <summary>
    /// Reinicia el nivel desde el último checkpoint.
    /// Conectar al botón "Try Again" / "Reintentar" del panelDeath.
    /// </summary>
    public void OnRestartButton()
    {
        Time.timeScale = 1f;

        if (GameManager.HasInstance())
            GameManager.Instance.RestartCurrentScene();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Vuelve al menú principal.
    /// Conectar al botón "Menú" del panelDeath.
    /// </summary>
    public void OnMenuButton()
    {
        Time.timeScale = 1f;

        if (GameManager.HasInstance())
            GameManager.Instance.GoToMainMenu();
        else
            SceneManager.LoadScene("Main menu");
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS — CHECKPOINT ----
    #region Métodos públicos — Checkpoint al completar la planta

    /// <summary>
    /// Guarda el estado actual del jugador y carga la siguiente escena.
    /// Llamado por LevelWin cuando el jugador activa el ascensor con los fusibles.
    /// </summary>
    public void CompleteLevel(string nextSceneName)
    {
        if (GameManager.HasInstance() && playerHealth != null && playerInventory != null)
        {
            GameManager.Instance.SaveCheckpoint(
                playerHealth.GetCurrentHealth(),
                playerInventory.GetBandageCount(),
                playerInventory.GetKeyCount()
            );
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void Init() { }

    /// <summary>
    /// Restaura vida e inventario del jugador al valor del último checkpoint.
    /// Se ejecuta al iniciar la escena, tanto en primera carga como al reiniciar.
    /// Si no hay GameManager (ejecución directa desde el editor), no hace nada.
    /// </summary>
    private void RestoreFromCheckpoint()
    {
        if (!GameManager.HasInstance()) return;
        if (playerHealth == null || playerInventory == null) return;

        playerHealth.SetHealthFromCheckpoint(GameManager.Instance.GetSavedHealth());
        playerInventory.SetBandagesFromCheckpoint(GameManager.Instance.GetSavedBandages());
        playerInventory.SetKeysFromCheckpoint(GameManager.Instance.GetSavedKeys());
    }

    #endregion

} // class LevelManager