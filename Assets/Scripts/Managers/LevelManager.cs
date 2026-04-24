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
/// CAMBIOS RESPECTO A LA VERSIÓN ANTERIOR:
///   · Update() comprueba null en _showMap antes de llamar WasPressedThisFrame()
///     → evita NullReferenceException cuando la acción no existe en el InputSystem.
///   · Añadidos MostrarMapaEnPausa() y OcultarMapaPausa():
///     el PauseManager los llama al pausar/reanudar para que el panelMap
///     siga siendo responsabilidad exclusiva del LevelManager pero sea
///     visible también durante la pausa (sin mover ni tocar panelMap a otra clase).
///   · MainCanvas recibe null-check en Update para no dar error si no está asignado.
/// </summary>
public class LevelManager : MonoBehaviour
{
    // ---- SINGLETON LOCAL ----
    #region Singleton local de escena

    private static LevelManager _instance;

    public static LevelManager Instance
    {
        get
        {
            Debug.Assert(_instance != null, "[LevelManager] No hay instancia en esta escena.");
            return _instance;
        }
    }

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
    [SerializeField] private GameObject panelDeath;
    [SerializeField] private GameObject panelWin;

    [Header("Panel del mapa")]
    [Tooltip("Panel del mapa (canvas legacy). Se muestra al pausar y al pulsar Tab.")]
    [SerializeField] private GameObject panelMap;

    [Header("HUD")]
    [SerializeField] private GameObject MainCanvas;

    [Header("Panel de los controles")]
    [SerializeField] private GameObject PanelControls;

    [Header("Referencias al jugador")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Inventory playerInventory;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    private InputAction _showMap;
    private bool _mapShown = false;

    /// <summary>
    /// True mientras el mapa está visible por la pausa (gestionado por PauseManager).
    /// Evita que la tecla Tab oculte un mapa que mostró la pausa.
    /// </summary>
    private bool _mapByPause = false;

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        _showMap = InputSystem.actions.FindAction("ShowMap");
        if (_showMap == null)
            Debug.LogWarning("[LevelManager] Acción 'ShowMap' no encontrada en el InputSystem.");

        if (panelDeath != null) panelDeath.SetActive(false);
        if (panelWin != null) panelWin.SetActive(false);
        if (panelMap != null) panelMap.SetActive(false);
        if (PanelControls != null) PanelControls.SetActive(false);

        Time.timeScale = 1f;
        RestoreFromCheckpoint();
    }

    private void Update()
    {
        // CORRECCIÓN: null-check en _showMap antes de WasPressedThisFrame
        if (_showMap == null) { return; }

        // Si el mapa está siendo mostrado por la pausa, Tab no debe ocultarlo
        if (_mapByPause) { return; }

        if (_showMap.WasPressedThisFrame())
        {
            _mapShown = !_mapShown;

            if (_mapShown)
            {
                if (panelMap != null) panelMap.SetActive(true);
                if (MainCanvas != null) MainCanvas.SetActive(false);
                Time.timeScale = 0f;
                Debug.Log("Mapa mostrado");
            }
            else
            {
                if (panelMap != null) panelMap.SetActive(false);
                if (MainCanvas != null) MainCanvas.SetActive(true);
                Time.timeScale = 1f;
                Debug.Log("Mapa ocultado");
            }
        }
    }

    #endregion

    // ---- API PÚBLICA — MAPA EN PAUSA ----
    #region API pública — Mapa en pausa (llamado por PauseManager)

    /// <summary>
    /// Muestra el panelMap durante la pausa. 
    /// Llamado por PauseManager al pausar.
    /// </summary>
    public void MostrarMapaEnPausa()
    {
        if (panelMap == null) { return; }
        _mapByPause = true;
        panelMap.SetActive(true);
        // No tocamos MainCanvas aquí; el overlay de pausa cubre la pantalla igualmente.
    }

    /// <summary>
    /// Oculta el panelMap cuando se reanuda desde la pausa.
    /// Llamado por PauseManager al reanudar.
    /// </summary>
    public void OcultarMapaPausa()
    {
        if (panelMap == null) { return; }
        _mapByPause = false;
        panelMap.SetActive(false);
    }

    #endregion

    // ---- API PÚBLICA — MUERTE Y WIN ----
    #region Métodos públicos — Muerte del jugador

    public void OnPlayerDeath()
    {
        if (panelDeath != null)
            panelDeath.SetActive(true);
        else
            Debug.LogWarning("[LevelManager] panelDeath no está asignado en el Inspector.");

        Time.timeScale = 0f;
    }

    public void OnBossDeath()
    {
        if (panelWin != null)
            panelWin.SetActive(true);
        else
            Debug.LogWarning("[LevelManager] panelWin no está asignado en el Inspector.");

        Time.timeScale = 0f;
    }

    #endregion

    // ---- API PÚBLICA — BOTONES DE PANEL ----
    #region Métodos públicos — Botones del panelDeath

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        if (GameManager.HasInstance())
            GameManager.Instance.RestartCurrentScene();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMenuButton()
    {
        Time.timeScale = 1f;
        if (GameManager.HasInstance())
            GameManager.Instance.GoToMainMenu();
        else
            SceneManager.LoadScene("Menu");
    }

    #endregion

    // ---- API PÚBLICA — CHECKPOINT ----
    #region Métodos públicos — Checkpoint

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

    public void HideMap()
    {
        if (panelMap != null) panelMap.SetActive(false);
        if (MainCanvas != null) MainCanvas.SetActive(true);
        Time.timeScale = 1f;
        _mapShown = false;
        _mapByPause = false;
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

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void Init() { }

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