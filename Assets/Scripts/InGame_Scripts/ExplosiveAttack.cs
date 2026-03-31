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
public class ExplosiveAttack : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Bullet Setup")]
    [Tooltip("Prefab de la bala. Debe tener el componente Bullet.")]
    [SerializeField] private GameObject _explosiveBulletPrefab;

    [Tooltip("Punto desde donde sale la bala. Si es null, sale desde el centro del jugador.")]
    [SerializeField] private Transform _shootOrigin;

    [Header("Shoot Settings")]
    [SerializeField] private float _fireRate = 0.4f;
    [SerializeField] private int _magicCost = 35;
    [SerializeField] private int _maxUses = 6;

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

    private float _cooldownTimer = 0f;
    private int _remainingUses;

    private Camera _mainCamera;

    private Magic _magic;
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    // Por defecto están los típicos (Update y Start) pero:
    // - Hay que añadir todos los que sean necesarios
    // - Hay que borrar los que no se usen 

    /// <summary>
    /// Start is called on the frame when a script is enabled just before 
    /// any of the Update methods are called the first time.
    /// </summary>
    private void Start()
    {
        _attackAction = InputSystem.actions.FindAction("MultiDir_Explosion");
        _aimMouse = InputSystem.actions.FindAction("HeadPoint1");
        _aimGamepad = InputSystem.actions.FindAction("HeadPoint2");

        if (_attackAction == null || _aimMouse == null || _aimGamepad == null)
        {
            Debug.LogError("Acción no encontrada.");
            enabled = false;
            return;
        }
        if (_explosiveBulletPrefab == null)
        {
            Debug.LogError("No hay prefab de bala asignado en el Inspector.");
            enabled = false;
            return;
        }

        _magic = GetComponent<Magic>();

        _mainCamera = Camera.main;

        if (_shootOrigin == null)
            _shootOrigin = transform;

        _remainingUses = _maxUses;

        _attackAction.Enable();
        _aimGamepad.Enable();
        _aimMouse.Enable();

        _cooldownTimer = _fireRate;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        _cooldownTimer += Time.deltaTime;

        if (_attackAction.WasPressedThisFrame() && _cooldownTimer >= _fireRate)
        {
            TryShoot();
        }
    }
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
    private void TryShoot()
    {
        if (_remainingUses <= 0)
        {
            Debug.Log("No quedan usos del ataque explosivo");
            return;
        }

        if (_magic == null || !_magic.TrySpendMagic(_magicCost))
        {
            return;
        }

        Vector2 dir = GetAimDirection();
        if (dir.sqrMagnitude < 0.01f)
        {
            return;
        }

        float spawnOffset = 2f;
        Vector2 spawnPosition = (Vector2)_shootOrigin.position + dir * spawnOffset;

        GameObject bullet = Instantiate(_explosiveBulletPrefab, spawnPosition, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) 
        {
            rb.linearVelocity = dir * 15f;
        }

        _remainingUses--;
        _cooldownTimer = 0f;
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

} // class ExplosiveAttack 
// Carlos Mesa Torres
