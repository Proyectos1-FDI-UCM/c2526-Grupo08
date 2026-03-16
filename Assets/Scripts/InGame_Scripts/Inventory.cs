//---------------------------------------------------------
// Gestiona el inventario del jugador: vendas, llaves y fusibles.
// El uso de vendas se controla con la tecla E (acción "Healing").
// La lógica del ascensor está en LevelWin, no aquí.
// Adriana Fernández Luna
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Almacena los objetos recogidos por Cori y gestiona su uso.
///
/// Vendas: se consumen pulsando E y curan al jugador llamando a Health.Healing().
/// Fusibles: se acumulan para activar el ascensor (LevelWin los consulta).
/// Llaves: se acumulan para abrir puertas (pendiente de implementar).
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

    private int _bandage = 0;
    private int _key = 0;
    private int _fusible = 0;
    private int _card = 0;

    private Health _health;

    /// <summary>
    /// Acción de input para usar vendas (tecla E).
    /// Se inicializa en Start, no en OnEnable, para evitar NullReferenceException
    /// ya que OnEnable se ejecuta antes que Start.
    /// </summary>
    private InputAction _healthAction;

    /// <summary>Evita registrar el evento dos veces.</summary>
    private bool _inputRegistered = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

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

        // Registrar aquí, después de que _healthAction esté inicializado
        RegisterInput();
    }

    private void Update()
    {
        
    }

    private void OnEnable()
    {
        // Solo registrar si Start ya se ejecutó (re-activaciones posteriores del GameObject)
        if (_healthAction != null)
            RegisterInput();
    }

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

    /// <summary>Devuelve el número de fusibles en el inventario.</summary>
    
    public int GetCardCount() => _card;

    public bool hasKey = false;
    //Esto lo ha hecho Marián
    public void CollectKey()
    {
        hasKey = true;
        Debug.Log("Llave recogida");
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

    private void RegisterInput()
    {
        if (_inputRegistered || _healthAction == null) return;
        _healthAction.Enable();
        _healthAction.performed += OnUseBandage;
        _inputRegistered = true;
    }

    private void UnregisterInput()
    {
        if (!_inputRegistered || _healthAction == null) return;
        _healthAction.performed -= OnUseBandage;
        _healthAction.Disable();
        _inputRegistered = false;
    }

    private void OnUseBandage(InputAction.CallbackContext context) => UseBandage();

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