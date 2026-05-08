//---------------------------------------------------------
// Feedback visual ingame usando UI Toolkit (UIDocument + UXML).
// Muestra tarjetas animadas al recoger objetos y al intentar
// abrir puertas bloqueadas.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Singleton local por escena.
/// Controla las tarjetas de feedback del FeedbackUI.uxml:
///   · pickupCard  — al recoger un objeto
///   · doorCard    — al intentar abrir una puerta bloqueada
///
/// SETUP EN UNITY:
///   · El GameObject debe tener un UIDocument con Source Asset = FeedbackUI.uxml
///   · Este script se adjunta al mismo GameObject que el UIDocument
///   · No hace falta asignar nada más en el Inspector
/// </summary>
public class FeedbackUI : MonoBehaviour
{
    // ---- SINGLETON ----
    #region Singleton

    private static FeedbackUI _instance;

    public static FeedbackUI Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogWarning("[FeedbackUI] No hay instancia en escena.");
            return _instance;
        }
    }

    public static bool HasInstance() => _instance != null;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    #endregion

    // ---- INSPECTOR ----
    #region Inspector

    [Header("Timing")]
    [Tooltip("Segundos que permanece visible la tarjeta de pickup.")]
    [SerializeField] private float PickupDuration = 2.5f;

    [Tooltip("Segundos que permanece visible la tarjeta de puerta.")]
    [SerializeField] private float DoorDuration = 2f;

    [Header("Sprites de objetos (opcionales)")]
    [SerializeField] private Sprite SpriteFusible;
    [SerializeField] private Sprite SpriteLlave;
    [SerializeField] private Sprite SpriteVenda;
    [SerializeField] private Sprite SpriteTarjeta;
    [SerializeField] private Sprite SpriteLlaveEspecial;
    [SerializeField] private Sprite SpriteHabilidadMulti;
    [SerializeField] private Sprite SpriteHabilidadExplosiva;
    [SerializeField] private Sprite SpritePuertaBloqueada;
    [SerializeField] private Sprite SpritePuertaAbierta;

    #endregion

    // ---- PRIVADOS ----
    #region Privados

    private VisualElement _pickupCard;
    private Label _pickupLabel;
    private Label _pickupSublabel;

    private VisualElement _doorCard;
    private Label _doorLabel;
    private Label _doorSublabel;

    private float _pickupTimer = 0f;
    private float _doorTimer = 0f;

    private bool _ready = false;

    private const string CSS_VISIBLE = "feedback-card--visible";
    private const string CSS_LOCKED = "feedback-card--locked";
    private const string CSS_OPEN = "feedback-card--unlocked";

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour

    private void Start()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[FeedbackUI] No se encontró UIDocument en este GameObject.");
            return;
        }

        VisualElement root = doc.rootVisualElement;

        _pickupCard = root.Q<VisualElement>("pickupCard");
        _pickupLabel = root.Q<Label>("pickupLabel");
        _pickupSublabel = root.Q<Label>("pickupSublabel");

        _doorCard = root.Q<VisualElement>("doorCard");
        _doorLabel = root.Q<Label>("doorLabel");
        _doorSublabel = root.Q<Label>("doorSublabel");

        if (_pickupCard == null || _doorCard == null)
        {
            Debug.LogError("[FeedbackUI] No se encontraron 'pickupCard' o 'doorCard' en el UXML. " +
                           "Verifica que FeedbackUI.uxml está asignado al UIDocument.");
            return;
        }

        // Ocultar al inicio (sin la clase visible)
        _pickupCard.RemoveFromClassList(CSS_VISIBLE);
        _doorCard.RemoveFromClassList(CSS_VISIBLE);

        _ready = true;
    }

    private void Update()
    {
        if (!_ready) { return; }

        // Timer pickup
        if (_pickupTimer > 0f)
        {
            _pickupTimer -= Time.deltaTime;
            if (_pickupTimer <= 0f)
                _pickupCard.RemoveFromClassList(CSS_VISIBLE);
        }

        // Timer puerta
        if (_doorTimer > 0f)
        {
            _doorTimer -= Time.deltaTime;
            if (_doorTimer <= 0f)
                _doorCard.RemoveFromClassList(CSS_VISIBLE);
        }
    }

    #endregion

    // ---- API PÚBLICA ----
    #region API pública

    /// <summary>Muestra la tarjeta de pickup con nombre, icono y cantidad.</summary>
    public void MostrarPickup(string nombre, Sprite icono, int cantidad)
    {
        if (!_ready) { return; }

        if (_pickupLabel != null) _pickupLabel.text = nombre;
        if (_pickupSublabel != null) _pickupSublabel.text = cantidad >= 0 ? $"Total: {cantidad}" : "";

        // Aplicar icono — tamaño forzado en inline style para que UI Toolkit lo renderice
        VisualElement iconEl = _pickupCard?.Q<VisualElement>("pickupIcon");
        if (iconEl != null)
        {
            if (icono != null)
            {
                iconEl.style.backgroundImage = new StyleBackground(icono);
                iconEl.style.backgroundSize = new StyleBackgroundSize(
                    new BackgroundSize(BackgroundSizeType.Contain));
                iconEl.style.width = 64;
                iconEl.style.height = 64;
                iconEl.style.display = DisplayStyle.Flex;
            }
            else
            {
                iconEl.style.display = DisplayStyle.None;
            }
        }

        _pickupCard.AddToClassList(CSS_VISIBLE);
        _pickupTimer = PickupDuration;
    }

    /// <summary>Detecta nombre e icono automáticamente según el tipo de objeto.</summary>
    public void MostrarPickupTipo(Objects.ObjectsType tipo, int cantidad)
    {
        if (!_ready) { return; }

        string nombre;
        switch (tipo)
        {
            case Objects.ObjectsType.fusible: nombre = "Fusible"; break;
            case Objects.ObjectsType.key: nombre = "Llave"; break;
            case Objects.ObjectsType.bandage: nombre = "Venda"; break;
            case Objects.ObjectsType.card: nombre = "Tarjeta de acceso"; break;
            case Objects.ObjectsType.multiAbility: nombre = "Habilidad multidireccional"; break;
            case Objects.ObjectsType.explosiveAbility: nombre = "Habilidad explosiva"; break;
            default: nombre = "Objeto"; break;
        }

        Sprite icono = tipo switch
        {
            Objects.ObjectsType.fusible => SpriteFusible,
            Objects.ObjectsType.key => SpriteLlave,
            Objects.ObjectsType.bandage => SpriteVenda,
            Objects.ObjectsType.card => SpriteTarjeta,
            Objects.ObjectsType.multiAbility => SpriteHabilidadMulti,
            Objects.ObjectsType.explosiveAbility => SpriteHabilidadExplosiva,
            _ => null
        };

        int c = (tipo == Objects.ObjectsType.multiAbility ||
                 tipo == Objects.ObjectsType.explosiveAbility) ? -1 : cantidad;

        MostrarPickup(nombre, icono, c);
    }

    /// <summary>Muestra la tarjeta de estado de puerta (bloqueada o abierta).</summary>
    public void MostrarPuerta(bool bloqueada, string mensaje, string submensaje = "")
    {
        if (!_ready) { return; }

        if (_doorLabel != null) _doorLabel.text = mensaje;
        if (_doorSublabel != null) _doorSublabel.text = submensaje;

        _doorCard.RemoveFromClassList(CSS_LOCKED);
        _doorCard.RemoveFromClassList(CSS_OPEN);
        _doorCard.AddToClassList(bloqueada ? CSS_LOCKED : CSS_OPEN);
        _doorCard.AddToClassList(CSS_VISIBLE);

        _doorTimer = DoorDuration;
    }

    #endregion

} // class FeedbackUI