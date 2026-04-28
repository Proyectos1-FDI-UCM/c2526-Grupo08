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
            Debug.LogWarning("[LevelManager] Duplicado detectado.");
            Destroy(gameObject);
            return;
        }
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

    [Header("Referencias al jugador")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Inventory playerInventory;

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour

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

    public void OnPlayerDeath()
    {
        if (panelDeath != null)
            panelDeath.SetActive(true);
        else
            Debug.LogWarning("[LevelManager] panelDeath no asignado en el Inspector.");
        Time.timeScale = 0f;
    }

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

    // ---- API PÚBLICA — NIVEL ----
    #region Nivel y checkpoint

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