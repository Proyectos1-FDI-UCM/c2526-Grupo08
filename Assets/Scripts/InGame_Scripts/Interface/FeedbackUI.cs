//---------------------------------------------------------
// Feedback visual ingame usando UI Toolkit (UXML + USS).
// Singleton por escena. Se asocia al UIDocument FeedbackUIDoc.
// Alexia Pérez Santana
// No Way Down
// — Proyectos 1 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Muestra tarjetas animadas de feedback:
///   · PickupCard — al recoger un objeto (nombre + cantidad)
///   · DoorCard   — al acercarse a una puerta (bloqueada / abierta)
///
/// Llamar desde otros scripts:
///   FeedbackUI.Instance.MostrarPickupTipo(Objects.ObjectsType.key, 2);
///   FeedbackUI.Instance.MostrarPuerta(bloqueada: true, "Puerta bloqueada", "Necesitas una llave");
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class FeedbackUI : MonoBehaviour
{
    // ---- SINGLETON ----
    #region Singleton
    private static FeedbackUI _instance;
    public static FeedbackUI Instance
    {
        get { if (_instance == null) Debug.LogWarning("[FeedbackUI] No hay instancia."); return _instance; }
    }
    public static bool HasInstance() => _instance != null;

    private void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
    }
    private void OnDestroy() { if (this == _instance) _instance = null; }
    #endregion

    // ---- INSPECTOR ----
    #region Inspector
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

    [Header("Timing")]
    [SerializeField] private float PickupDuration = 2.5f;
    [SerializeField] private float DoorDuration = 2f;
    #endregion

    // ---- PRIVADOS ----
    #region Privados
    private VisualElement _pickupCard;
    private VisualElement _pickupIcon;
    private Label _pickupLabel;
    private Label _pickupSublabel;

    private VisualElement _doorCard;
    private VisualElement _doorIcon;
    private Label _doorLabel;
    private Label _doorSublabel;

    private float _pickupTimer = 0f;
    private bool _pickupActive = false;
    private float _doorTimer = 0f;
    private bool _doorActive = false;

    private bool _ready = false;

    private const string CSS_VISIBLE = "feedback-card--visible";
    private const string CSS_LOCKED = "feedback-card--locked";
    private const string CSS_UNLOCKED = "feedback-card--unlocked";
    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour
    private void Start()
    {
        UIDocument doc = GetComponent<UIDocument>();
        if (doc == null) { Debug.LogError("[FeedbackUI] No hay UIDocument."); return; }

        VisualElement root = doc.rootVisualElement;
        if (root == null) { Debug.LogError("[FeedbackUI] rootVisualElement null."); return; }

        _pickupCard = root.Q<VisualElement>("pickupCard");
        _pickupIcon = root.Q<VisualElement>("pickupIcon");
        _pickupLabel = root.Q<Label>("pickupLabel");
        _pickupSublabel = root.Q<Label>("pickupSublabel");

        _doorCard = root.Q<VisualElement>("doorCard");
        _doorIcon = root.Q<VisualElement>("doorIcon");
        _doorLabel = root.Q<Label>("doorLabel");
        _doorSublabel = root.Q<Label>("doorSublabel");

        if (_pickupCard == null || _doorCard == null)
        {
            Debug.LogError("[FeedbackUI] pickupCard o doorCard no encontrados en el UXML.");
            return;
        }

        // Empezar ocultos (sin la clase visible)
        _pickupCard.RemoveFromClassList(CSS_VISIBLE);
        _doorCard.RemoveFromClassList(CSS_VISIBLE);
        _ready = true;
    }

    private void Update()
    {
        if (!_ready) { return; }

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

    /// <summary>Muestra tarjeta de pickup con nombre, icono y cantidad.</summary>
    public void MostrarPickup(string nombre, Sprite icono, int cantidad)
    {
        if (!_ready) { return; }

        if (_pickupLabel != null) _pickupLabel.text = nombre;
        if (_pickupSublabel != null)
            _pickupSublabel.text = cantidad >= 0 ? $"Total: {cantidad}" : "";

        SetIconSprite(_pickupIcon, icono);

        _pickupCard.AddToClassList(CSS_VISIBLE);
        _pickupTimer = PickupDuration;
        _pickupActive = true;
    }

    /// <summary>Detecta nombre e icono automáticamente según el tipo de objeto.</summary>
    public void MostrarPickupTipo(Objects.ObjectsType tipo, int cantidad)
    {
        if (!_ready) { return; }
        string nombre;
        Sprite icono;
        int c = cantidad;

        switch (tipo)
        {
            case Objects.ObjectsType.fusible: nombre = "Fusible"; icono = SpriteFusible; break;
            case Objects.ObjectsType.key: nombre = "Llave"; icono = SpriteLlave; break;
            case Objects.ObjectsType.bandage: nombre = "Venda"; icono = SpriteVenda; break;
            case Objects.ObjectsType.card: nombre = "Tarjeta de acceso"; icono = SpriteTarjeta; break;
            case Objects.ObjectsType.multiAbility: nombre = "Habilidad multidireccional"; icono = SpriteHabilidadMulti; c = -1; break;
            case Objects.ObjectsType.explosiveAbility: nombre = "Habilidad explosiva"; icono = SpriteHabilidadExplosiva; c = -1; break;
            default: nombre = "Objeto"; icono = null; break;
        }

        MostrarPickup(nombre, icono, c);
    }

    /// <summary>Muestra tarjeta de estado de puerta.</summary>
    public void MostrarPuerta(bool bloqueada, string mensaje, string submensaje = "")
    {
        if (!_ready) { return; }

        if (_doorLabel != null) _doorLabel.text = mensaje;
        if (_doorSublabel != null) _doorSublabel.text = submensaje;

        SetIconSprite(_doorIcon, bloqueada ? SpritePuertaBloqueada : SpritePuertaAbierta);

        // Cambiar color del borde según estado
        _doorCard.RemoveFromClassList(CSS_LOCKED);
        _doorCard.RemoveFromClassList(CSS_UNLOCKED);
        _doorCard.AddToClassList(bloqueada ? CSS_LOCKED : CSS_UNLOCKED);

        _doorCard.AddToClassList(CSS_VISIBLE);
        _doorTimer = DoorDuration;
        _doorActive = true;
    }

    #endregion

    // ---- HELPERS ----
    #region Helpers
    private void SetIconSprite(VisualElement iconEl, Sprite sprite)
    {
        if (iconEl == null) { return; }
        if (sprite != null)
        {
            iconEl.style.backgroundImage = new StyleBackground(sprite);
            iconEl.style.display = DisplayStyle.Flex;
        }
        else
        {
            iconEl.style.display = DisplayStyle.None;
        }
    }
    #endregion

} // class FeedbackUI