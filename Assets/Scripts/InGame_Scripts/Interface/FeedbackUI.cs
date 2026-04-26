//---------------------------------------------------------
// Controlador de feedback visual ingame usando UI Toolkit.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Singleton local por escena.
/// Muestra tarjetas animadas para pickup de objetos y estado de puertas.
///
/// IMPORTANTE: la inicialización de UI se hace en Start (no OnEnable)
/// porque UI Toolkit necesita un frame para procesar el UXML.
///
/// </summary>
[RequireComponent(typeof(UIDocument))]
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
                Debug.LogWarning("[FeedbackUI] No hay instancia en esta escena.");
            return _instance;
        }
    }
    public static bool HasInstance() => _instance != null;

    private void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (this == _instance) { _instance = null; }
    }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Timing")]
    [SerializeField] private float PickupDuration = 2.5f;
    [SerializeField] private float DoorDuration = 2f;

    [Header("Sprites de objetos")]
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

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    private VisualElement _doorCard;
    private VisualElement _doorIcon;
    private Label _doorLabel;
    private Label _doorSublabel;

    private VisualElement _pickupCard;
    private VisualElement _pickupIcon;
    private Label _pickupLabel;
    private Label _pickupSublabel;

    private float _pickupTimer = 0f;
    private float _doorTimer = 0f;
    private bool _pickupActive = false;
    private bool _doorActive = false;
    private bool _uiReady = false;

    private const string CSS_VISIBLE = "feedback-card--visible";
    private const string CSS_LOCKED = "feedback-card--locked";
    private const string CSS_UNLOCKED = "feedback-card--unlocked";

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour

    private void Start()
    {
        InicializarUI();
    }

    private void Update()
    {
        if (!_uiReady) { return; }

        if (_pickupActive)
        {
            _pickupTimer -= Time.deltaTime;
            if (_pickupTimer <= 0f)
            {
                _pickupActive = false;
                _pickupCard.RemoveFromClassList(CSS_VISIBLE);
            }
        }

        if (_doorActive)
        {
            _doorTimer -= Time.deltaTime;
            if (_doorTimer <= 0f)
            {
                _doorActive = false;
                _doorCard.RemoveFromClassList(CSS_VISIBLE);
            }
        }
    }

    #endregion

    // ---- API PÚBLICA ----
    #region API pública

    /// <summary>
    /// Muestra la tarjeta de pickup con nombre, icono y cantidad total.
    /// </summary>
    public void MostrarPickup(string nombreObjeto, Sprite icono, int cantidad)
    {
        if (!_uiReady) { return; }

        _pickupLabel.text = nombreObjeto;
        _pickupSublabel.text = $"Total: {cantidad}";
        SetIconSprite(_pickupIcon, icono);

        _pickupTimer = PickupDuration;
        _pickupActive = true;
        _pickupCard.AddToClassList(CSS_VISIBLE);
    }

    /// <summary>
    /// Detecta automáticamente nombre e icono según el tipo de Objects.
    /// </summary>
    public void MostrarPickupTipo(Objects.ObjectsType tipo, int cantidad)
    {
        if (!_uiReady) { return; }

        string nombre;
        Sprite icono;

        switch (tipo)
        {
            case Objects.ObjectsType.fusible:
                nombre = "Fusible"; icono = SpriteFusible; break;
            case Objects.ObjectsType.key:
                nombre = "Llave"; icono = SpriteLlave; break;
            case Objects.ObjectsType.bandage:
                nombre = "Venda"; icono = SpriteVenda; break;
            case Objects.ObjectsType.card:
                nombre = "Tarjeta de acceso"; icono = SpriteTarjeta; break;
            case Objects.ObjectsType.multiAbility:
                nombre = "Habilidad multidireccional"; icono = SpriteHabilidadMulti; cantidad = -1; break;
            case Objects.ObjectsType.explosiveAbility:
                nombre = "Habilidad explosiva"; icono = SpriteHabilidadExplosiva; cantidad = -1; break;
            default:
                nombre = "Objeto"; icono = null; break;
        }

        if (cantidad < 0)
        {
            _pickupLabel.text = nombre;
            _pickupSublabel.text = "";
            SetIconSprite(_pickupIcon, icono);
            _pickupTimer = PickupDuration;
            _pickupActive = true;
            _pickupCard.AddToClassList(CSS_VISIBLE);
        }
        else
        {
            MostrarPickup(nombre, icono, cantidad);
        }
    }

    /// <summary>
    /// Muestra la tarjeta de puerta (bloqueada = amarillo / abierta = cyan).
    /// </summary>
    public void MostrarPuerta(bool bloqueada, string mensaje, string submensaje = "")
    {
        if (!_uiReady) { return; }

        _doorCard.RemoveFromClassList(CSS_LOCKED);
        _doorCard.RemoveFromClassList(CSS_UNLOCKED);

        _doorLabel.text = mensaje;
        _doorSublabel.text = submensaje;

        if (bloqueada)
        {
            _doorCard.AddToClassList(CSS_LOCKED);
            SetIconSprite(_doorIcon, SpritePuertaBloqueada);
        }
        else
        {
            _doorCard.AddToClassList(CSS_UNLOCKED);
            SetIconSprite(_doorIcon, SpritePuertaAbierta);
        }

        _doorTimer = DoorDuration;
        _doorActive = true;
        _doorCard.AddToClassList(CSS_VISIBLE);
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void InicializarUI()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null)
        {
            Debug.LogError("[FeedbackUI] No hay UIDocument en este GameObject.");
            return;
        }

        VisualElement root = doc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[FeedbackUI] rootVisualElement es null. " +
                           "¿Está el UXML asignado en Source Asset?");
            return;
        }

        _doorCard = root.Q<VisualElement>("doorCard");
        _doorIcon = root.Q<VisualElement>("doorIcon");
        _doorLabel = root.Q<Label>("doorLabel");
        _doorSublabel = root.Q<Label>("doorSublabel");

        _pickupCard = root.Q<VisualElement>("pickupCard");
        _pickupIcon = root.Q<VisualElement>("pickupIcon");
        _pickupLabel = root.Q<Label>("pickupLabel");
        _pickupSublabel = root.Q<Label>("pickupSublabel");

        if (_doorCard == null || _pickupCard == null)
        {
            Debug.LogError("[FeedbackUI] 'doorCard' o 'pickupCard' no encontrados en el UXML. " +
                           "Nombres esperados: doorCard, pickupCard.");
            return;
        }

        _doorCard.RemoveFromClassList(CSS_VISIBLE);
        _pickupCard.RemoveFromClassList(CSS_VISIBLE);

        _uiReady = true;
    }

    private void SetIconSprite(VisualElement element, Sprite sprite)
    {
        if (element == null) { return; }

        if (sprite != null)
        {
            element.style.backgroundImage = new StyleBackground(sprite);
            element.style.display = DisplayStyle.Flex;
        }
        else
        {
            element.style.display = DisplayStyle.None;
        }
    }

    #endregion

} // class FeedbackUI