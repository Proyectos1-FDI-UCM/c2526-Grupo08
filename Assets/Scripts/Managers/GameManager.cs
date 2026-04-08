//---------------------------------------------------------
// Gestor global del juego. Singleton persistente entre escenas.
// Responsabilidad única: almacenar y transferir datos de estado
// entre escenas (checkpoints, desbloqueos, ajustes de usuario).
// NO gestiona UI, paneles ni lógica de escena — eso es LevelManager.
// Guillermo Jiménez Díaz, Pedro P. Gómez Martín — Template-P1
// Alexia Pérez Santana — No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton persistente entre escenas (DontDestroyOnLoad).
/// Almacena datos que necesitan viajar entre escenas:
///   · Vida del jugador en el último checkpoint.
///   · Inventario en el último checkpoint (vendas, llaves).
///   · Desbloqueos permanentes (disparo multidireccional, etc.).
///   · Ajustes de usuario: intensidad de shake y follow delay de cámara.
///
/// Navegación entre escenas: los métodos de carga de escena
/// también viven aquí para centralizar el uso de SceneManager.
///
/// CameraController lee CameraShakeIntensity y CameraFollowDelay en cada frame
/// para reflejar los cambios del menú de ajustes sin reiniciar la escena.
///
/// Todo lo relacionado con UI de escena (panel de muerte, pausa)
/// pertenece a LevelManager, que vive solo en su escena.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ---- SINGLETON ----
    #region Singleton

    private static GameManager _instance;

    /// <summary>Acceso global a la instancia única.</summary>
    public static GameManager Instance
    {
        get
        {
            Debug.Assert(_instance != null, "[GameManager] No hay instancia activa.");
            return _instance;
        }
    }

    /// <summary>
    /// Devuelve true si el singleton está inicializado.
    /// Usar antes de acceder a Instance para evitar errores al cerrar la app.
    /// </summary>
    public static bool HasInstance() => _instance != null;

    protected void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }

    protected void OnDestroy()
    {
        if (this == _instance)
            _instance = null;
    }

    #endregion

    // ---- DATOS PERSISTENTES — CHECKPOINT ----
    #region Datos persistentes entre escenas — Checkpoint

    /// <summary>Vida del jugador en el último checkpoint.</summary>
    private int _savedHealth = 200;

    /// <summary>Vendas en el último checkpoint.</summary>
    private int _savedBandages = 0;

    /// <summary>Llaves en el último checkpoint.</summary>
    private int _savedKeys = 0;

    /// <summary>Si el jugador tiene desbloqueado el disparo multidireccional.</summary>
    private bool _hasMultishot = false;

    #endregion

    // ---- AJUSTES DE USUARIO — CÁMARA ----
    #region Ajustes de usuario — Cámara

    /// <summary>
    /// Intensidad del efecto de temblor de cámara.
    /// 0 = sin temblor, 1 = intensidad máxima.
    /// CameraController multiplica su _shakeIntensity base por este valor.
    /// Modificado desde SettingsMenu y persistente entre escenas.
    /// </summary>
    public float CameraShakeIntensity = 1f;

    /// <summary>
    /// Retraso del seguimiento de cámara en segundos.
    /// CameraController usa este valor como smoothTime en SmoothDamp.
    /// Modificado desde SettingsMenu y persistente entre escenas.
    /// </summary>
    public float CameraFollowDelay = 0.5f;

    #endregion

    // ---- MÉTODOS PÚBLICOS — NAVEGACIÓN ----
    #region Métodos públicos — Navegación

    /// <summary>Carga una escena por índice (Build Settings).</summary>
    public void ChangeScene(int index)
    {
        System.GC.Collect();
        SceneManager.LoadScene(index);
        System.GC.Collect();
    }

    /// <summary>Carga una escena por nombre.</summary>
    public void ChangeScene(string sceneName)
    {
        System.GC.Collect();
        SceneManager.LoadScene(sceneName);
        System.GC.Collect();
    }

    /// <summary>
    /// Reinicia la escena activa. El jugador reaparece con los datos
    /// del último checkpoint. Llamado desde LevelManager.
    /// </summary>
    public void RestartCurrentScene()
    {
        System.GC.Collect();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        System.GC.Collect();
    }

    /// <summary>Vuelve al menú principal. Llamado desde LevelManager.</summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS — CHECKPOINT ----
    #region Métodos públicos — Checkpoint

    /// <summary>
    /// Guarda el estado actual del jugador como checkpoint.
    /// Llamado por LevelManager al completar una planta.
    /// </summary>
    public void SaveCheckpoint(int health, int bandages, int keys)
    {
        _savedHealth = health;
        _savedBandages = bandages;
        _savedKeys = keys;
        Debug.Log($"[GameManager] Checkpoint — Vida: {health}, Vendas: {bandages}, Llaves: {keys}");
    }

    /// <summary>Devuelve la vida guardada en el último checkpoint.</summary>
    public int GetSavedHealth() => _savedHealth;

    /// <summary>Devuelve las vendas guardadas en el último checkpoint.</summary>
    public int GetSavedBandages() => _savedBandages;

    /// <summary>Devuelve las llaves guardadas en el último checkpoint.</summary>
    public int GetSavedKeys() => _savedKeys;

    /// <summary>Devuelve si el multidireccional está desbloqueado.</summary>
    public bool HasMultishot() => _hasMultishot;

    /// <summary>Desbloquea el disparo multidireccional permanentemente.</summary>
    public void UnlockMultishot() => _hasMultishot = true;

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void Init()
    {
        _savedHealth = 200;
        _savedBandages = 0;
        _savedKeys = 0;
        _hasMultishot = false;

        // Valores por defecto de los ajustes de cámara
        CameraShakeIntensity = 1f;
        CameraFollowDelay = 0.5f;
    }

    #endregion

} // class GameManager