//---------------------------------------------------------
// Gestor de escena. Un LevelManager por cada escena de juego.
// Guillermo Jiménez Díaz, Pedro P. Gómez Martín — Template-P1
// Alexia Pérez Santana — No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton local de escena (sin DontDestroyOnLoad).
///
/// CORRECCIÓN DEFINITIVA del bug de barras desapareciendo al reanudar pausa:
///   · ShowMap (Tab) y Pause (Escape) son acciones DISTINTAS. Si por error
///     estaban mapeadas a la misma tecla, al pausar también se activaba
///     el mapa, que desactivaba MainCanvas. Al reanudar, MainCanvas seguía oculto.
///   · Ahora: cuando la pausa está activa (_isPausedByManager = true),
///     Update() ignora completamente la acción ShowMap. El Canvas nunca
///     se toca durante la pausa — solo el overlay de UI Toolkit lo cubre.
///   · Al reanudar desde pausa, MainCanvas se fuerza a activo
///     independientemente del estado anterior.
/// </summary>
public class LevelManager : MonoBehaviour
{
    // ---- SINGLETON LOCAL ----
    #region Singleton

    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get { Debug.Assert(_instance != null, "[LevelManager] No hay instancia."); return _instance; }
    }
    public static bool HasInstance() => _instance != null;

    protected void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
    }

    protected void OnDestroy()
    {
        if (this == _instance) _instance = null;
    }

    #endregion

    // ---- INSPECTOR ----
    #region Inspector

    [Header("Paneles de estado")]
    [SerializeField] private GameObject panelDeath;
    [SerializeField] private GameObject panelWin;

    [Header("Panel del mapa (Tab)")]
    [Tooltip("Se muestra al pulsar Tab. Durante la pausa se ignora Tab.")]
    [SerializeField] private GameObject panelMap;

    [Header("HUD principal")]
    [Tooltip("Canvas con barras de vida, magia y minimapa. " +
             "NUNCA se desactiva durante la pausa — solo al abrir el mapa con Tab.")]
    [SerializeField] private GameObject MainCanvas;

    [Header("Panel de controles")]
    [SerializeField] private GameObject PanelControls;

    [Header("Referencias al jugador")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Inventory playerInventory;

    #endregion

    // ---- PRIVADOS ----
    #region Privados

    private InputAction _showMap;
    private bool _mapShown = false;
    // Flag que el PauseManager activa para que el mapa-Tab no interfiera
    private bool _isPausedByManager = false;

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour

    private void Start()
    {
        _showMap = InputSystem.actions?.FindAction("ShowMap");
        if (_showMap == null)
            Debug.LogWarning("[LevelManager] Acción 'ShowMap' no encontrada.");

        if (panelDeath != null) panelDeath.SetActive(false);
        if (panelWin != null) panelWin.SetActive(false);
        if (panelMap != null) panelMap.SetActive(false);
        if (PanelControls != null) PanelControls.SetActive(false);

        // HUD siempre visible al iniciar
        if (MainCanvas != null) MainCanvas.SetActive(true);

        Time.timeScale = 1f;
        RestoreFromCheckpoint();
    }

    private void Update()
    {
        if (_showMap == null) { return; }
        // Durante la pausa, Tab no hace nada — el PauseManager controla la UI
        if (_isPausedByManager) { return; }

        if (_showMap.WasPressedThisFrame())
        {
            _mapShown = !_mapShown;

            if (_mapShown)
            {
                if (panelMap != null) panelMap.SetActive(true);
                if (MainCanvas != null) MainCanvas.SetActive(false);
                Time.timeScale = 0f;
            }
            else
            {
                if (panelMap != null) panelMap.SetActive(false);
                if (MainCanvas != null) MainCanvas.SetActive(true);
                Time.timeScale = 1f;
            }
        }
    }

    #endregion

    // ---- API — PAUSA (llamado por PauseManager) ----
    #region API pausa

    /// <summary>
    /// Llamado por PauseManager.Pausar().
    /// Bloquea Tab y garantiza que el HUD esté visible bajo el overlay.
    /// </summary>
    public void OnGamePaused()
    {
        _isPausedByManager = true;
        // El HUD puede quedarse activo — el overlay de pausa lo cubre igualmente
        // NO tocamos MainCanvas aquí
    }

    /// <summary>
    /// Llamado por PauseManager.Reanudar().
    /// Reactiva el HUD y desbloquea Tab.
    /// </summary>
    public void OnGameResumed()
    {
        _isPausedByManager = false;
        _mapShown = false;
        // Forzar el HUD visible por si algo lo había ocultado
        if (MainCanvas != null) MainCanvas.SetActive(true);
        if (panelMap != null) panelMap.SetActive(false);
    }

    // Mantener compatibilidad con versiones anteriores del PauseManager
    public void MostrarMapaEnPausa() => OnGamePaused();
    public void OcultarMapaPausa() => OnGameResumed();

    #endregion

    // ---- API — MUERTE / WIN ----
    #region Muerte y victoria

    public void OnPlayerDeath()
    {
        if (panelDeath != null) panelDeath.SetActive(true);
        else Debug.LogWarning("[LevelManager] panelDeath no asignado.");
        Time.timeScale = 0f;
    }

    public void OnBossDeath()
    {
        if (panelWin != null) panelWin.SetActive(true);
        else Debug.LogWarning("[LevelManager] panelWin no asignado.");
        Time.timeScale = 0f;
    }

    #endregion

    // ---- API — BOTONES ----
    #region Botones

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        if (GameManager.HasInstance()) GameManager.Instance.RestartCurrentScene();
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMenuButton()
    {
        Time.timeScale = 1f;
        if (GameManager.HasInstance()) GameManager.Instance.GoToMainMenu();
        else SceneManager.LoadScene("Menu");
    }

    #endregion

    // ---- API — NIVEL / CHECKPOINT ----
    #region Nivel y checkpoint

    public void CompleteLevel(string nextSceneName)
    {
        if (GameManager.HasInstance() && playerHealth != null && playerInventory != null)
        {
            GameManager.Instance.SaveCheckpoint(
                playerHealth.GetCurrentHealth(),
                playerInventory.GetBandageCount(),
                playerInventory.GetKeyCount());
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    public void HideMap()
    {
        if (panelMap != null) panelMap.SetActive(false);
        if (MainCanvas != null) MainCanvas.SetActive(true);
        Time.timeScale = 1f;
        _mapShown = false;
        _isPausedByManager = false;
    }

    public void ShowControls()
    {
        if (PanelControls != null) PanelControls.SetActive(true);
        if (panelMap != null) panelMap.SetActive(false);
    }

    public void ReturnToPauseMenu()
    {
        if (PanelControls != null) PanelControls.SetActive(false);
        if (panelMap != null) panelMap.SetActive(true);
    }

    #endregion

    // ---- PRIVADOS ----
    #region Privados

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