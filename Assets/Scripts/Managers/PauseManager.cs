//---------------------------------------------------------
// Controlador del menú de pausa. Usa UI Toolkit con los mismos patrones
// que MenuManager (UQuery, RegisterCallback, AddToClassList/RemoveFromClassList).
// Se abre/cierra con Escape (teclado) o Start (mando).
// Hereda la estética del menú principal: misma paleta, mismos estilos USS.
// Reemplaza SettingsMenu.cs para la escena de juego.
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
///
/// Vista principal: Reanudar · Ajustes · Menú Principal · Salir
/// Vista Ajustes:   Música · Efectos · Vibración cámara · Suavidad cámara
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class PauseManager : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Audio")]
    [Tooltip("AudioSource de música de fondo.")]
    [SerializeField] private AudioSource MusicaSource;

    [Tooltip("AudioSource de efectos de sonido.")]
    [SerializeField] private AudioSource EfectosSource;

    [Header("Escena de menú")]
    [Tooltip("Nombre exacto de la escena del menú principal.")]
    [SerializeField] private string NombreEscenaMenu = "MainMenu";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    // ── Elementos UI ──
    private VisualElement _overlay;
    private VisualElement _vistaMain;
    private VisualElement _vistaAjustes;
    private Label _lblShake;
    private Label _lblDelay;

    // ── Estado ──
    private bool _isPaused = false;

    // ── Ajustes de cámara ──
    private float _shakeIntensity = 1f;
    private float _followDelay = 0.5f;
    private const float PASO = 0.1f;

    // ── Clase USS de visibilidad ──
    private const string CSS_OVERLAY_VISIBLE = "pause-overlay--visible";
    private const string CSS_SETTINGS_VISIBLE = "pause-settings--visible";

    // ── Input ──
    private InputAction _pauseAction;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void OnEnable()
    {
        // ── Obtener la raíz del UIDocument (Lab 2 DSI) ──
        UIDocument doc = GetComponent<UIDocument>();
        VisualElement root = doc.rootVisualElement;

        // ── Cachear elementos con Q<> (UQuery — Lab 2 DSI) ──
        _overlay = root.Q<VisualElement>("pauseOverlay");
        _vistaMain = root.Q<VisualElement>("pauseMain");
        _vistaAjustes = root.Q<VisualElement>("pauseAjustes");
        _lblShake = root.Q<Label>("lblShake");
        _lblDelay = root.Q<Label>("lblDelay");

        // ── Estado inicial: oculto ──
        _overlay.RemoveFromClassList(CSS_OVERLAY_VISIBLE);

        // ── Sincronizar con GameManager si existe ──
        if (GameManager.HasInstance())
        {
            _shakeIntensity = GameManager.Instance.CameraShakeIntensity;
            _followDelay = GameManager.Instance.CameraFollowDelay;
        }

        RefrescarLabels();
        SuscribirEventos(root);

        // ── Input ──
        _pauseAction = InputSystem.actions.FindAction("Pause");
        if (_pauseAction == null)
        {
            // Fallback: buscar por nombre alternativo
            _pauseAction = InputSystem.actions.FindAction("Cancel");
        }
        _pauseAction?.Enable();
    }

    private void OnDisable()
    {
        _pauseAction?.Disable();
    }

    private void Update()
    {
        if (_pauseAction != null && _pauseAction.WasPressedThisFrame())
        {
            TogglePausa();
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Alterna entre pausado y en juego.
    /// Puede llamarse también desde otros scripts (p.ej. botón de HUD).
    /// </summary>
    public void TogglePausa()
    {
        if (_isPaused)
            Reanudar();
        else
            Pausar();
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void SuscribirEventos(VisualElement root)
    {
        // ── Vista principal (Lab 3 DSI) ──
        root.Q<Button>("btnReanudar").clicked += Reanudar;
        root.Q<Button>("btnAjustes").clicked += MostrarAjustes;
        root.Q<Button>("btnMenuPpal").clicked += IrAlMenu;
        root.Q<Button>("btnSalir").clicked += SalirAlEscritorio;

        // ── Vista Ajustes ──
        root.Q<Slider>("sliderMusica").RegisterCallback<ChangeEvent<float>>(OnCambiarMusica);
        root.Q<Slider>("sliderEfectos").RegisterCallback<ChangeEvent<float>>(OnCambiarEfectos);

        root.Q<Button>("btnShakeMenos").clicked += () => CambiarShake(-PASO);
        root.Q<Button>("btnShakeMas").clicked += () => CambiarShake(+PASO);
        root.Q<Button>("btnDelayMenos").clicked += () => CambiarDelay(-PASO);
        root.Q<Button>("btnDelayMas").clicked += () => CambiarDelay(+PASO);

        root.Q<Button>("btnVolverAjustes").clicked += MostrarMain;
    }

    private void Pausar()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        _overlay.AddToClassList(CSS_OVERLAY_VISIBLE);
        MostrarMain();
    }

    private void Reanudar()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _overlay.RemoveFromClassList(CSS_OVERLAY_VISIBLE);
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
        Debug.Log("[PauseManager] Saliendo del juego.");
    }

    // ── Ajustes ──

    private void OnCambiarMusica(ChangeEvent<float> ev)
    {
        if (MusicaSource != null) { MusicaSource.volume = ev.newValue; }
    }

    private void OnCambiarEfectos(ChangeEvent<float> ev)
    {
        if (EfectosSource != null) { EfectosSource.volume = ev.newValue; }
    }

    private void CambiarShake(float delta)
    {
        _shakeIntensity = Mathf.Clamp(
            Mathf.Round((_shakeIntensity + delta) * 10f) / 10f, 0f, 3f);
        if (GameManager.HasInstance())
            GameManager.Instance.CameraShakeIntensity = _shakeIntensity;
        RefrescarLabels();
    }

    private void CambiarDelay(float delta)
    {
        _followDelay = Mathf.Clamp(
            Mathf.Round((_followDelay + delta) * 10f) / 10f, 0f, 2f);
        if (GameManager.HasInstance())
            GameManager.Instance.CameraFollowDelay = _followDelay;
        RefrescarLabels();
    }

    private void RefrescarLabels()
    {
        if (_lblShake != null) { _lblShake.text = _shakeIntensity.ToString("F1"); }
        if (_lblDelay != null) { _lblDelay.text = _followDelay.ToString("F1"); }
    }

    #endregion

} // class PauseManager