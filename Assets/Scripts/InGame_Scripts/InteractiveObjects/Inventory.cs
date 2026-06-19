//---------------------------------------------------------
// Gestiona el inventario del jugador: vendas, llaves, fusibles y llave especial.
// El uso de vendas se controla con la tecla E (acción "Healing").
// La lógica del ascensor está en LevelWin, no aquí.
// Adriana Fernández Luna
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Almacena los objetos recogidos por Cori y gestiona su uso.
///
/// Vendas: se consumen pulsando E y curan al jugador llamando a Health.Healing().
/// Fusibles: se acumulan para activar el ascensor (LevelWin los consulta).
/// Llaves genéricas: se acumulan para abrir puertas comunes.
/// Llave especial: flag único que abre solo la habitación secreta de la planta 2.
///
/// LevelManager puede restaurar el inventario al iniciar la escena
/// llamando a SetBandagesFromCheckpoint() y SetKeysFromCheckpoint().
/// </summary>
public class Inventory : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Tooltip("Puntos de vida restaurados por cada venda. (GDD: 20 puntos)")]
    [SerializeField] private int BandageHealth = 20;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Número de vendas en el inventario.</summary>
    private int _bandage = 0;

    /// <summary>Número de llaves genéricas en el inventario.</summary>
    private int _key = 0;

    /// <summary>Número de fusibles en el inventario.</summary>
    private int _fusible = 0;

    /// <summary>Número de tarjetas en el inventario.</summary>
    private int _card = 0;

    /// <summary>Componente Health del jugador, usado para aplicar la curación de las vendas.</summary>
    private Health _health;

    /// <summary>
    /// Acción de input para usar vendas (tecla E).
    /// Se inicializa en Start, no en OnEnable, para evitar NullReferenceException
    /// ya que OnEnable se ejecuta antes que Start.
    /// </summary>
    private InputAction _healthAction;

    /// <summary>Evita registrar el evento dos veces.</summary>
    private bool _inputRegistered = false;

    /// <summary>True si el jugador tiene desbloqueado el disparo multidireccional.</summary>
    private bool _hasMultiAbility = false;

    /// <summary>True si el jugador tiene desbloqueado el disparo explosivo.</summary>
    private bool _hasExplosiveAbility = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea el componente Health del jugador y obtiene/registra la
    /// acción "Healing" del Input System.
    /// </summary>
    private void Start()
    {
        _health = GetComponent<Health>();
        if (_health == null)
            Debug.LogError("[Inventory] No se encontró Health en el jugador.");

        _healthAction = InputSystem.actions.FindAction("Healing");
        if (_healthAction == null)
        {
            Debug.LogError("[Inventory] Acción 'Healing' no encontrada en el Input System.");
            return;
        }

        if (GameManager.HasInstance())
        {
            _hasMultiAbility = GameManager.Instance.HasMultishot();
            _hasExplosiveAbility = GameManager.Instance.HasExplosive();
        }

        RegisterInput();
    }

    /// <summary>Vuelve a registrar la acción de curar al reactivarse el componente.</summary>
    private void OnEnable()
    {
        if (_healthAction != null)
            RegisterInput();
    }

    /// <summary>Desregistra la acción de curar al desactivarse el componente.</summary>
    private void OnDisable()
    {
        UnregisterInput();
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS — CONSULTA ----
    #region Métodos públicos — Consulta

    /// <summary>Devuelve el número de vendas en el inventario.</summary>
    public int GetBandageCount() => _bandage;

    /// <summary>Devuelve el número de llaves en el inventario.</summary>
    public int GetKeyCount() => _key;

    /// <summary>Devuelve el número de fusibles en el inventario.</summary>
    public int GetFusibleCount() => _fusible;

    /// <summary>Devuelve el número de tarjetas en el inventario.</summary>
    public int GetCardCount() => _card;

    /// <summary>True si el jugador tiene desbloqueado el disparo multidireccional.</summary>
    public bool HasMultiAbility() => _hasMultiAbility;

    /// <summary>True si el jugador tiene desbloqueado el disparo explosivo.</summary>
    public bool HasExplosiveAbility() => _hasExplosiveAbility;

    // ---- Llave genérica (puertas comunes) — de Marián ----

    /// <summary>True si el jugador tiene al menos una llave genérica.</summary>
    public bool hasKey { get; set; } = false;

    /// <summary>
    /// Registra que el jugador ha recogido una llave genérica.
    /// Llamado por Objects.cs (tipo key) a través de AddItem,
    /// o directamente por Door.cs según la implementación de Marián.
    /// </summary>
    public void CollectKey()
    {
        hasKey = true;
        Debug.Log("[Inventory] Llave recogida");
    }

    /// <summary>
    /// Consume una llave genérica: decrementa el contador y desactiva el flag hasKey.
    /// Llamado por Door.cs al abrir una puerta.
    /// </summary>
    public void UseKey()
    {
        _key = Mathf.Max(0, _key - 1);
        hasKey = _key > 0; // si aún quedan llaves, el flag sigue activo
        Debug.Log($"[Inventory] Llave usada. Llaves restantes: {_key}");
    }

    // ---- Llave especial (habitación secreta de la planta 2) ----

    /// <summary>
    /// True si el jugador ha recogido la llave especial que dropea el enemigo
    /// especial al ser amenazado. Solo existe una en toda la partida.
    /// La usa SecretDoor.cs para abrir la habitación secreta.
    /// </summary>
    public bool hasSpecialKey { get; set; } = false;

    /// <summary>
    /// Registra que el jugador ha recogido la llave especial.
    /// Llamado por SpecialKeyPickup.cs al recoger el drop del enemigo especial.
    /// </summary>
    public void CollectSpecialKey()
    {
        hasSpecialKey = true;
        Debug.Log("[Inventory] Llave especial recogida");
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS — AÑADIR OBJETOS ----
    #region Métodos públicos — Añadir objetos

    /// <summary>Añade un objeto al inventario según su tipo.</summary>
    public void AddItem(Objects.ObjectsType type)
    {
        switch (type)
        {
            case Objects.ObjectsType.bandage:
                _bandage++;
                Debug.Log($"[Inventory] Vendas: {_bandage}");
                break;
            case Objects.ObjectsType.key:
                _key++;
                CollectKey(); // sincronizar con el flag de Marián
                Debug.Log($"[Inventory] Llaves: {_key}");
                break;
            case Objects.ObjectsType.fusible:
                _fusible++;
                Debug.Log($"[Inventory] Fusibles: {_fusible}");
                break;
            case Objects.ObjectsType.card:
                _card++;
                Debug.Log($"[Inventory] Tarjetas: {_card}");
                break;
            case Objects.ObjectsType.multiAbility:
                _hasMultiAbility = true;
                GameManager.Instance.UnlockMultishot();
                Debug.Log("[Inventory] Habilidad multidireccional desbloqueada");
                break;
            case Objects.ObjectsType.explosiveAbility:
                _hasExplosiveAbility = true;
                GameManager.Instance.UnlockExplosive();
                Debug.Log("[Inventory] Habilidad explosiva desbloqueada");
                break;
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS — CHECKPOINT ----
    #region Métodos públicos — Restaurar desde checkpoint

    /// <summary>
    /// Restaura las vendas al valor del checkpoint.
    /// Llamado por LevelManager al iniciar la escena.
    /// </summary>
    public void SetBandagesFromCheckpoint(int savedBandages)
    {
        _bandage = Mathf.Max(savedBandages, 0);
    }

    /// <summary>
    /// Restaura las llaves al valor del checkpoint.
    /// Llamado por LevelManager al iniciar la escena.
    /// </summary>
    public void SetKeysFromCheckpoint(int savedKeys)
    {
        _key = Mathf.Max(savedKeys, 0);
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>Habilita la acción "Healing" y suscribe OnUseBandage, si no estaba ya registrada.</summary>
    private void RegisterInput()
    {
        if (_inputRegistered || _healthAction == null) return;
        _healthAction.Enable();
        _healthAction.performed += OnUseBandage;
        _inputRegistered = true;
    }

    /// <summary>Desuscribe OnUseBandage y deshabilita la acción "Healing", si estaba registrada.</summary>
    private void UnregisterInput()
    {
        if (!_inputRegistered || _healthAction == null) return;
        _healthAction.performed -= OnUseBandage;
        _healthAction.Disable();
        _inputRegistered = false;
    }

    /// <summary>Callback de la acción "Healing": intenta usar una venda.</summary>
    private void OnUseBandage(InputAction.CallbackContext context) => UseBandage();

    /// <summary>
    /// Si hay vendas disponibles, consume una y cura al jugador BandageHealth
    /// puntos mediante Health.Healing(); si no hay vendas, lo indica por consola.
    /// </summary>
    private void UseBandage()
    {
        if (_health == null) return;

        if (_bandage > 0)
        {
            _bandage--;
            _health.Healing(BandageHealth);
            Debug.Log($"[Inventory] Venda usada. Quedan: {_bandage}");
        }
        else
        {
            Debug.Log("[Inventory] No tienes vendas.");
        }
    }

    #endregion

} // class Inventory
  // Adriana Fernández Luna — Laura Garay Zubiaguirre