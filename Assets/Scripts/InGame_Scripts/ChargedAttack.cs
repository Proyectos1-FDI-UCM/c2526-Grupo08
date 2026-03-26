//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Responsable de la creación de este archivo
// Nombre del juego
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class ChargedAttack : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Bullet Setup")]
    [Tooltip("Prefab de la bala. Debe tener el componente Bullet.")]
    [SerializeField] private GameObject _bulletPrefab;

    [Tooltip("Punto desde donde sale la bala. Si es null, sale desde el centro del jugador.")]
    [SerializeField] private Transform _shootOrigin;

    [Header("Shoot Settings")]
    [Tooltip("Tiempo mínimo entre disparos en segundos. (GDD: 0,4 s)")]
    [SerializeField] private float _fireRate = 0.4f;

    [Header("Charged Attack")]
    [SerializeField] private float _chargedtime = 1.5f;
    [SerializeField] private int _chargeDamage = 70;
    [SerializeField] private int _chargedMagicCost = 20;
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    private InputAction _attackAction;
    private InputAction _aimAction;

    private float _fireCooldownTimer = 0f;

    private Camera _mainCamera;

    private Magic _magic;

    private float _chargeTimer = 0f;
    private bool _isCharging = false;
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        _attackAction = InputSystem.actions.FindAction("Charged");
        _aimAction = InputSystem.actions.FindAction("HeadPoint");

        if (_attackAction == null || _aimAction == null)
        {
            Debug.LogError("Acción no encontrada.");
            enabled = false;
            return;
        }
        if (_bulletPrefab == null)
        {
            Debug.LogError("No hay prefab de bala asignado en el Inspector.");
            enabled = false;
            return;
        }

        _magic = GetComponent<Magic>();

        _mainCamera = Camera.main;

        if (_shootOrigin == null)
            _shootOrigin = transform;

        _attackAction.Enable();
        _aimAction.Enable();
    }

    private void Update()
    {
        _fireCooldownTimer += Time.deltaTime;
        if (_attackAction.WasPressedThisFrame())
        {
            _isCharging = true;
            _chargeTimer = 0f;
        }

        if (_isCharging && _attackAction.IsPressed())
        {
            _chargeTimer += Time.deltaTime;
            if (_chargeTimer >= _chargedtime)
            {
                TryChargedShot();
                _isCharging = false;
                _fireCooldownTimer = 0f;
            }
        }

        if (_attackAction.WasReleasedThisFrame())
        {
            _isCharging = false;
        }
    }

    public bool IsCharging()
    {
        return _isCharging;
    }
    // Por defecto están los típicos (Update y Start) pero:
    // - Hay que añadir todos los que sean necesarios
    // - Hay que borrar los que no se usen 

    /// <summary>
    /// Start is called on the frame when a script is enabled just before 
    /// any of the Update methods are called the first time.
    /// </summary>

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Documentar cada método que aparece aquí con ///<summary>
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)
    // Ejemplo: GetPlayerController

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void TryChargedShot()
    {
        if (_magic == null)
        {
            return;
        }

        if (!_magic.TrySpendMagic(_chargedMagicCost))
        {
            return;
        }
        Vector2 shootDirection = GetAimDirection();

        if (shootDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        GameObject bulletObj = Instantiate(_bulletPrefab, _shootOrigin.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.Init(shootDirection, _chargeDamage);
        }
    }

    private Vector2 GetAimDirection()
    {
        Vector2 rawAim = _aimAction.ReadValue<Vector2>();
        bool isMouse = Mouse.current != null && _aimAction.activeControl?.device is Mouse;

        if (isMouse)
        {
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(
                new Vector3(rawAim.x, rawAim.y, 0f));
            return ((Vector2)worldPos - (Vector2)transform.position).normalized;
        }
        else
        {
            return rawAim.normalized;
        }
    }
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    #endregion   

} // class ChargedAttack 
// Carlos Mesa Torres
