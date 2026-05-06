//---------------------------------------------------------
// Controlador del menú principal usando UI Toolkit.
// Alexia Pérez Santana
// — No Way Down
// — Proyectos 1 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la navegación del menú principal.
/// Paneles: Main / Ajustes / Controles / Créditos.
///
/// NAVEGACIÓN POR MANDO:
///   · UI Toolkit mueve el foco con D-Pad/Stick izquierdo automáticamente.
///   · "B" / Cancel vuelve al panel principal desde cualquier subpanel.
///   · Al abrir cada panel se fuerza el foco al primer elemento interactivo.
///   · "A" / Submit selecciona el botón enfocado (comportamiento por defecto de UI Toolkit).
///
/// CONTROLES: panel con dos tabs (Teclado / Mando).
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MenuManager : MonoBehaviour
{
    #region Inspector
    [Header("Audio")]
    [SerializeField] private AudioSource _musicaSource;
    [SerializeField] private AudioSource _efectosSource;
    [Header("Escena de inicio")]
    [SerializeField] private string NombreEscenaInicio = "Level_1";
    #endregion

    #region Privados
    private VisualElement _root;
    private VisualElement _panelMain;
    private VisualElement _panelAjustes;
    private VisualElement _panelControles;
    private VisualElement _panelCreditos;

    // Tabs de controles
    private Button _tabTecladoM;
    private Button _tabMandoM;
    private VisualElement _ctrlTecladoM;
    private VisualElement _ctrlMandoM;

    private Label _lblShake;
    private Label _lblDelay;
    private float _shakeIntensity = 1f;
    private float _followDelay = 0.5f;
    private const float PASO_SHAKE = 0.1f;
    private const float PASO_DELAY = 0.1f;

    // Primer botón de cada panel para foco con mando
    private Button _btnIniciar;
    private Button _btnVolverAjustes;
    private Button _btnVolverControles;
    private Button _btnVolverCreditos;

    private const string CSS_VISIBLE = "panel-overlay--visible";
    private const string CSS_TAB_ON = "ctrl-tab--active";
    private const string CSS_HIDDEN = "ctrl-panel--hidden";

    private InputAction _cancelAction;
    private bool _inicializado = false;

    // Panel activo para que Cancel sepa a dónde volver
    private enum Panel { Main, Ajustes, Controles, Creditos }
    private Panel _panelActual = Panel.Main;
    #endregion

    #region MonoBehaviour
    private void Start()
    {
        InicializarUI();
        InicializarInput();
    }
    private void OnDisable()
    {
        if (_cancelAction != null) { _cancelAction.performed -= OnCancelPressed; _cancelAction.Disable(); }
    }
    #endregion

    #region Input
    private void InicializarInput()
    {
        _cancelAction = InputSystem.actions?.FindAction("Cancel");
        if (_cancelAction != null) { _cancelAction.performed += OnCancelPressed; _cancelAction.Enable(); }
    }

    private void OnCancelPressed(InputAction.CallbackContext ctx)
    {
        if (!_inicializado) { return; }
        if (_panelActual != Panel.Main) OcultarPaneles();
    }
    #endregion

    #region UI — Inicialización
    private void InicializarUI()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null) { Debug.LogError("[MenuManager] No hay UIDocument."); return; }

        _root = doc.rootVisualElement;
        if (_root == null) { Debug.LogError("[MenuManager] rootVisualElement null."); return; }

        _panelMain = _root.Q<VisualElement>("panelMain");
        _panelAjustes = _root.Q<VisualElement>("panelAjustes");
        _panelControles = _root.Q<VisualElement>("panelControles");
        _panelCreditos = _root.Q<VisualElement>("panelCreditos");

        if (_panelMain == null)
            Debug.LogWarning("[MenuManager] panelMain no encontrado. La vuelta desde overlays no funcionará.");
        if (_panelAjustes == null) { Debug.LogError("[MenuManager] panelAjustes no encontrado."); return; }
        if (_panelControles == null) { Debug.LogError("[MenuManager] panelControles no encontrado."); return; }
        if (_panelCreditos == null) { Debug.LogError("[MenuManager] panelCreditos no encontrado."); return; }

        // Tabs controles
        _tabTecladoM = _root.Q<Button>("btnTabTecladoM");
        _tabMandoM = _root.Q<Button>("btnTabMandoM");
        _ctrlTecladoM = _root.Q<VisualElement>("ctrlTecladoM");
        _ctrlMandoM = _root.Q<VisualElement>("ctrlMandoM");

        // Labels ajustes
        _lblShake = _root.Q<Label>("lblShake");
        _lblDelay = _root.Q<Label>("lblDelay");

        // Botones de foco
        _btnIniciar = _root.Q<Button>("btnIniciar");
        _btnVolverAjustes = _root.Q<Button>("btnVolverAjustes");
        _btnVolverControles = _root.Q<Button>("btnVolverControlesM");
        _btnVolverCreditos = _root.Q<Button>("btnVolverCreditos");

        if (GameManager.HasInstance())
        {
            _shakeIntensity = GameManager.Instance.CameraShakeIntensity;
            _followDelay = GameManager.Instance.CameraFollowDelay;
        }

        RefrescarLabels();
        AsegurarPaneles();
        SuscribirEventos();

        // Foco inicial para mando
        _btnIniciar?.Focus();

        _inicializado = true;
    }

    private void AsegurarPaneles()
    {
        _panelAjustes.RemoveFromClassList(CSS_VISIBLE);
        _panelControles.RemoveFromClassList(CSS_VISIBLE);
        _panelCreditos.RemoveFromClassList(CSS_VISIBLE);
    }

    private void SuscribirEventos()
    {
        // Menú principal
        Bind("btnIniciar", OnIniciarJuego);
        Bind("btnAjustes", () => AbrirPanel(Panel.Ajustes));
        Bind("btnControles", () => AbrirPanel(Panel.Controles));
        Bind("btnCreditos", () => AbrirPanel(Panel.Creditos));
        Bind("btnSalir", () => { Application.Quit(); Debug.Log("[MenuManager] Saliendo."); });

        // Ajustes
        var slM = _root.Q<Slider>("sliderMusica");
        var slE = _root.Q<Slider>("sliderEfectos");
        if (slM != null) slM.RegisterCallback<ChangeEvent<float>>(ev => { if (_musicaSource != null) _musicaSource.volume = ev.newValue; });
        if (slE != null) slE.RegisterCallback<ChangeEvent<float>>(ev => { if (_efectosSource != null) _efectosSource.volume = ev.newValue; });
        Bind("btnShakeMenos", () => CambiarShake(-PASO_SHAKE));
        Bind("btnShakeMas", () => CambiarShake(+PASO_SHAKE));
        Bind("btnDelayMenos", () => CambiarDelay(-PASO_DELAY));
        Bind("btnDelayMas", () => CambiarDelay(+PASO_DELAY));
        Bind("btnVolverAjustes", OcultarPaneles);

        // Controles
        if (_tabTecladoM != null) _tabTecladoM.clicked += () => CambiarTabControles(teclado: true);
        if (_tabMandoM != null) _tabMandoM.clicked += () => CambiarTabControles(teclado: false);
        Bind("btnVolverControlesM", OcultarPaneles);

        // Créditos
        Bind("btnVolverCreditos", OcultarPaneles);
    }

    private void Bind(string name, System.Action cb)
    {
        Button btn = _root.Q<Button>(name);
        if (btn != null) btn.clicked += cb;
        else Debug.LogWarning($"[MenuManager] Botón '{name}' no encontrado.");
    }
    #endregion

    #region Navegación de paneles
    private void OnIniciarJuego()
    {
        System.GC.Collect();
        SceneManager.LoadScene(NombreEscenaInicio);
    }

    private void AbrirPanel(Panel panel)
    {
        _panelActual = panel;
        if (_panelMain != null) _panelMain.style.display = DisplayStyle.None;

        AsegurarPaneles();

        switch (panel)
        {
            case Panel.Ajustes:
                _panelAjustes.AddToClassList(CSS_VISIBLE);
                RefrescarLabels();
                _btnVolverAjustes?.Focus();
                break;
            case Panel.Controles:
                _panelControles.AddToClassList(CSS_VISIBLE);
                CambiarTabControles(teclado: true);
                _tabTecladoM?.Focus();
                break;
            case Panel.Creditos:
                _panelCreditos.AddToClassList(CSS_VISIBLE);
                _btnVolverCreditos?.Focus();
                break;
        }
    }

    private void OcultarPaneles()
    {
        _panelActual = Panel.Main;
        AsegurarPaneles();
        if (_panelMain != null) _panelMain.style.display = DisplayStyle.Flex;
        _btnIniciar?.Focus();
    }

    private void CambiarTabControles(bool teclado)
    {
        if (_ctrlTecladoM == null || _ctrlMandoM == null) { return; }
        if (teclado)
        {
            _ctrlTecladoM.RemoveFromClassList(CSS_HIDDEN);
            _ctrlMandoM.AddToClassList(CSS_HIDDEN);
            _tabTecladoM?.AddToClassList(CSS_TAB_ON);
            _tabMandoM?.RemoveFromClassList(CSS_TAB_ON);
        }
        else
        {
            _ctrlMandoM.RemoveFromClassList(CSS_HIDDEN);
            _ctrlTecladoM.AddToClassList(CSS_HIDDEN);
            _tabMandoM?.AddToClassList(CSS_TAB_ON);
            _tabTecladoM?.RemoveFromClassList(CSS_TAB_ON);
        }
    }
    #endregion

    #region Ajustes — cámara
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

} // class MenuManager