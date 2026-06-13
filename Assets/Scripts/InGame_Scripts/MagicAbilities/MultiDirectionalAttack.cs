//---------------------------------------------------------
// Gestiona el ataque especial "Multidireccional": al pulsar, dispara
// 4 balas en diagonal (una por cada esquina) si hay magia suficiente,
// con cooldown gestionado por timer en Update, sin corrutinas.
// Carlos Mesa Torres
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el ataque especial "Multidireccional" del jugador.
/// Al pulsar la acción "MultiDir_Explosion", si ha pasado el tiempo de
/// cooldown (FireRate) y hay magia suficiente, instancia 4 balas en las
/// direcciones diagonales con el rango y daño configurados.
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
    [Tooltip("Tiempo mínimo entre disparos en segundos.")]
    [SerializeField] private float FireRate = 0.4f;

    [Tooltip("Daño que aplica cada bala diagonal al impactar.")]
    [SerializeField] private int Damage = 30;

    [Tooltip("Rango máximo en unidades de cada bala diagonal antes de destruirse.")]
    [SerializeField] private float Range = 8f;

    [Tooltip("Magia que consume cada uso del ataque multidireccional.")]
    [SerializeField] private int MagicCost = 30;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Acción de Input System para disparar el ataque multidireccional.</summary>
    private InputAction _attackAction;

    /// <summary>Acumulador de tiempo desde el último disparo.</summary>
    private float _cooldownTimer = 0f;

    /// <summary>Componente Magic del jugador, usado para comprobar y restar el coste de magia.</summary>
    private Magic _magic;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Obtiene la acción de Input System, valida que el prefab de bala
    /// esté asignado, cachea el componente Magic y deja el ataque listo
    /// para usarse desde el primer frame.
    /// </summary>
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

    /// <summary>
    /// Cada frame, acumula el tiempo desde el último disparo e intenta
    /// disparar si se ha pulsado la acción y el cooldown ha terminado.
    /// </summary>
    private void Update()
    {
        _cooldownTimer += Time.deltaTime;

        // Dispara si se pulsa y no está en cooldown
        if (_attackAction.WasPressedThisFrame() && _cooldownTimer >= FireRate)
        {
            TryShoot();
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Esta clase no expone métodos públicos.
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Si hay magia suficiente, gasta el coste, dispara las 4 balas
    /// diagonales y reinicia el cooldown.
    /// </summary>
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

    /// <summary>
    /// Instancia una bala en cada una de las 4 direcciones diagonales,
    /// aplicándoles el rango y el daño configurados.
    /// </summary>
    private void ShootMulti()
    {
        // Direcciones en diagonal
        Vector2[] directions = new Vector2[]
        {
            new Vector2(1,1).normalized,
            new Vector2(-1,1).normalized,
            new Vector2(-1,-1).normalized,
            new Vector2(1,-1).normalized
        };

        foreach (Vector2 dir in directions)
        {
            GameObject bulletObj = Instantiate(BulletPrefab, ShootOrigin.position, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();

            if (bullet != null)
            {
                bullet.SetRange(Range);
                bullet.Init(dir, Damage);
            }
        }
    }

    #endregion

} // class MultiDirectionalAttack
  // Carlos Mesa Torres