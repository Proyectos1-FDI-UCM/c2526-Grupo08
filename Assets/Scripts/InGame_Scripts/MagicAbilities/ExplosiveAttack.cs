//---------------------------------------------------------
// Gestiona el ataque especial "Explosivo": dispara una bala
// que explota al impactar, con coste de magia y número limitado
// de usos. Controlado por timers en Update, sin corrutinas.
// Carlos Mesa Torres
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Controla el disparo del ataque explosivo del jugador.
/// Al pulsar la acción "MultiDir_Explosion", si hay usos restantes
/// y magia suficiente, instancia una ExplosiveBullet en la dirección
/// de apuntado (mando o ratón) con una velocidad y desplazamiento
/// de aparición fijos.
/// </summary>
public class ExplosiveAttack : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Bullet Setup")]
    [Tooltip("Prefab de la bala explosiva. Debe tener el componente ExplosiveBullet.")]
    [SerializeField] private GameObject ExplosiveBulletPrefab;

    [Tooltip("Punto desde donde sale la bala. Si es null, sale desde el centro del jugador.")]
    [SerializeField] private Transform ShootOrigin;

    [Header("Shoot Settings")]
    [Tooltip("Tiempo mínimo en segundos entre disparos.")]
    [SerializeField] private float FireRate = 0.4f;

    [Tooltip("Cantidad de magia que consume cada disparo.")]
    [SerializeField] private int MagicCost = 35;

    [Tooltip("Número máximo de usos del ataque explosivo disponibles.")]
    [SerializeField] private int MaxUses = 6;

    [Header("Spawn / Velocidad de la bala")]
    [Tooltip("Distancia desde ShootOrigin a la que aparece la bala al disparar.")]
    [SerializeField] private float BulletSpawnOffset = 2f;

    [Tooltip("Velocidad inicial de la bala explosiva al ser disparada.")]
    [SerializeField] private float BulletSpeed = 15f;

    [Header("Umbrales de apuntado")]
    [Tooltip("Magnitud mínima de la dirección de apuntado para considerar válido el disparo.")]
    [SerializeField] private float MinAimDirectionSqr = 0.01f;

    [Tooltip("Magnitud mínima del stick derecho del mando para usarlo como dirección de apuntado " +
             "(por debajo de este valor se usa la posición del ratón).")]
    [SerializeField] private float GamepadAimDeadzone = 0.1f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Acción de Input System para disparar el ataque explosivo.</summary>
    private InputAction _attackAction;

    /// <summary>Acción de Input System para apuntar con el stick derecho del mando.</summary>
    private InputAction _aimGamepad;

    /// <summary>Acción de Input System para apuntar con la posición del ratón.</summary>
    private InputAction _aimMouse;

    /// <summary>Acumulador de tiempo desde el último disparo.</summary>
    private float _cooldownTimer = 0f;

    /// <summary>Número de usos restantes del ataque explosivo.</summary>
    private int _remainingUses;

    /// <summary>Cámara principal, usada para convertir la posición del ratón a coordenadas de mundo.</summary>
    private Camera _mainCamera;

    /// <summary>Componente Magic del jugador, usado para comprobar y restar el coste de magia.</summary>
    private Magic _magic;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Obtiene las acciones de Input System, cachea los componentes necesarios
    /// y valida que el prefab de bala esté asignado.
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
        if (ExplosiveBulletPrefab == null)
        {
            Debug.LogError("No hay prefab de bala asignado en el Inspector.");
            enabled = false;
            return;
        }

        _magic = GetComponent<Magic>();

        _mainCamera = Camera.main;

        if (ShootOrigin == null)
            ShootOrigin = transform;

        _remainingUses = MaxUses;

        _attackAction.Enable();
        _aimGamepad.Enable();
        _aimMouse.Enable();

        _cooldownTimer = FireRate;
    }

    /// <summary>
    /// Cada frame, avanza el temporizador de cadencia y comprueba si se ha
    /// pulsado la acción de ataque para intentar disparar.
    /// </summary>
    private void Update()
    {
        _cooldownTimer += Time.deltaTime;

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
    /// Comprueba usos restantes y magia disponible, calcula la dirección
    /// de apuntado y, si todo es válido, instancia la bala explosiva
    /// con la velocidad configurada y reinicia el temporizador de cadencia.
    /// </summary>
    private void TryShoot()
    {
        if (_remainingUses <= 0)
        {
            Debug.Log("No quedan usos del ataque explosivo");
            return;
        }

        if (_magic == null || !_magic.TrySpendMagic(MagicCost))
        {
            return;
        }

        Vector2 dir = GetAimDirection();
        if (dir.sqrMagnitude < MinAimDirectionSqr)
        {
            return;
        }

        Vector2 spawnPosition = (Vector2)ShootOrigin.position + dir * BulletSpawnOffset;

        GameObject bullet = Instantiate(ExplosiveBulletPrefab, spawnPosition, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * BulletSpeed;
        }

        _remainingUses--;
        _cooldownTimer = 0f;
    }

    /// <summary>
    /// Calcula la dirección de apuntado: si el stick derecho del mando
    /// supera la zona muerta configurada, se usa esa dirección; en caso
    /// contrario, se usa la dirección hacia la posición del ratón en el mundo.
    /// </summary>
    private Vector2 GetAimDirection()
    {
        Vector2 rawAim = _aimGamepad.ReadValue<Vector2>();

        if (rawAim.magnitude > GamepadAimDeadzone)
        {
            return rawAim.normalized;
        }
        else
        {
            Vector2 mousePos = _aimMouse.ReadValue<Vector2>();
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

            return ((Vector2)worldPos - (Vector2)transform.position).normalized;
        }
    }

    #endregion

} // class ExplosiveAttack
  // Carlos Mesa Torres