//---------------------------------------------------------
// Controlador del menú de pausa con UI Toolkit: overlay de pausa,
// subpaneles de Ajustes y Controles, navegación por teclado/mando
// y mapa en pausa mediante RenderTexture.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Gestiona el menú de pausa (UI Toolkit): muestra/oculta el overlay con
/// la tecla/acción de pausa, navega entre los subpaneles Main/Ajustes/Controles,
/// permite cambiar volumen de música y efectos, shake e intensidad de cámara
/// (espejados en GameManager) y volumen/tabs de controles (teclado/mando).
/// Singleton local de escena.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class PauseManager : MonoBehaviour
{
    // ---- SINGLETON ----
    #region Singleton

    private static PauseManager _instance;

    /// <summary>Acceso global al PauseManager de la escena activa.</summary>
    public static PauseManager Instance => _instance;

    /// <summary>True si hay un PauseManager activo en la escena.</summary>
    public static bool HasInstance() => _instance != null;

    /// <summary>Si ya existe una instancia distinta, destruye este duplicado; si no, se establece como la instancia.</summary>
    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }

    /// <summary>Limpia la referencia estática y desuscribe las acciones de Input System.</summary>
    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
        if (_openMenuAction != null) _openMenuAction.performed -= OnPausePressed;
        if (_exitMenuAction != null) _exitMenuAction.performed -= OnCancelPressed;
    }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Audio")]
    [Tooltip("AudioSource de la música, cuyo volumen controla el slider de música del panel de Ajustes.")]
    [SerializeField] private AudioSource MusicaSource;

    [Tooltip("AudioSource de los efectos, cuyo volumen controla el slider de efectos del panel de Ajustes.")]
    [SerializeField] private AudioSource EfectosSource;

    [Header("Escena de menu")]
    [Tooltip("Nombre de la escena del menú principal a la que se vuelve al pulsar 'Menú principal'.")]
    [SerializeField] private string NombreEscenaMenu = "Menu";

    [Header("Mapa en pausa")]
    [Tooltip("RenderTexture de la cámara del mapa, usada como imagen de fondo del mapa en pausa.")]
    [SerializeField] private RenderTexture MapRenderTexture;

    [Tooltip("Marcador del jugador en el mapa, activado mientras el juego está en pausa.")]
    [SerializeField] private GameObject PlayerMarker;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Contenedor raíz del overlay de pausa (visible/oculto con CSS_OVERLAY).</summary>
    private VisualElement _overlay;

    /// <summary>Subpanel principal del menú de pausa.</summary>
    private VisualElement _vistaMain;

    /// <summary>Subpanel de Ajustes.</summary>
    private VisualElement _vistaAjustes;

    /// <summary>Subpanel de Controles.</summary>
    private VisualElement _vistaControles;

    /// <summary>Elemento de UI donde se muestra el RenderTexture del mapa.</summary>
    private VisualElement _mapImageEl;

    /// <summary>Botón de la pestaña "Teclado" del panel de Controles.</summary>
    private Button _tabTeclado;

    /// <summary>Botón de la pestaña "Mando" del panel de Controles.</summary>
    private Button _tabMando;

    /// <summary>Contenido de controles de teclado.</summary>
    private VisualElement _ctrlTeclado;

    /// <summary>Contenido de controles de mando.</summary>
    private VisualElement _ctrlMando;

    /// <summary>Label que muestra el valor numérico de intensidad de shake.</summary>
    private Label _lblShake;

    /// <summary>Label que muestra el valor numérico de follow delay de cámara.</summary>
    private Label _lblDelay;

    /// <summary>Botón "Reanudar", recibe el foco al volver al panel principal.</summary>
    private Button _btnReanudar;

    /// <summary>Botón "Volver" del panel de Ajustes, recibe el foco al abrir ese panel.</summary>
    private Button _btnVolverAjustes;

    /// <summary>Botón "Volver" del panel de Controles.</summary>
    private Button _btnVolverControles;

    /// <summary>True mientras el juego está en pausa.</summary>
    private bool _isPaused = false;

    /// <summary>Propiedad pública de solo lectura que indica si el juego está en pausa.</summary>
    public bool IsPaused => _isPaused;

    /// <summary>True una vez que InicializarUI ha encontrado todos los elementos necesarios del UXML.</summary>
    private bool _uiReady = false;

    /// <summary>Intensidad de shake de cámara mostrada/editada en el panel de Ajustes.</summary>
    private float _shakeIntensity = 1f;

    /// <summary>Follow delay de cámara mostrado/editado en el panel de Ajustes.</summary>
    private float _followDelay = 0.5f;

    /// <summary>Incremento aplicado por cada pulsación de los botones +/- de shake y delay.</summary>
    private const float PASO = 0.1f;

    /// <summary>Valor máximo permitido para la intensidad de shake.</summary>
    private const float ShakeMax = 3f;

    /// <summary>Valor máximo permitido para el follow delay de cámara.</summary>
    private const float DelayMax = 2f;

    /// <summary>Factor usado para redondear shake/delay a un decimal (Mathf.Round(x * RoundingFactor) / RoundingFactor).</summary>
    private const float RoundingFactor = 10f;

    /// <summary>Vista actualmente visible dentro del menú de pausa.</summary>
    private enum Vista { Main, Ajustes, Controles }

    /// <summary>Vista actual (Main, Ajustes o Controles).</summary>
    private Vista _vistaActual = Vista.Main;

    private const string CSS_OVERLAY = "pause-overlay--visible";
    private const string CSS_SUBPANEL = "pause-subpanel--visible";
    private const string CSS_TAB_ON = "ctrl-tab--active";
    private const string CSS_HIDDEN = "ctrl-panel--hidden";

    /// <summary>Acción de Input System que abre/cierra el menú de pausa.</summary>
    private InputAction _openMenuAction;

    /// <summary>Acción de Input System que vuelve al panel anterior o reanuda el juego (Cancel).</summary>
    private InputAction _exitMenuAction;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region MonoBehaviour

    /// <summary>
    /// Inicializa las referencias de UI Toolkit y las acciones de Input System.
    /// </summary>
    private void Start()
    {
        InicializarUI();
        InicializarInput();
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Alterna entre pausado y reanudado. No hace nada si la UI aún no está lista.
    /// </summary>
    public void TogglePausa()
    {
        if (!_uiReady) { return; }
        if (_isPaused) Reanudar(); else Pausar();
    }

    #endregion

    // ---- INPUT ----
    #region Input

    /// <summary>
    /// Busca el action map "Player" entre los InputActionAsset cargados y
    /// obtiene las acciones "Menu" y "ExitMenu". Si no encuentra "Menu" por
    /// esa vía, recurre a InputSystem.actions como alternativa.
    /// </summary>
    private void InicializarInput()
    {
        // Buscar el action map Player y habilitarlo explícitamente
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

    }

    /// <summary>Alterna la pausa al pulsar la acción de menú.</summary>
    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (!_uiReady) { return; }
        TogglePausa();
    }

    /// <summary>
    /// Al pulsar Cancel: si está en un subpanel (Ajustes/Controles) vuelve
    /// al principal; si está en el principal, reanuda el juego.
    /// </summary>
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

    // ---- UI INICIALIZACIÓN ----
    #region UI Inicializacion

    /// <summary>
    /// Obtiene el rootVisualElement, localiza todos los elementos del UXML,
    /// aplica el RenderTexture del mapa si está disponible, sincroniza
    /// shake/delay desde GameManager, y suscribe los eventos de la UI.
    /// </summary>
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
            _shakeIntensity = GameManager.Instance.GetShakeDelay();
            _followDelay = GameManager.Instance.GetCameraFollowDelay();
        }

        _overlay.RemoveFromClassList(CSS_OVERLAY);
        RefrescarLabels();
        SuscribirEventos(root);
        _uiReady = true;
    }

    /// <summary>
    /// Suscribe todos los botones y sliders del menú de pausa a sus
    /// callbacks correspondientes (reanudar, navegación, volumen, shake/delay, tabs).
    /// </summary>
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

    /// <summary>
    /// Busca un botón por nombre dentro de root y suscribe cb a su evento
    /// clicked. Si no se encuentra, registra un warning.
    /// </summary>
    private void Bind(VisualElement root, string name, System.Action cb)
    {
        Button btn = root.Q<Button>(name);
        if (btn != null) btn.clicked += cb;
        else Debug.LogWarning($"[PauseManager] Boton '{name}' no encontrado.");
    }

    #endregion

    // ---- LÓGICA DE PAUSA ----
    #region Logica de pausa

    /// <summary>
    /// Activa la pausa: detiene el tiempo, muestra el overlay y el panel
    /// principal, activa el marcador del jugador en el mapa y notifica a LevelManager.
    /// </summary>
    private void Pausar()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        _overlay.AddToClassList(CSS_OVERLAY);
        MostrarMain();
        if (PlayerMarker != null) PlayerMarker.SetActive(true);
        if (LevelManager.HasInstance()) LevelManager.Instance.OnGamePaused();
    }

    /// <summary>
    /// Desactiva la pausa: reanuda el tiempo, oculta el overlay, desactiva
    /// el marcador del jugador y notifica a LevelManager.
    /// </summary>
    private void Reanudar()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _overlay.RemoveFromClassList(CSS_OVERLAY);
        if (PlayerMarker != null) PlayerMarker.SetActive(false);
        if (LevelManager.HasInstance()) LevelManager.Instance.OnGameResumed();
    }

    /// <summary>Reanuda el tiempo y vuelve a la escena del menú principal.</summary>
    private void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(NombreEscenaMenu);
    }

    /// <summary>Muestra el subpanel principal y oculta Ajustes/Controles.</summary>
    private void MostrarMain()
    {
        _vistaActual = Vista.Main;
        _vistaMain.style.display = DisplayStyle.Flex;
        _vistaAjustes.RemoveFromClassList(CSS_SUBPANEL);
        _vistaControles.RemoveFromClassList(CSS_SUBPANEL);
        _btnReanudar?.Focus();
    }

    /// <summary>
    /// Oculta el panel principal y muestra el subpanel indicado (Ajustes o
    /// Controles), refrescando sus valores/tabs y dando el foco al primer
    /// elemento interactivo.
    /// </summary>
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

    /// <summary>
    /// Muestra los controles de teclado o mando y resalta la pestaña
    /// correspondiente.
    /// </summary>
    /// <param name="teclado">True para mostrar teclado, false para mando.</param>
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

    // ---- AJUSTES ----
    #region Ajustes

    /// <summary>
    /// Incrementa/decrementa la intensidad de shake en pasos de PASO,
    /// redondeando a un decimal y limitando entre 0 y ShakeMax.
    /// Persiste el valor en GameManager para que CameraController lo lea.
    /// </summary>
    private void CambiarShake(float d)
    {
        _shakeIntensity = Mathf.Clamp(Mathf.Round((_shakeIntensity + d) * RoundingFactor) / RoundingFactor, 0f, ShakeMax);

        if (GameManager.HasInstance())
            GameManager.Instance.SetShakeIntensity(_shakeIntensity);

        RefrescarLabels();
    }

    /// <summary>
    /// Incrementa/decrementa el follow delay en pasos de PASO,
    /// redondeando a un decimal y limitando entre 0 y DelayMax.
    /// Persiste el valor en GameManager para que CameraController lo lea.
    /// </summary>
    private void CambiarDelay(float d)
    {
        _followDelay = Mathf.Clamp(Mathf.Round((_followDelay + d) * RoundingFactor) / RoundingFactor, 0f, DelayMax);

        if (GameManager.HasInstance())
            GameManager.Instance.SetCameraFollowDelay(_followDelay);

        RefrescarLabels();
    }

    /// <summary>Actualiza los labels de shake y delay con los valores actuales (1 decimal).</summary>
    private void RefrescarLabels()
    {
        if (_lblShake != null) _lblShake.text = _shakeIntensity.ToString("F1");
        if (_lblDelay != null) _lblDelay.text = _followDelay.ToString("F1");
    }

    #endregion

} // class PauseManager
  // Alexia Pérez Santana