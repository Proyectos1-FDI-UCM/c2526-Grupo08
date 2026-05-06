//---------------------------------------------------------
// Controlador del menú de pausa con UI Toolkit.
// Alexia Pérez Santana — No Way Down — Proyectos 1 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PauseManager : MonoBehaviour
{
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
        if (_openMenuAction != null) _openMenuAction.performed -= OnPausePressed;
        if (_exitMenuAction != null) _exitMenuAction.performed -= OnCancelPressed;
    }
    #endregion

    #region Inspector
    [Header("Audio")]
    [SerializeField] private AudioSource MusicaSource;
    [SerializeField] private AudioSource EfectosSource;
    [Header("Escena de menu")]
    [SerializeField] private string NombreEscenaMenu = "Menu";
    [Header("Mapa en pausa")]
    [SerializeField] private RenderTexture MapRenderTexture;
    [SerializeField] private GameObject PlayerMarker;
    #endregion

    #region Privados
    private VisualElement _overlay;
    private VisualElement _vistaMain;
    private VisualElement _vistaAjustes;
    private VisualElement _vistaControles;
    private VisualElement _mapImageEl;
    private Button _tabTeclado;
    private Button _tabMando;
    private VisualElement _ctrlTeclado;
    private VisualElement _ctrlMando;
    private Label _lblShake;
    private Label _lblDelay;
    private Button _btnReanudar;
    private Button _btnVolverAjustes;
    private Button _btnVolverControles;

    private bool _isPaused = false;
    public bool IsPaused => _isPaused;
    private bool _uiReady = false;
    private float _shakeIntensity = 1f;
    private float _followDelay = 0.5f;
    private const float PASO = 0.1f;

    private enum Vista { Main, Ajustes, Controles }
    private Vista _vistaActual = Vista.Main;

    private const string CSS_OVERLAY = "pause-overlay--visible";
    private const string CSS_SUBPANEL = "pause-subpanel--visible";
    private const string CSS_TAB_ON = "ctrl-tab--active";
    private const string CSS_HIDDEN = "ctrl-panel--hidden";

    private InputAction _openMenuAction;
    private InputAction _exitMenuAction;
    #endregion

    #region MonoBehaviour
    private void Start()
    {
        InicializarUI();
        InicializarInput();
    }
    #endregion

    #region API publica
    public void TogglePausa()
    {
        if (!_uiReady) { return; }
        if (_isPaused) Reanudar(); else Pausar();
    }
    #endregion

    #region Input
    private void InicializarInput()
    {
        // Buscar el action map Player y habilitarlo explicitamente
        foreach (var asset in Resources.FindObjectsOfTypeAll<InputActionAsset>())
        {
            var playerMap = asset.FindActionMap("Player", throwIfNotFound: false);
            if (playerMap == null) continue;

            playerMap.Enable();
            _openMenuAction = playerMap.FindAction("Menu", throwIfNotFound: false);
            _exitMenuAction = playerMap.FindAction("ExitMenu", throwIfNotFound: false);
            break;
        }

        // Fallback
        if (_openMenuAction == null)
            _openMenuAction = InputSystem.actions?.FindAction("Menu");

        if (_openMenuAction == null)
        {
            Debug.LogError("[PauseManager] Accion 'Menu' no encontrada.");
            return;
        }

        _openMenuAction.performed += OnPausePressed;
        _openMenuAction.Enable();

        if (_exitMenuAction != null)
        {
            _exitMenuAction.performed += OnCancelPressed;
            _exitMenuAction.Enable();
        }

        Debug.Log($"[PauseManager] Input OK — Menu={_openMenuAction != null} Exit={_exitMenuAction != null}");
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (!_uiReady) { return; }
        TogglePausa();
    }

    private void OnCancelPressed(InputAction.CallbackContext ctx)
    {
        if (!_uiReady || !_isPaused) { return; }
        switch (_vistaActual)
        {
            case Vista.Ajustes: MostrarMain(); break;
            case Vista.Controles: MostrarMain(); break;
            case Vista.Main: Reanudar(); break;
        }
    }
    #endregion

    #region UI Inicializacion
    private void InicializarUI()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null) { Debug.LogError("[PauseManager] No hay UIDocument."); return; }

        VisualElement root = doc.rootVisualElement;
        if (root == null) { Debug.LogError("[PauseManager] rootVisualElement null."); return; }

        _overlay = root.Q<VisualElement>("pauseOverlay");
        _vistaMain = root.Q<VisualElement>("pauseMain");
        _vistaAjustes = root.Q<VisualElement>("pauseAjustes");
        _vistaControles = root.Q<VisualElement>("pauseControles");
        _mapImageEl = root.Q<VisualElement>("pauseMapImage");

        if (_overlay == null || _vistaMain == null ||
            _vistaAjustes == null || _vistaControles == null)
        {
            Debug.LogError("[PauseManager] Elementos no encontrados en UXML.\n" +
                $"  pauseOverlay   -> {(_overlay == null ? "NULL" : "OK")}\n" +
                $"  pauseMain      -> {(_vistaMain == null ? "NULL" : "OK")}\n" +
                $"  pauseAjustes   -> {(_vistaAjustes == null ? "NULL" : "OK")}\n" +
                $"  pauseControles -> {(_vistaControles == null ? "NULL" : "OK")}");
            return;
        }

        if (_mapImageEl != null && MapRenderTexture != null)
            _mapImageEl.style.backgroundImage =
                new StyleBackground(Background.FromRenderTexture(MapRenderTexture));

        _tabTeclado = root.Q<Button>("btnTabTeclado");
        _tabMando = root.Q<Button>("btnTabMando");
        _ctrlTeclado = root.Q<VisualElement>("ctrlTeclado");
        _ctrlMando = root.Q<VisualElement>("ctrlMando");
        _lblShake = root.Q<Label>("lblShake");
        _lblDelay = root.Q<Label>("lblDelay");
        _btnReanudar = root.Q<Button>("btnReanudar");
        _btnVolverAjustes = root.Q<Button>("btnVolverAjustes");
        _btnVolverControles = root.Q<Button>("btnVolverControles");

        if (GameManager.HasInstance())
        {
            _shakeIntensity = GameManager.Instance.CameraShakeIntensity;
            _followDelay = GameManager.Instance.CameraFollowDelay;
        }

        _overlay.RemoveFromClassList(CSS_OVERLAY);
        RefrescarLabels();
        SuscribirEventos(root);
        _uiReady = true;
        Debug.Log("[PauseManager] UI lista v");
    }

    private void SuscribirEventos(VisualElement root)
    {
        Bind(root, "btnReanudar", Reanudar);
        Bind(root, "btnAjustes", () => MostrarSubpanel(Vista.Ajustes));
        Bind(root, "btnControles", () => MostrarSubpanel(Vista.Controles));
        Bind(root, "btnMenuPpal", IrAlMenu);
        Bind(root, "btnSalir", Application.Quit);

        var slM = root.Q<Slider>("sliderMusica");
        var slE = root.Q<Slider>("sliderEfectos");
        if (slM != null) slM.RegisterCallback<ChangeEvent<float>>(
            ev => { if (MusicaSource != null) MusicaSource.volume = ev.newValue; });
        if (slE != null) slE.RegisterCallback<ChangeEvent<float>>(
            ev => { if (EfectosSource != null) EfectosSource.volume = ev.newValue; });

        Bind(root, "btnShakeMenos", () => CambiarShake(-PASO));
        Bind(root, "btnShakeMas", () => CambiarShake(+PASO));
        Bind(root, "btnDelayMenos", () => CambiarDelay(-PASO));
        Bind(root, "btnDelayMas", () => CambiarDelay(+PASO));
        Bind(root, "btnVolverAjustes", MostrarMain);

        if (_tabTeclado != null) _tabTeclado.clicked += () => CambiarTab(true);
        if (_tabMando != null) _tabMando.clicked += () => CambiarTab(false);
        Bind(root, "btnVolverControles", MostrarMain);
    }

    private void Bind(VisualElement root, string name, System.Action cb)
    {
        Button btn = root.Q<Button>(name);
        if (btn != null) btn.clicked += cb;
        else Debug.LogWarning($"[PauseManager] Boton '{name}' no encontrado.");
    }
    #endregion

    #region Logica de pausa
    private void Pausar()
    {
        Debug.Log("[PauseManager] Pausar() ejecutado");
        _isPaused = true;
        Time.timeScale = 0f;
        _overlay.AddToClassList(CSS_OVERLAY);
        Debug.Log($"[PauseManager] Clases overlay: {string.Join(",", _overlay.GetClasses())}");
        MostrarMain();
        if (PlayerMarker != null) PlayerMarker.SetActive(true);
        if (LevelManager.HasInstance()) LevelManager.Instance.OnGamePaused();
    }

    private void Reanudar()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _overlay.RemoveFromClassList(CSS_OVERLAY);
        if (PlayerMarker != null) PlayerMarker.SetActive(false);
        if (LevelManager.HasInstance()) LevelManager.Instance.OnGameResumed();
    }

    private void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(NombreEscenaMenu);
    }

    private void MostrarMain()
    {
        _vistaActual = Vista.Main;
        _vistaMain.style.display = DisplayStyle.Flex;
        _vistaAjustes.RemoveFromClassList(CSS_SUBPANEL);
        _vistaControles.RemoveFromClassList(CSS_SUBPANEL);
        _btnReanudar?.Focus();
    }

    private void MostrarSubpanel(Vista vista)
    {
        _vistaActual = vista;
        _vistaMain.style.display = DisplayStyle.None;
        if (vista == Vista.Ajustes)
        {
            _vistaAjustes.AddToClassList(CSS_SUBPANEL);
            _vistaControles.RemoveFromClassList(CSS_SUBPANEL);
            RefrescarLabels();
            _btnVolverAjustes?.Focus();
        }
        else
        {
            _vistaControles.AddToClassList(CSS_SUBPANEL);
            _vistaAjustes.RemoveFromClassList(CSS_SUBPANEL);
            CambiarTab(true);
            _tabTeclado?.Focus();
        }
    }

    private void CambiarTab(bool teclado)
    {
        if (_ctrlTeclado == null || _ctrlMando == null) { return; }
        if (teclado)
        {
            _ctrlTeclado.RemoveFromClassList(CSS_HIDDEN);
            _ctrlMando.AddToClassList(CSS_HIDDEN);
            _tabTeclado?.AddToClassList(CSS_TAB_ON);
            _tabMando?.RemoveFromClassList(CSS_TAB_ON);
        }
        else
        {
            _ctrlMando.RemoveFromClassList(CSS_HIDDEN);
            _ctrlTeclado.AddToClassList(CSS_HIDDEN);
            _tabMando?.AddToClassList(CSS_TAB_ON);
            _tabTeclado?.RemoveFromClassList(CSS_TAB_ON);
        }
    }
    #endregion

    #region Ajustes
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