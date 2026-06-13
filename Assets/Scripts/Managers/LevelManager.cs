//---------------------------------------------------------
// Gestor de escena. Un LevelManager por cada escena de juego.
// Guillermo Jiménez Díaz, Pedro P. Gómez Martín — Template-P1
// Alexia Pérez Santana — No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton local de escena (sin DontDestroyOnLoad).
///
/// VERSIÓN LIMPIA: toda la lógica de pausa y mapa ha sido
/// eliminada de aquí. El PauseManager (UI Toolkit) es el
/// único responsable de pausar, mostrar el mapa y gestionar
/// la UI de pausa. El CanvasMapa legacy debe estar desactivado
/// en la jerarquía.
///
/// Este script solo gestiona:
///   · Muerte y victoria del jugador
///   · Transición de nivel / checkpoint
///   · Singleton local de escena
/// </summary>
public class LevelManager : MonoBehaviour
{
    // ---- SINGLETON ----
    #region Singleton

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

    /// <summary>True si hay un LevelManager activo en la escena.</summary>
    public static bool HasInstance() => _instance != null;

    /// <summary>Si ya existe una instancia en la escena, destruye este duplicado; si no, se establece como la instancia.</summary>
    protected void Awake()
    {
        if (_instance != null)
        {
            Debug.LogWarning("[LevelManager] Duplicado detectado.");
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>Limpia la referencia estática si esta instancia es la activa.</summary>
    protected void OnDestroy()
    {
        if (this == _instance) _instance = null;
    }

    #endregion

    // ---- INSPECTOR ----
    #region Inspector

    [Header("Paneles de estado")]
    [Tooltip("Panel de UI que se muestra cuando el jugador muere.")]
    [SerializeField] private GameObject panelDeath;

    [Tooltip("Panel de UI que se muestra cuando se derrota al jefe (final bueno).")]
    [SerializeField] private GameObject panelWin;

    [Header("Referencias al jugador")]
    [Tooltip("Componente Health del jugador, usado para restaurar y guardar la vida del checkpoint.")]
    [SerializeField] private Health playerHealth;

    [Tooltip("Componente Inventory del jugador, usado para restaurar y guardar vendas y llaves del checkpoint.")]
    [SerializeField] private Inventory playerInventory;

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour

    /// <summary>
    /// Oculta los paneles de muerte/victoria, restaura el timeScale normal
    /// y restaura al jugador desde el último checkpoint guardado.
    /// </summary>
    private void Start()
    {
        if (panelDeath != null) panelDeath.SetActive(false);
        if (panelWin != null) panelWin.SetActive(false);

        Time.timeScale = 1f;
        RestoreFromCheckpoint();
    }

    #endregion

    // ---- API PÚBLICA — STUB para compatibilidad con PauseManager ----
    #region Stubs de compatibilidad

    /// <summary>Llamado por PauseManager al pausar. Sin efecto — el PauseManager gestiona todo.</summary>
    public void OnGamePaused() { }

    /// <summary>Llamado por PauseManager al reanudar. Sin efecto — el PauseManager gestiona todo.</summary>
    public void OnGameResumed() { }

    #endregion

    // ---- API PÚBLICA — MUERTE Y VICTORIA ----
    #region Muerte y victoria

    /// <summary>
    /// Llamado cuando el jugador muere: muestra el panel de muerte y pausa el juego.
    /// </summary>
    public void OnPlayerDeath()
    {
        if (panelDeath != null)
            panelDeath.SetActive(true);
        else
            Debug.LogWarning("[LevelManager] panelDeath no asignado en el Inspector.");
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Llamado cuando el jefe es derrotado: muestra el panel de victoria y pausa el juego.
    /// </summary>
    public void OnBossDeath()
    {
        if (panelWin != null)
            panelWin.SetActive(true);
        else
            Debug.LogWarning("[LevelManager] panelWin no asignado en el Inspector.");
        Time.timeScale = 0f;
    }

    #endregion

    // ---- API PÚBLICA — BOTONES ----
    #region Botones

    /// <summary>
    /// Reanuda el tiempo y reinicia la escena activa (con los datos del
    /// último checkpoint), usando GameManager si existe o, en su defecto,
    /// recargando la escena directamente.
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
    /// Reanuda el tiempo y vuelve al menú principal, usando GameManager si
    /// existe o, en su defecto, cargando la escena "Menu" directamente.
    /// </summary>
    public void OnMenuButton()
    {
        Time.timeScale = 1f;
        if (GameManager.HasInstance())
            GameManager.Instance.GoToMainMenu();
        else
            SceneManager.LoadScene("Menu");
    }

    #endregion

    // ---- API PÚBLICA — NIVEL ----
    #region Nivel y checkpoint

    /// <summary>
    /// Guarda el estado actual del jugador (vida, vendas, llaves) como
    /// checkpoint en GameManager (si existe), reanuda el tiempo y carga
    /// la siguiente escena.
    /// </summary>
    /// <param name="nextSceneName">Nombre de la escena a cargar.</param>
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

    // ---- PRIVADOS ----
    #region Privados

    /// <summary>
    /// Restaura la vida, las vendas y las llaves del jugador a partir del
    /// último checkpoint guardado en GameManager, si existe y hay referencias
    /// al jugador asignadas.
    /// </summary>
    private void RestoreFromCheckpoint()
    {
        if (!GameManager.HasInstance()) { return; }
        if (playerHealth == null || playerInventory == null) { return; }

        playerHealth.SetHealthFromCheckpoint(GameManager.Instance.GetSavedHealth());
        playerInventory.SetBandagesFromCheckpoint(GameManager.Instance.GetSavedBandages());
        playerInventory.SetKeysFromCheckpoint(GameManager.Instance.GetSavedKeys());
    }

    #endregion

} // class LevelManager
  // Alexia Pérez Santana