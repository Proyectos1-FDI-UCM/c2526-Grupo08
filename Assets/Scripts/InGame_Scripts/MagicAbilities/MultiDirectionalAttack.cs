//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Responsable de la creación de este archivo
// No way down
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
public class MultiDirectionalAttack : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)


    [Header("Bullet Setup")]
    [Tooltip("Prefab de la bala. Debe tener el componente Bullet.")]
    [SerializeField] private GameObject BulletPrefab;

    [Tooltip("Punto desde donde sale la bala. Si es null, sale desde el centro del jugador.")]
    [SerializeField] private Transform ShootOrigin;

    [Header("MultiDirectionalAttack")]
    [SerializeField] private float FireRate = 0.4f;
    [SerializeField] private int Damage = 30;
    [SerializeField] private float Range = 8f;
    [SerializeField] private int MagicCost = 30;
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    private InputAction _attackAction;
    private float _cooldownTimer = 0f;
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

    private void Start()
    {
        _attackAction = InputSystem.actions.FindAction("MultiDir_Explosion");

        if (_attackAction == null)
        {
            Debug.LogError("Acción no encontrada.");
            enabled = false;
            return;
        }

        if (BulletPrefab == null)
        {
            Debug.LogError("No hay prefab de bala asignado en el Inspector.");
            enabled = false;
            return;
        }

        _magic = GetComponent<Magic>();

        if (ShootOrigin == null)
            ShootOrigin = transform;

        _attackAction.Enable();

        _cooldownTimer = FireRate;
    }

    private void Update()
    {
        _cooldownTimer += Time.deltaTime;

        //Dispara si se pulsa y no esta con cooldown
        if (_attackAction.WasPressedThisFrame() && _cooldownTimer >= FireRate)
        {
            TryShoot();
        }
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

    private void TryShoot()
    {
        if (_magic == null)
        {
            return;
        }
        if (!_magic.TrySpendMagic(MagicCost))
        {
            return;
        }

        ShootMulti();
        _cooldownTimer = 0f;
    }

    private void ShootMulti()
    {
        //Direcciones en diagonal
        Vector2[] directions = new Vector2[]
        {
            new Vector2(1,1).normalized,
            new Vector2(-1,1).normalized,
            new Vector2(-1,-1).normalized,
            new Vector2(1,-1).normalized
        };

        foreach (Vector2 dir in  directions)
        {
            GameObject bulletObj = Instantiate(BulletPrefab,ShootOrigin.position, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();

            if (bullet != null)
            {
                bullet.SetRange(Range);
                bullet.Init(dir, Damage);
            }
        }
    }
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    #endregion

} // class MultiDirectionalAttack 
// Carlos Mesa Torres
