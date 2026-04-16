//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Responsable de la creación de este archivo
// Nombre del juego
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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
    [SerializeField] private float _chargedTime = 1.5f;
    [SerializeField] private int _chargeDamage = 70;
    [SerializeField] private int _chargedMagicCost = 20;

    [Header("Charge Visual")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _chargeColor = Color.cyan;

    [Header("Charge Aura")]
    [SerializeField] private ParticleSystem _chargeAura;
    [SerializeField] private float _maxEmission = 40f;
    [SerializeField] private float _rotationSpeed = 180f;
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    private InputAction _attackAction;

    private InputAction _aimGamepad;
    private InputAction _aimMouse;

    private Camera _mainCamera;

    private Magic _magic;

    private float _chargeTimer = 0f;
    private bool _isCharging = false;
    private bool _canCharge = false;

    private Color _originalColor;
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
        //_aimAction = InputSystem.actions.FindAction("HeadPoint");
        _aimMouse = InputSystem.actions.FindAction("HeadPoint1");
        _aimGamepad = InputSystem.actions.FindAction("HeadPoint2");

        if (_attackAction == null || _aimMouse == null || _aimGamepad == null)
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
        _aimGamepad.Enable();
        _aimMouse.Enable();

        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }

        if (_chargeAura != null)
        {
            _chargeAura.Stop();
        }
    }

    private void Update()
    {
        if (_attackAction.WasPressedThisFrame())
        {
            if (_magic != null && _magic.HasEnoughMagic(_chargedMagicCost))
            {
                _isCharging = true;
                _canCharge = true;
                _chargeTimer = 0f;
            }
            else
            {
                _isCharging = false;
                _canCharge = false;
            }
        }

        if (_isCharging && _canCharge && _attackAction.IsPressed())
        {
            _chargeTimer += Time.deltaTime;
            float chargePercent = _chargeTimer / _chargedTime;

            if (_spriteRenderer != null)
            {
                float pulse = Mathf.Sin(Time.time * 25f) * 0.1f;

                _spriteRenderer.color = Color.Lerp(_originalColor, _chargeColor, Mathf.Clamp01(chargePercent + pulse));
            }
            
            if (_chargeAura != null)
            {
                if (!_chargeAura.isPlaying)
                {
                    _chargeAura.Play();
                }
                var emission = _chargeAura.emission;
                emission.rateOverTime = Mathf.Lerp(5f, _maxEmission, chargePercent);
                _chargeAura.transform.position = transform.position;
                _chargeAura.transform.Rotate(0, 0, _rotationSpeed* Time.deltaTime);
            }

            if (_chargeTimer >= _chargedTime)
            {
                TryChargedShot();
                _isCharging = false;
                _canCharge = false;
                ResetChargeVisual();
            }
        }

        if (_attackAction.WasReleasedThisFrame())
        {
            _isCharging = false;
            _canCharge = false;
            ResetChargeVisual();
        }
    }

    private void ResetChargeVisual()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
        
        if (_chargeAura != null )
        {
            _chargeAura.Stop();
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
        Vector2 rawAim = _aimGamepad.ReadValue<Vector2>();

        if (rawAim.magnitude > 0.1f)
        {
            Debug.Log(rawAim);
            return rawAim.normalized;
        }
        else
        {
            Vector2 mousePos = _aimMouse.ReadValue<Vector2>();
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

            return ((Vector2)worldPos - (Vector2)transform.position).normalized;
        }
    }
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    #endregion   

} // class ChargedAttack 
// Carlos Mesa Torres
