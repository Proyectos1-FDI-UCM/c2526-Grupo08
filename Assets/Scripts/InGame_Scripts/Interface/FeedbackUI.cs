//---------------------------------------------------------
// Feedback visual ingame usando Canvas legacy (Image + TMP_Text).
// NO usa UI Toolkit — evita los problemas de rutas USS.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Singleton local por escena.
/// Muestra dos tarjetas animadas (slide-in desde izquierda):
///   · PickupCard — icono + nombre + cantidad al recoger objeto
///   · DoorCard   — icono + mensaje al acercarse a una puerta
///
/// ANIMACIÓN: Lerp de anchoredPosition en Update (sin coroutines).
///
/// SETUP EN UNITY:
///   Dentro del Canvas principal de la escena, crea:
///
///   [GameObject] FeedbackUI          ← este script
///   ├── PickupCard (Image)           ← Image con sprite Kenney 9-sliced, anclado bottom-left
///   │   ├── PickupIcon  (Image)      ← 48x48, sin sprite por defecto
///   │   ├── PickupLabel (TMP_Text)   ← bold, 14px
///   │   └── PickupSublabel (TMP_Text)← normal, 11px, color gris
///   └── DoorCard (Image)             ← mismo setup que PickupCard
///       ├── DoorIcon  (Image)
///       ├── DoorLabel (TMP_Text)
///       └── DoorSublabel (TMP_Text)
///
///   RectTransform de cada Card:
///     Anchor preset: bottom-left (min/max = 0,0) | Pivot = (0, 0)
///     PickupCard: anchoredPos = (-320, 24), Width=280, Height=80
///     DoorCard:   anchoredPos = (-320, 116), Width=280, Height=80
///   (el script los mueve a X=24 al mostrarlos)
///
///   Asignar en Inspector del FeedbackUI todos los campos de abajo.
/// </summary>
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

    [Header("Tarjeta Pickup")]
    [SerializeField] private RectTransform PickupCard;
    [SerializeField] private Image PickupIconImg;
    [SerializeField] private TMP_Text PickupLabel;
    [SerializeField] private TMP_Text PickupSublabel;

    [Header("Tarjeta Puerta")]
    [SerializeField] private RectTransform DoorCard;
    [SerializeField] private Image DoorIconImg;
    [SerializeField] private TMP_Text DoorLabel;
    [SerializeField] private TMP_Text DoorSublabel;

    [Header("Posiciones")]
    [SerializeField] private float XVisible = 24f;
    [SerializeField] private float XOculto = -320f;

    [Header("Timing")]
    [SerializeField] private float PickupDuration = 2.5f;
    [SerializeField] private float DoorDuration = 2f;
    [SerializeField] private float AnimSpeed = 12f;

    [Header("Colores del panel")]
    [SerializeField] private Color ColorPickup = new Color(0.18f, 0.35f, 0.37f, 0.95f);
    [SerializeField] private Color ColorBloqueada = new Color(0.40f, 0.30f, 0.08f, 0.95f);
    [SerializeField] private Color ColorAbierta = new Color(0.12f, 0.30f, 0.30f, 0.95f);

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

    // ---- PRIVADOS ----
    #region Privados
    private float _pickupTimer = 0f;
    private bool _pickupActive = false;
    private float _pickupTargetX;

    private float _doorTimer = 0f;
    private bool _doorActive = false;
    private float _doorTargetX;

    private bool _ready = false;
    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour
    private void Start()
    {
        if (PickupCard == null || DoorCard == null)
        {
            Debug.LogError("[FeedbackUI] PickupCard o DoorCard no asignados en el Inspector. " +
                           "Crea la jerarquía Canvas → FeedbackUI → PickupCard / DoorCard.");
            return;
        }

        // Empezar fuera de pantalla
        SetX(PickupCard, XOculto);
        SetX(DoorCard, XOculto);
        _pickupTargetX = XOculto;
        _doorTargetX = XOculto;
        _ready = true;
    }

    private void Update()
    {
        if (!_ready) { return; }

        // Timers
        if (_pickupActive)
        {
            _pickupTimer -= Time.deltaTime;
            if (_pickupTimer <= 0f) { _pickupActive = false; _pickupTargetX = XOculto; }
        }
        if (_doorActive)
        {
            _doorTimer -= Time.deltaTime;
            if (_doorTimer <= 0f) { _doorActive = false; _doorTargetX = XOculto; }
        }

        // Animación slide (Lerp, sin coroutines)
        SetX(PickupCard, Mathf.Lerp(PickupCard.anchoredPosition.x, _pickupTargetX, Time.deltaTime * AnimSpeed));
        SetX(DoorCard, Mathf.Lerp(DoorCard.anchoredPosition.x, _doorTargetX, Time.deltaTime * AnimSpeed));
    }
    #endregion

    // ---- API PÚBLICA ----
    #region API pública

    /// <summary>Muestra tarjeta de pickup con nombre, icono y cantidad.</summary>
    public void MostrarPickup(string nombre, Sprite icono, int cantidad)
    {
        if (!_ready) { return; }
        if (PickupLabel != null) PickupLabel.text = nombre;
        if (PickupSublabel != null) PickupSublabel.text = cantidad >= 0 ? $"Total: {cantidad}" : "";
        SetSprite(PickupIconImg, icono);
        SetPanelColor(PickupCard, ColorPickup);
        _pickupTimer = PickupDuration;
        _pickupActive = true;
        _pickupTargetX = XVisible;
    }

    /// <summary>Detecta nombre e icono automáticamente según el tipo.</summary>
    public void MostrarPickupTipo(Objects.ObjectsType tipo, int cantidad)
    {
        if (!_ready) { return; }
        string nombre; Sprite icono; int c = cantidad;
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
        if (DoorLabel != null) DoorLabel.text = mensaje;
        if (DoorSublabel != null) DoorSublabel.text = submensaje;
        SetSprite(DoorIconImg, bloqueada ? SpritePuertaBloqueada : SpritePuertaAbierta);
        SetPanelColor(DoorCard, bloqueada ? ColorBloqueada : ColorAbierta);
        _doorTimer = DoorDuration;
        _doorActive = true;
        _doorTargetX = XVisible;
    }

    #endregion

    // ---- HELPERS ----
    #region Helpers
    private void SetX(RectTransform rt, float x)
    {
        if (rt == null) { return; }
        var p = rt.anchoredPosition; p.x = x; rt.anchoredPosition = p;
    }

    private void SetSprite(Image img, Sprite sprite)
    {
        if (img == null) { return; }
        img.sprite = sprite;
        img.enabled = sprite != null;
    }

    private void SetPanelColor(RectTransform rt, Color color)
    {
        if (rt == null) { return; }
        Image img = rt.GetComponent<Image>();
        if (img != null) img.color = color;
    }
    #endregion

} // class FeedbackUI