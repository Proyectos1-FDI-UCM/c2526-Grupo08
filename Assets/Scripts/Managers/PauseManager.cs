//---------------------------------------------------------
// Controlador del menú de pausa con UI Toolkit.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Singleton local por escena.
/// Al pausar avisa al LevelManager (OnGamePaused) para bloquear Tab.
/// Al reanudar avisa al LevelManager (OnGameResumed) para forzar HUD visible.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class PauseManager : MonoBehaviour
{
    // ---- SINGLETON ----
    #region Singleton
    private static PauseManager _instance;
    public static PauseManager Instance => _instance;
    public static bool HasInstance() => _instance != null;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }
    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
        if (_pauseAction != null)
        {
            _pauseAction.performed -= OnPausePressed;
            _pauseAction.Disable();
        }
    }
    #endregion

    // ---- INSPECTOR ----
    #region Inspector
    [Header("Audio")]
    [SerializeField] private AudioSource MusicaSource;
    [SerializeField] private AudioSource EfectosSource;
    [Header("Escena de menú")]
    [SerializeField] private string NombreEscenaMenu = "Menu";
    #endregion

    // ---- PRIVADOS ----
    #region Privados
    private VisualElement _overlay;
    private VisualElement _vistaMain;
    private VisualElement _vistaAjustes;
    private Label _lblShake;
    private Label _lblDelay;

    private bool _isPaused = false;
    private bool _uiReady = false;
    private float _shakeIntensity = 1f;
    private float _followDelay = 0.5f;
    private const float PASO = 0.1f;

    private const string CSS_OVERLAY_VISIBLE = "pause-overlay--visible";
    private const string CSS_SETTINGS_VISIBLE = "pause-settings--visible";

    private InputAction _pauseAction;
    #endregion

    // ---- MONOBEHAVIOUR ----
    private void Start()
    {
        InicializarUI();
        InicializarInput();
    }

    // ---- API PÚBLICA ----
    public void TogglePausa()
    {
        if (!_uiReady) { return; }
        if (_isPaused) Reanudar(); else Pausar();
    }

    // ---- INPUT ----
    #region Input
    private void InicializarInput()
    {
        _pauseAction = InputSystem.actions?.FindAction("Pause");
        if (_pauseAction == null)
            _pauseAction = InputSystem.actions?.FindAction("Cancel");
        if (_pauseAction == null) { Debug.LogWarning("[PauseManager] Acción Pause/Cancel no encontrada."); return; }
        _pauseAction.performed += OnPausePressed;
        _pauseAction.Enable();
    }
    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (!_uiReady) { return; }
        TogglePausa();
    }
    #endregion

    // ---- UI ----
    #region UI
    private void InicializarUI()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null) { Debug.LogError("[PauseManager] No hay UIDocument."); return; }

        VisualElement root = doc.rootVisualElement;
        if (root == null) { Debug.LogError("[PauseManager] rootVisualElement null. Asigna PauseMenu.uxml."); return; }

        _overlay = root.Q<VisualElement>("pauseOverlay");
        _vistaMain = root.Q<VisualElement>("pauseMain");
        _vistaAjustes = root.Q<VisualElement>("pauseAjustes");
        _lblShake = root.Q<Label>("lblShake");
        _lblDelay = root.Q<Label>("lblDelay");

        if (_overlay == null || _vistaMain == null || _vistaAjustes == null)
        {
            Debug.LogError("[PauseManager] Elementos no encontrados.\n" +
                           $"  pauseOverlay  → {(_overlay == null ? "NULL ❌" : "OK ✓")}\n" +
                           $"  pauseMain     → {(_vistaMain == null ? "NULL ❌" : "OK ✓")}\n" +
                           $"  pauseAjustes  → {(_vistaAjustes == null ? "NULL ❌" : "OK ✓")}\n" +
                           "Verifica que PauseMenu.uxml tiene <ui:Style src=\".../PauseMenu.uss\"/>");
            return;
        }

        if (GameManager.HasInstance())
        {
            _shakeIntensity = GameManager.Instance.CameraShakeIntensity;
            _followDelay = GameManager.Instance.CameraFollowDelay;
        }

        _overlay.RemoveFromClassList(CSS_OVERLAY_VISIBLE);
        RefrescarLabels();
        SuscribirEventos(root);
        _uiReady = true;
        Debug.Log("[PauseManager] UI lista ✓");
    }

    private void SuscribirEventos(VisualElement root)
    {
        BindButton(root, "btnReanudar", Reanudar);
        BindButton(root, "btnAjustes", MostrarAjustes);
        BindButton(root, "btnMenuPpal", IrAlMenu);
        BindButton(root, "btnSalir", SalirAlEscritorio);
        var sliderMusica = root.Q<Slider>("sliderMusica");
        var sliderEfectos = root.Q<Slider>("sliderEfectos");
        if (sliderMusica != null) sliderMusica.RegisterCallback<ChangeEvent<float>>(OnCambiarMusica);
        if (sliderEfectos != null) sliderEfectos.RegisterCallback<ChangeEvent<float>>(OnCambiarEfectos);
        BindButton(root, "btnShakeMenos", () => CambiarShake(-PASO));
        BindButton(root, "btnShakeMas", () => CambiarShake(+PASO));
        BindButton(root, "btnDelayMenos", () => CambiarDelay(-PASO));
        BindButton(root, "btnDelayMas", () => CambiarDelay(+PASO));
        BindButton(root, "btnVolverAjustes", MostrarMain);
    }

    private void BindButton(VisualElement root, string name, System.Action cb)
    {
        Button btn = root.Q<Button>(name);
        if (btn != null) btn.clicked += cb;
        else Debug.LogWarning($"[PauseManager] Botón '{name}' no encontrado.");
    }
    #endregion

    // ---- LÓGICA ----
    #region Lógica de pausa
    private void Pausar()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        _overlay.AddToClassList(CSS_OVERLAY_VISIBLE);
        MostrarMain();
        // Avisar al LevelManager para que bloquee Tab
        if (LevelManager.HasInstance()) LevelManager.Instance.OnGamePaused();
    }

    private void Reanudar()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _overlay.RemoveFromClassList(CSS_OVERLAY_VISIBLE);
        // Avisar al LevelManager: fuerza HUD visible y desbloquea Tab
        if (LevelManager.HasInstance()) LevelManager.Instance.OnGameResumed();
    }

    private void MostrarMain()
    {
        _vistaMain.style.display = DisplayStyle.Flex;
        _vistaAjustes.RemoveFromClassList(CSS_SETTINGS_VISIBLE);
    }

    private void MostrarAjustes()
    {
        _vistaMain.style.display = DisplayStyle.None;
        _vistaAjustes.AddToClassList(CSS_SETTINGS_VISIBLE);
        RefrescarLabels();
    }

    private void IrAlMenu() { Time.timeScale = 1f; SceneManager.LoadScene(NombreEscenaMenu); }
    private void SalirAlEscritorio() { Application.Quit(); }
    #endregion

    // ---- AUDIO / CÁMARA ----
    #region Audio y cámara
    private void OnCambiarMusica(ChangeEvent<float> ev) { if (MusicaSource != null) MusicaSource.volume = ev.newValue; }
    private void OnCambiarEfectos(ChangeEvent<float> ev) { if (EfectosSource != null) EfectosSource.volume = ev.newValue; }

    private void CambiarShake(float d)
    {
        _shakeIntensity = Mathf.Clamp(Mathf.Round((_shakeIntensity + d) * 10f) / 10f, 0f, 3f);
        if (GameManager.HasInstance()) GameManager.Instance.CameraShakeIntensity = _shakeIntensity;
        RefrescarLabels();
    }
    private void CambiarDelay(float d)
    {
        _followDelay = Mathf.Clamp(Mathf.Round((_followDelay + d) * 10f) / 10f, 0f, 2f);
        if (GameManager.HasInstance()) GameManager.Instance.CameraFollowDelay = _followDelay;
        RefrescarLabels();
    }
    private void RefrescarLabels()
    {
        if (_lblShake != null) _lblShake.text = _shakeIntensity.ToString("F1");
        if (_lblDelay != null) _lblDelay.text = _followDelay.ToString("F1");
    }
    #endregion

} // class PauseManager