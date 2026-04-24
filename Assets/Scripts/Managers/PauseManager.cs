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
/// Gestiona el menú de pausa usando UI Toolkit.
/// Vista principal: Reanudar · Ajustes · Menú Principal · Salir
/// Vista Ajustes:   Música · Efectos · Vibración cámara · Suavidad cámara
///
/// INTEGRACIÓN CON EL MAPA:
///   Al pausar, muestra el panelMap del LevelManager (canvas legacy con el mapa
///   y la posición de Cori) y lo oculta al reanudar.
///   El PauseManager no gestiona el contenido del mapa: solo lo activa/desactiva.
///   La referencia a LevelManager se obtiene en runtime, no en Inspector,
///   porque LevelManager es un singleton local de escena.
///
/// IMPORTANTE: la inicialización de UI se hace en Start (no OnEnable)
/// porque UI Toolkit necesita un frame para procesar el UXML.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class PauseManager : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Audio")]
    [SerializeField] private AudioSource MusicaSource;
    [SerializeField] private AudioSource EfectosSource;

    [Header("Escena de menú")]
    [Tooltip("Nombre exacto de la escena del menú principal en Build Settings.")]
    [SerializeField] private string NombreEscenaMenu = "Menu";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

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
    #region MonoBehaviour

    private void Start()
    {
        InicializarUI();
        InicializarInput();
    }

    private void OnDisable()
    {
        _pauseAction?.Disable();
        _uiReady = false;
    }

    private void Update()
    {
        if (!_uiReady) { return; }
        if (_pauseAction != null && _pauseAction.WasPressedThisFrame())
            TogglePausa();
    }

    #endregion

    // ---- API PÚBLICA ----
    #region API pública

    public void TogglePausa()
    {
        if (!_uiReady) { return; }
        if (_isPaused) Reanudar(); else Pausar();
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void InicializarUI()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[PauseManager] No hay UIDocument en este GameObject.");
            return;
        }

        VisualElement root = doc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[PauseManager] rootVisualElement es null. " +
                           "¿Está el UXML asignado en Source Asset?");
            return;
        }

        _overlay = root.Q<VisualElement>("pauseOverlay");
        _vistaMain = root.Q<VisualElement>("pauseMain");
        _vistaAjustes = root.Q<VisualElement>("pauseAjustes");
        _lblShake = root.Q<Label>("lblShake");
        _lblDelay = root.Q<Label>("lblDelay");

        if (_overlay == null || _vistaMain == null || _vistaAjustes == null)
        {
            Debug.LogError("[PauseManager] Elementos críticos no encontrados. " +
                           "Nombres esperados en el UXML: pauseOverlay, pauseMain, pauseAjustes.");
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
    }

    private void InicializarInput()
    {
        _pauseAction = InputSystem.actions.FindAction("Pause");
        if (_pauseAction == null)
            _pauseAction = InputSystem.actions.FindAction("Cancel");
        if (_pauseAction == null)
            Debug.LogWarning("[PauseManager] Acción 'Pause'/'Cancel' no encontrada en InputSystem.");
        else
            _pauseAction.Enable();
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

    private void BindButton(VisualElement root, string name, System.Action callback)
    {
        Button btn = root.Q<Button>(name);
        if (btn != null)
            btn.clicked += callback;
        else
            Debug.LogWarning($"[PauseManager] Botón '{name}' no encontrado en el UXML.");
    }

    private void Pausar()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        _overlay.AddToClassList(CSS_OVERLAY_VISIBLE);
        MostrarMain();

        // Mostrar el mapa del LevelManager durante la pausa
        if (LevelManager.HasInstance())
            LevelManager.Instance.MostrarMapaEnPausa();
    }

    private void Reanudar()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _overlay.RemoveFromClassList(CSS_OVERLAY_VISIBLE);

        // Ocultar el mapa del LevelManager al reanudar
        if (LevelManager.HasInstance())
            LevelManager.Instance.OcultarMapaPausa();
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

    private void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(NombreEscenaMenu);
    }

    private void SalirAlEscritorio()
    {
        Application.Quit();
        Debug.Log("[PauseManager] Saliendo.");
    }

    private void OnCambiarMusica(ChangeEvent<float> ev)
    {
        if (MusicaSource != null) MusicaSource.volume = ev.newValue;
    }

    private void OnCambiarEfectos(ChangeEvent<float> ev)
    {
        if (EfectosSource != null) EfectosSource.volume = ev.newValue;
    }

    private void CambiarShake(float delta)
    {
        _shakeIntensity = Mathf.Clamp(Mathf.Round((_shakeIntensity + delta) * 10f) / 10f, 0f, 3f);
        if (GameManager.HasInstance()) GameManager.Instance.CameraShakeIntensity = _shakeIntensity;
        RefrescarLabels();
    }

    private void CambiarDelay(float delta)
    {
        _followDelay = Mathf.Clamp(Mathf.Round((_followDelay + delta) * 10f) / 10f, 0f, 2f);
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