//---------------------------------------------------------
// Controlador del menú principal usando UI Toolkit.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
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
public class MainMenu : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Inspector

    [Header("Audio")]
    [Tooltip("AudioSource usado para la música del menú. Su volumen se controla desde el slider de Ajustes.")]
    [SerializeField] private AudioSource _musicaSource;

    [Tooltip("AudioSource usado para los efectos de sonido del menú. Su volumen se controla desde el slider de Ajustes.")]
    [SerializeField] private AudioSource _efectosSource;

    [Header("Escena de inicio")]
    [Tooltip("Nombre de la escena que se carga al pulsar 'Iniciar Juego'.")]
    [SerializeField] private string NombreEscenaInicio = "Level_1";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Privados

    /// <summary>rootVisualElement del UIDocument de este menú.</summary>
    private VisualElement _root;

    /// <summary>Panel principal (botones Iniciar/Ajustes/Controles/Créditos/Salir).</summary>
    private VisualElement _panelMain;

    /// <summary>Subpanel de Ajustes.</summary>
    private VisualElement _panelAjustes;

    /// <summary>Subpanel de Controles.</summary>
    private VisualElement _panelControles;

    /// <summary>Subpanel de Créditos.</summary>
    private VisualElement _panelCreditos;

    // Tabs de controles

    /// <summary>Botón de la pestaña "Teclado" del panel de Controles.</summary>
    private Button _tabTecladoM;

    /// <summary>Botón de la pestaña "Mando" del panel de Controles.</summary>
    private Button _tabMandoM;

    /// <summary>Contenido de controles de teclado.</summary>
    private VisualElement _ctrlTecladoM;

    /// <summary>Contenido de controles de mando.</summary>
    private VisualElement _ctrlMandoM;

    /// <summary>Label que muestra el valor numérico de intensidad de shake.</summary>
    private Label _lblShake;

    /// <summary>Label que muestra el valor numérico de follow delay de cámara.</summary>
    private Label _lblDelay;

    /// <summary>Intensidad de shake de cámara mostrada/editada en el panel de Ajustes.</summary>
    private float _shakeIntensity = 1f;

    /// <summary>Follow delay de cámara mostrado/editado en el panel de Ajustes.</summary>
    private float _followDelay = 0.5f;

    /// <summary>Incremento aplicado por cada pulsación de los botones +/- de shake.</summary>
    private const float PASO_SHAKE = 0.1f;

    /// <summary>Incremento aplicado por cada pulsación de los botones +/- de follow delay.</summary>
    private const float PASO_DELAY = 0.1f;

    /// <summary>Valor máximo permitido para la intensidad de shake.</summary>
    private const float ShakeMax = 3f;

    /// <summary>Valor máximo permitido para el follow delay de cámara.</summary>
    private const float DelayMax = 2f;

    /// <summary>Factor usado para redondear shake/delay a un decimal (Mathf.Round(x * RoundingFactor) / RoundingFactor).</summary>
    private const float RoundingFactor = 10f;

    // Primer botón de cada panel para foco con mando

    /// <summary>Botón "Iniciar Juego", recibe el foco al volver al panel principal.</summary>
    private Button _btnIniciar;

    /// <summary>Botón "Volver" del panel de Ajustes.</summary>
    private Button _btnVolverAjustes;

    /// <summary>Botón "Volver" del panel de Controles.</summary>
    private Button _btnVolverControles;

    /// <summary>Botón "Volver" del panel de Créditos.</summary>
    private Button _btnVolverCreditos;

    private const string CSS_VISIBLE = "panel-overlay--visible";
    private const string CSS_TAB_ON = "ctrl-tab--active";
    private const string CSS_HIDDEN = "ctrl-panel--hidden";

    /// <summary>Acción de Input System que vuelve al panel principal desde un subpanel (Cancel).</summary>
    private InputAction _cancelAction;

    /// <summary>True una vez que InicializarUI ha encontrado todos los elementos necesarios del UXML.</summary>
    private bool _inicializado = false;

    // Panel activo para que Cancel sepa a dónde volver

    /// <summary>Paneles disponibles en el menú principal.</summary>
    private enum Panel { Main, Ajustes, Controles, Creditos }

    /// <summary>Panel actualmente visible.</summary>
    private Panel _panelActual = Panel.Main;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region MonoBehaviour

    /// <summary>
    /// Obtiene el UIDocument/rootVisualElement, inicializa el input de
    /// Cancel y retrasa un frame la consulta de los paneles concretos
    /// (ver InicializarUI) para que UI Toolkit haya terminado de construir
    /// el árbol visual del UXML.
    /// </summary>
    private void Start()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null) { Debug.LogError("[MenuManager] No hay UIDocument."); return; }

        _root = doc.rootVisualElement;
        if (_root == null) { Debug.LogError("[MenuManager] rootVisualElement null."); return; }

        InicializarInput();

        _root.schedule.Execute(InicializarUI);
    }

    /// <summary>Desuscribe y desactiva la acción de Cancel al desactivar este componente.</summary>
    private void OnDisable()
    {
        if (_cancelAction != null) { _cancelAction.performed -= OnCancelPressed; _cancelAction.Disable(); }
    }

    #endregion

    // ---- INPUT ----
    #region Input

    /// <summary>
    /// Suscribe la acción "Cancel" del Input System para volver al panel
    /// principal desde cualquier subpanel.
    /// </summary>
    private void InicializarInput()
    {
        _cancelAction = InputSystem.actions?.FindAction("Cancel");
        if (_cancelAction != null) { _cancelAction.performed += OnCancelPressed; _cancelAction.Enable(); }
    }

    /// <summary>Al pulsar Cancel en un subpanel, vuelve al panel principal.</summary>
    private void OnCancelPressed(InputAction.CallbackContext ctx)
    {
        if (!_inicializado) { return; }
        if (_panelActual != Panel.Main) OcultarPaneles();
    }

    #endregion

    // ---- UI — INICIALIZACIÓN ----
    #region UI — Inicialización

    /// <summary>
    /// Obtiene las referencias a todos los paneles y controles del UXML,
    /// suscribe los eventos de los botones y deja el menú listo para usarse.
    /// Se ejecuta un frame después de Start() mediante _root.schedule.Execute.
    /// </summary>
    private void InicializarUI()
    {
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
            _shakeIntensity = GameManager.Instance.GetShakeDelay();
            _followDelay = GameManager.Instance.GetCameraFollowDelay();
        }

        RefrescarLabels();
        AsegurarPaneles();
        SuscribirEventos();

        // Foco inicial para mando
        _btnIniciar?.Focus();

        _inicializado = true;
    }

    /// <summary>
    /// Quita la clase CSS de visibilidad a todos los subpaneles para que
    /// solo se muestre el panel principal al arrancar.
    /// </summary>
    private void AsegurarPaneles()
    {
        _panelAjustes.RemoveFromClassList(CSS_VISIBLE);
        _panelControles.RemoveFromClassList(CSS_VISIBLE);
        _panelCreditos.RemoveFromClassList(CSS_VISIBLE);
    }

    /// <summary>
    /// Suscribe los callbacks de todos los botones y sliders del menú.
    /// </summary>
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

    /// <summary>
    /// Busca un botón por nombre y le asigna el callback indicado.
    /// Si no se encuentra, avisa por consola.
    /// </summary>
    private void Bind(string name, System.Action cb)
    {
        Button btn = _root.Q<Button>(name);
        if (btn != null) btn.clicked += cb;
        else Debug.LogWarning($"[MenuManager] Botón '{name}' no encontrado.");
    }

    #endregion

    // ---- NAVEGACIÓN DE PANELES ----
    #region Navegación de paneles

    /// <summary>
    /// Inicia la partida cargando la escena configurada en NombreEscenaInicio.
    /// </summary>
    private void OnIniciarJuego()
    {
        System.GC.Collect();
        SceneManager.LoadScene(NombreEscenaInicio);
    }

    /// <summary>
    /// Oculta el panel principal y muestra el subpanel indicado.
    /// </summary>
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

    /// <summary>
    /// Oculta todos los subpaneles y vuelve a mostrar el panel principal.
    /// </summary>
    private void OcultarPaneles()
    {
        _panelActual = Panel.Main;
        AsegurarPaneles();
        if (_panelMain != null) _panelMain.style.display = DisplayStyle.Flex;
        _btnIniciar?.Focus();
    }

    /// <summary>
    /// Cambia entre la pestaña de controles de Teclado y la de Mando dentro
    /// del panel de Controles.
    /// </summary>
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

    // ---- AJUSTES — CÁMARA ----
    #region Ajustes — cámara

    /// <summary>
    /// Aumenta o reduce la intensidad del shake de cámara (redondeada a un
    /// decimal y limitada entre 0 y ShakeMax).
    /// Persiste el valor en GameManager para que CameraController lo lea en partida.
    /// </summary>
    private void CambiarShake(float d)
    {
        _shakeIntensity = Mathf.Clamp(Mathf.Round((_shakeIntensity + d) * RoundingFactor) / RoundingFactor, 0f, ShakeMax);

        if (GameManager.HasInstance())
            GameManager.Instance.SetShakeIntensity(_shakeIntensity);

        RefrescarLabels();
    }

    /// <summary>
    /// Aumenta o reduce el follow delay de cámara (redondeado a un decimal
    /// y limitado entre 0 y DelayMax).
    /// Persiste el valor en GameManager para que CameraController lo lea en partida.
    /// </summary>
    private void CambiarDelay(float d)
    {
        _followDelay = Mathf.Clamp(Mathf.Round((_followDelay + d) * RoundingFactor) / RoundingFactor, 0f, DelayMax);

        if (GameManager.HasInstance())
            GameManager.Instance.SetCameraFollowDelay(_followDelay);

        RefrescarLabels();
    }

    /// <summary>
    /// Actualiza los labels de Ajustes con los valores actuales de shake y delay.
    /// </summary>
    private void RefrescarLabels()
    {
        if (_lblShake != null) _lblShake.text = _shakeIntensity.ToString("F1");
        if (_lblDelay != null) _lblDelay.text = _followDelay.ToString("F1");
    }

    #endregion

} // class MainMenu
  // Alexia Pérez Santana