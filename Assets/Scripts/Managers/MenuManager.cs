//---------------------------------------------------------
// Controlador del menú principal usando UI Toolkit.
// Accede a los elementos con UQuery (root.Q<>)
// Suscribe eventos con RegisterCallback 
// La visibilidad de los paneles se controla con
// AddToClassList / RemoveFromClassList 
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la navegación del menú principal (Iniciar, Ajustes, Créditos, Salir)
/// y los ajustes de sonido y cámara. Usa UI Toolkit: UIDocument + UQuery + eventos.
///
/// CAMBIOS RESPECTO A LA VERSIÓN ANTERIOR:
///   · LoadScene usa nombre de escena ("Level_1") en lugar de índice (1)
///     para que el Build Settings no rompa la carga al reordenar escenas.
///   · InicializarUI() lanza una advertencia amigable en lugar de error fatal
///     cuando panelMain no existe (el juego puede funcionar sin él).
///   · Se elimina el return inmediato si panelMain es null, permitiendo que
///     ajustes y créditos sigan funcionando.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MenuManager : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Audio")]
    [Tooltip("AudioSource que controla la música de fondo.")]
    [SerializeField] private AudioSource _musicaSource;

    [Tooltip("AudioSource que controla los efectos de sonido.")]
    [SerializeField] private AudioSource _efectosSource;

    [Header("Escena de inicio")]
    [Tooltip("Nombre exacto de la escena del Level 1 tal como aparece en Build Settings.")]
    [SerializeField] private string NombreEscenaInicio = "Level_1";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    private VisualElement _root;
    private VisualElement _panelMain;
    private VisualElement _panelAjustes;
    private VisualElement _panelCreditos;

    private float _shakeIntensity = 1f;
    private float _followDelay = 0.5f;

    private Label _lblShake;
    private Label _lblDelay;

    private const string CSS_VISIBLE = "panel-overlay--visible";
    private const float PASO_SHAKE = 0.1f;
    private const float PASO_DELAY = 0.1f;

    private bool _inicializado = false;

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour

    private void Start()
    {
        InicializarUI();
    }

    private void OnDisable()
    {
        _root = null;
        _inicializado = false;
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void InicializarUI()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[MenuManager] No hay UIDocument en este GameObject.");
            return;
        }

        _root = doc.rootVisualElement;
        if (_root == null)
        {
            Debug.LogError("[MenuManager] rootVisualElement es null. " +
                           "¿Está el UXML asignado en Source Asset del UIDocument?");
            return;
        }

        // Cachear paneles — panelMain puede no existir en variantes del UXML
        _panelMain = _root.Q<VisualElement>("panelMain");
        _panelAjustes = _root.Q<VisualElement>("panelAjustes");
        _panelCreditos = _root.Q<VisualElement>("panelCreditos");

        if (_panelMain == null)
            Debug.LogWarning("[MenuManager] 'panelMain' no encontrado en el UXML. " +
                             "La transición al menú principal desde overlays no funcionará.");

        if (_panelAjustes == null)
        {
            Debug.LogError("[MenuManager] 'panelAjustes' no encontrado en el UXML. " +
                           "Verifica que el name en el UXML es exactamente 'panelAjustes'.");
            return;
        }

        if (_panelCreditos == null)
        {
            Debug.LogError("[MenuManager] 'panelCreditos' no encontrado en el UXML.");
            return;
        }

        // Cachear labels de ajustes de cámara
        _lblShake = _root.Q<Label>("lblShake");
        _lblDelay = _root.Q<Label>("lblDelay");

        // Sincronizar con GameManager si existe
        if (GameManager.HasInstance())
        {
            _shakeIntensity = GameManager.Instance.CameraShakeIntensity;
            _followDelay = GameManager.Instance.CameraFollowDelay;
        }

        RefrescarLabels();
        AsegurarPaneles();
        SuscribirEventos();

        _inicializado = true;
    }

    private void AsegurarPaneles()
    {
        _panelAjustes.RemoveFromClassList(CSS_VISIBLE);
        _panelCreditos.RemoveFromClassList(CSS_VISIBLE);
    }

    private void SuscribirEventos()
    {
        // Menú principal
        BindButton("btnIniciar", OnIniciarJuego);
        BindButton("btnAjustes", OnAbrirAjustes);
        BindButton("btnCreditos", OnAbrirCreditos);
        BindButton("btnSalir", OnSalir);

        // Panel Ajustes
        var sliderMusica = _root.Q<Slider>("sliderMusica");
        var sliderEfectos = _root.Q<Slider>("sliderEfectos");
        if (sliderMusica != null) sliderMusica.RegisterCallback<ChangeEvent<float>>(OnCambiarMusica);
        if (sliderEfectos != null) sliderEfectos.RegisterCallback<ChangeEvent<float>>(OnCambiarEfectos);

        BindButton("btnShakeMenos", () => CambiarShake(-PASO_SHAKE));
        BindButton("btnShakeMas", () => CambiarShake(+PASO_SHAKE));
        BindButton("btnDelayMenos", () => CambiarDelay(-PASO_DELAY));
        BindButton("btnDelayMas", () => CambiarDelay(+PASO_DELAY));
        BindButton("btnVolverAjustes", OnVolverAlMenu);

        // Panel Créditos
        BindButton("btnVolverCreditos", OnVolverAlMenu);
    }

    private void BindButton(string name, System.Action callback)
    {
        Button btn = _root.Q<Button>(name);
        if (btn != null)
            btn.clicked += callback;
        else
            Debug.LogWarning($"[MenuManager] Botón '{name}' no encontrado en el UXML.");
    }

    private void RefrescarLabels()
    {
        if (_lblShake != null) _lblShake.text = _shakeIntensity.ToString("F1");
        if (_lblDelay != null) _lblDelay.text = _followDelay.ToString("F1");
    }

    private void MostrarPanel(VisualElement panel)
    {
        if (_panelMain != null) _panelMain.style.display = DisplayStyle.None;
        panel.AddToClassList(CSS_VISIBLE);
    }

    private void OcultarPaneles()
    {
        _panelAjustes.RemoveFromClassList(CSS_VISIBLE);
        _panelCreditos.RemoveFromClassList(CSS_VISIBLE);
        if (_panelMain != null) _panelMain.style.display = DisplayStyle.Flex;
    }

    #endregion

    // ---- CALLBACKS ----
    #region Callbacks

    private void OnIniciarJuego()
    {
        // Usar nombre de escena, no índice, para no depender del orden en Build Settings
        System.GC.Collect();
        SceneManager.LoadScene(NombreEscenaInicio);
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
        if (_musicaSource != null) _musicaSource.volume = ev.newValue;
    }

    private void OnCambiarEfectos(ChangeEvent<float> ev)
    {
        if (_efectosSource != null) _efectosSource.volume = ev.newValue;
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

    #endregion

} // class MenuManager