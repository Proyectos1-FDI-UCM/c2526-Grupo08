//---------------------------------------------------------
// Controlador de feedback visual ingame usando UI Toolkit.
// Muestra tarjetas animadas en la esquina inferior izquierda para:
//   · Recoger un objeto  → nombre del objeto + cantidad total
//   · Acercarse a puerta → "Bloqueada: falta una llave" / "¡Abierta!"
// Cada tarjeta se muestra durante un tiempo configurable y desaparece
// con transición de opacidad (USS transition).
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Singleton local por escena.
///
/// Uso desde otros scripts:
///
///   // Al recoger un objeto:
///   FeedbackUI.Instance.MostrarPickup("Fusible", Sprites.Fusible, 2);
///
///   // Al acercarse a una puerta bloqueada:
///   FeedbackUI.Instance.MostrarPuerta(bloqueada: true, "Falta una llave");
///
///   // Al abrir la puerta:
///   FeedbackUI.Instance.MostrarPuerta(bloqueada: false, "¡Puerta abierta!");
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class FeedbackUI : MonoBehaviour
{
    // ---- SINGLETON ----
    #region Singleton local de escena

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
    [Tooltip("Segundos que se muestra la tarjeta de pickup antes de desaparecer.")]
    [SerializeField] private float PickupDuration = 2.5f;

    [Tooltip("Segundos que se muestra la tarjeta de puerta antes de desaparecer.")]
    [SerializeField] private float DoorDuration = 2f;

    [Header("Sprites de objetos")]
    [Tooltip("Sprite del fusible (se muestra en la tarjeta de pickup).")]
    [SerializeField] private Sprite SpriteFusible;

    [Tooltip("Sprite de la llave genérica.")]
    [SerializeField] private Sprite SpriteLlave;

    [Tooltip("Sprite de la venda.")]
    [SerializeField] private Sprite SpriteVenda;

    [Tooltip("Sprite de la tarjeta de acceso.")]
    [SerializeField] private Sprite SpriteTarjeta;

    [Tooltip("Sprite de la llave especial.")]
    [SerializeField] private Sprite SpriteLlaveEspecial;

    [Tooltip("Sprite de la habilidad multidireccional.")]
    [SerializeField] private Sprite SpriteHabilidadMulti;

    [Tooltip("Sprite de la habilidad explosiva.")]
    [SerializeField] private Sprite SpriteHabilidadExplosiva;

    [Tooltip("Sprite icono de puerta bloqueada.")]
    [SerializeField] private Sprite SpritePuertaBloqueada;

    [Tooltip("Sprite icono de puerta abierta.")]
    [SerializeField] private Sprite SpritePuertaAbierta;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    // ── Elementos UI ──
    private VisualElement _doorCard;
    private VisualElement _doorIcon;
    private Label _doorLabel;
    private Label _doorSublabel;

    private VisualElement _pickupCard;
    private VisualElement _pickupIcon;
    private Label _pickupLabel;
    private Label _pickupSublabel;

    // ── Timers (acumuladores, sin corrutinas) ──
    private float _pickupTimer = 0f;
    private float _doorTimer = 0f;
    private bool _pickupActive = false;
    private bool _doorActive = false;

    private const string CSS_VISIBLE = "feedback-card--visible";
    private const string CSS_LOCKED = "feedback-card--locked";
    private const string CSS_UNLOCKED = "feedback-card--unlocked";

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void OnEnable()
    {
        UIDocument doc = GetComponent<UIDocument>();
        VisualElement root = doc.rootVisualElement;

        _doorCard = root.Q<VisualElement>("doorCard");
        _doorIcon = root.Q<VisualElement>("doorIcon");
        _doorLabel = root.Q<Label>("doorLabel");
        _doorSublabel = root.Q<Label>("doorSublabel");

        _pickupCard = root.Q<VisualElement>("pickupCard");
        _pickupIcon = root.Q<VisualElement>("pickupIcon");
        _pickupLabel = root.Q<Label>("pickupLabel");
        _pickupSublabel = root.Q<Label>("pickupSublabel");

        // Estado inicial: ocultos
        _doorCard.RemoveFromClassList(CSS_VISIBLE);
        _pickupCard.RemoveFromClassList(CSS_VISIBLE);
    }

    private void Update()
    {
        // Timer de pickup
        if (_pickupActive)
        {
            _pickupTimer -= Time.deltaTime;
            if (_pickupTimer <= 0f)
            {
                _pickupActive = false;
                _pickupCard.RemoveFromClassList(CSS_VISIBLE);
            }
        }

        // Timer de puerta
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
    /// Muestra la tarjeta de pickup.
    /// Llamar desde Objects.cs (o desde Inventory.cs) al recoger un objeto.
    /// </summary>
    /// <param name="nombreObjeto">Nombre a mostrar ("Fusible", "Venda", etc.)</param>
    /// <param name="icono">Sprite del objeto. Puede ser null (sin icono).</param>
    /// <param name="cantidad">Cantidad total que tiene el jugador ahora.</param>
    public void MostrarPickup(string nombreObjeto, Sprite icono, int cantidad)
    {
        if (_pickupCard == null) { return; }

        _pickupLabel.text = nombreObjeto;
        _pickupSublabel.text = $"Total: <color=#79e2d6><b>{cantidad}</b></color>";

        SetIconSprite(_pickupIcon, icono);

        _pickupTimer = PickupDuration;
        _pickupActive = true;
        _pickupCard.AddToClassList(CSS_VISIBLE);
    }

    /// <summary>
    /// Atajo: muestra pickup a partir del tipo de objeto del enum de Inventory.
    /// Llama a MostrarPickup con el sprite correcto automáticamente.
    /// </summary>
    public void MostrarPickupTipo(Objects.ObjectsType tipo, int cantidad)
    {
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
                nombre = "Habilidad multidireccional desbloqueda";
                icono = SpriteHabilidadMulti;
                cantidad = -1; // las habilidades no tienen cantidad
                break;
            case Objects.ObjectsType.explosiveAbility:
                nombre = "Habilidad explosiva desbloqueada";
                icono = SpriteHabilidadExplosiva;
                cantidad = -1;
                break;
            default:
                nombre = "Objeto"; icono = null; break;
        }

        if (cantidad < 0)
        {
            // Para habilidades: solo el nombre, sin "Total:"
            if (_pickupCard != null)
            {
                _pickupLabel.text = nombre;
                _pickupSublabel.text = "";
                SetIconSprite(_pickupIcon, icono);
                _pickupTimer = PickupDuration;
                _pickupActive = true;
                _pickupCard.AddToClassList(CSS_VISIBLE);
            }
        }
        else
        {
            MostrarPickup(nombre, icono, cantidad);
        }
    }

    /// <summary>
    /// Muestra la tarjeta de puerta.
    /// Llamar desde Door.cs al intentar abrir una puerta.
    /// </summary>
    /// <param name="bloqueada">True = sin llave (rojo). False = abierta (cyan).</param>
    /// <param name="mensaje">Texto principal ("Bloqueada: falta una llave" / "¡Puerta abierta!").</param>
    /// <param name="submensaje">Texto secundario opcional.</param>
    public void MostrarPuerta(bool bloqueada, string mensaje, string submensaje = "")
    {
        if (_doorCard == null) { return; }

        // Limpiar clases de variante anteriores
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

    /// <summary>
    /// Asigna un sprite a un VisualElement usando backgroundImage (UI Toolkit).
    /// Si el sprite es null, oculta el elemento.
    /// </summary>
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