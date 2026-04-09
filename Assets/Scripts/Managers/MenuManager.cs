//---------------------------------------------------------
// Controlador del menú principal usando UI Toolkit.
// Accede a los elementos con UQuery (root.Q<>)
// Suscribe eventos con RegisterCallback 
// La visibilidad de los paneles se controla con
// AddToClassList / RemoveFromClassList 
// Alexia Peñerez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la navegación del menú principal (Iniciar, Ajustes, Créditos, Salir)
/// y los ajustes de sonido y cámara. Usa UI Toolkit: UIDocument + UQuery + eventos.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MenuManager : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Audio")]
    [Tooltip("AudioSource del que controla la música de fondo.")]
    [SerializeField] private AudioSource _musicaSource;

    [Tooltip("AudioSource del que controla los efectos de sonido.")]
    [SerializeField] private AudioSource _efectosSource;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    // Elementos raíz
    private VisualElement _root;
    private VisualElement _panelMain;
    private VisualElement _panelAjustes;
    private VisualElement _panelCreditos;

    // Ajustes de cámara (se sincronizan con GameManager si existe)
    private float _shakeIntensity = 1f;
    private float _followDelay = 0.5f;

    // Etiquetas de valores numéricos
    private Label _lblShake;
    private Label _lblDelay;

    // Clase USS que hace visible un panel overlay (Lab 2 DSI)
    private const string CSS_VISIBLE = "panel-overlay--visible";

    // Paso de incremento para los controles de cámara
    private const float PASO_SHAKE = 0.1f;
    private const float PASO_DELAY = 0.1f;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// OnEnable: se obtiene el UIDocument y se conectan todos los eventos.
    /// Se usa OnEnable en lugar de Start para que funcione aunque el objeto
    /// se desactive y reactive (patrón del Lab 3 DSI).
    /// </summary>
    private void OnEnable()
    {
        // Obtener la raíz del UIDocument (patrón Lab 2 DSI)
        UIDocument doc = GetComponent<UIDocument>();
        _root = doc.rootVisualElement;

        // ── Cachear paneles con Q<> (UQuery — Lab 2 DSI) ──
        _panelMain = _root.Q<VisualElement>("panelMain");
        _panelAjustes = _root.Q<VisualElement>("panelAjustes");
        _panelCreditos = _root.Q<VisualElement>("panelCreditos");

        // ── Cachear labels de valores ──
        _lblShake = _root.Q<Label>("lblShake");
        _lblDelay = _root.Q<Label>("lblDelay");

        // ── Sincronizar valores con GameManager si existe ──
        if (GameManager.HasInstance())
        {
            _shakeIntensity = GameManager.Instance.CameraShakeIntensity;
            _followDelay = GameManager.Instance.CameraFollowDelay;
        }

        RefrescarLabels();
        AsegurarPaneles();
        SuscribirEventos();
    }

    private void OnDisable()
    {
        // UI Toolkit limpia automáticamente los callbacks al destruir los elementos,
        // pero es buena práctica limpiar referencias para ayudar al GC.
        _root = null;
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Asegura el estado inicial: menú principal visible, overlays ocultos.
    /// </summary>
    private void AsegurarPaneles()
    {
        // Botones de navegación: menú principal visible, sin clase visible en overlays
        _panelAjustes.RemoveFromClassList(CSS_VISIBLE);
        _panelCreditos.RemoveFromClassList(CSS_VISIBLE);
    }

    /// <summary>
    /// Suscribe todos los eventos de botones con RegisterCallback (Lab 3 DSI).
    /// clicked es azúcar sintáctico de RegisterCallback&lt;ClickEvent&gt;.
    /// </summary>
    private void SuscribirEventos()
    {
        // ── Menú principal ──
        _root.Q<Button>("btnIniciar").clicked += OnIniciarJuego;
        _root.Q<Button>("btnAjustes").clicked += OnAbrirAjustes;
        _root.Q<Button>("btnCreditos").clicked += OnAbrirCreditos;
        _root.Q<Button>("btnSalir").clicked += OnSalir;

        // ── Panel Ajustes ──
        _root.Q<Slider>("sliderMusica").RegisterCallback<ChangeEvent<float>>(OnCambiarMusica);
        _root.Q<Slider>("sliderEfectos").RegisterCallback<ChangeEvent<float>>(OnCambiarEfectos);
        _root.Q<Button>("btnShakeMenos").clicked += () => CambiarShake(-PASO_SHAKE);
        _root.Q<Button>("btnShakeMas").clicked += () => CambiarShake(+PASO_SHAKE);
        _root.Q<Button>("btnDelayMenos").clicked += () => CambiarDelay(-PASO_DELAY);
        _root.Q<Button>("btnDelayMas").clicked += () => CambiarDelay(+PASO_DELAY);
        _root.Q<Button>("btnVolverAjustes").clicked += OnVolverAlMenu;

        // ── Panel Créditos ──
        _root.Q<Button>("btnVolverCreditos").clicked += OnVolverAlMenu;
    }

    /// <summary>Actualiza los labels de Shake y Delay con el formato correcto.</summary>
    private void RefrescarLabels()
    {
        if (_lblShake != null)
            _lblShake.text = _shakeIntensity.ToString("F1");
        if (_lblDelay != null)
            _lblDelay.text = _followDelay.ToString("F1");
    }

    /// <summary>
    /// Muestra un panel overlay y oculta el menú principal.
    /// Usa AddToClassList para activar la clase USS (patrón Lab 2 DSI).
    /// </summary>
    private void MostrarPanel(VisualElement panel)
    {
        _panelMain.style.display = DisplayStyle.None;
        panel.AddToClassList(CSS_VISIBLE);
    }

    /// <summary>
    /// Oculta todos los overlays y vuelve a mostrar el menú principal.
    /// Usa RemoveFromClassList (patrón Lab 2 DSI).
    /// </summary>
    private void OcultarPaneles()
    {
        _panelAjustes.RemoveFromClassList(CSS_VISIBLE);
        _panelCreditos.RemoveFromClassList(CSS_VISIBLE);
        _panelMain.style.display = DisplayStyle.Flex;
    }

    #endregion

    // ---- CALLBACKS DE BOTONES ----
    #region Callbacks de botones

    private void OnIniciarJuego()
    {
        System.GC.Collect();
        SceneManager.LoadScene(1); // índice de Level_1
        System.GC.Collect();
    }

    private void OnAbrirAjustes() => MostrarPanel(_panelAjustes);
    private void OnAbrirCreditos() => MostrarPanel(_panelCreditos);
    private void OnVolverAlMenu() => OcultarPaneles();

    private void OnSalir()
    {
        Application.Quit();
        Debug.Log("[MenuManager] Saliendo del juego.");
    }

    private void OnCambiarMusica(ChangeEvent<float> ev)
    {
        if (_musicaSource != null)
            _musicaSource.volume = ev.newValue;
    }

    private void OnCambiarEfectos(ChangeEvent<float> ev)
    {
        if (_efectosSource != null)
            _efectosSource.volume = ev.newValue;
    }

    private void CambiarShake(float delta)
    {
        _shakeIntensity = Mathf.Clamp(Mathf.Round((_shakeIntensity + delta) * 10f) / 10f, 0f, 3f);
        if (GameManager.HasInstance())
            GameManager.Instance.CameraShakeIntensity = _shakeIntensity;
        RefrescarLabels();
    }

    private void CambiarDelay(float delta)
    {
        _followDelay = Mathf.Clamp(Mathf.Round((_followDelay + delta) * 10f) / 10f, 0f, 2f);
        if (GameManager.HasInstance())
            GameManager.Instance.CameraFollowDelay = _followDelay;
        RefrescarLabels();
    }

    #endregion

} // class MenuManager