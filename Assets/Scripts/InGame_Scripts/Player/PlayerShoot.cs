//---------------------------------------------------------
// Componente que gestiona el disparo básico del jugador.
// Instancia balas en la dirección del cursor (teclado/ratón)
// o del joystick derecho (mando), con cooldown configurable.
// Alexia Pérez Santana — Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el sistema de disparo básico del jugador.
/// Al mantener pulsado el botón de ataque, instancia balas con cooldown.
/// Según el GDD: cooldown 0,4 s, daño 20, rango 12 casillas.
///
/// El cooldown usa un acumulador con Time.deltaTime en lugar de Time.time
/// para evitar problemas si el juego se pausa o si el timeScale cambia.
/// </summary>
public class PlayerShoot : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Bullet Setup")]
    [Tooltip("Prefab de la bala. Debe tener el componente Bullet.")]
    [SerializeField] private GameObject BulletPrefab;

    [Tooltip("Punto desde donde sale la bala. Si es null, sale desde el centro del jugador.")]
    [SerializeField] private Transform ShootOrigin;

    [Header("Shoot Settings")]
    [Tooltip("Tiempo mínimo entre disparos en segundos. (GDD: 0,4 s)")]
    [SerializeField] private float FireRate = 0.4f;

    [Tooltip("Daño que aplica cada bala al impactar. (GDD ataque básico: 20)")]
    [SerializeField] private int BulletDamage = 20;

    [Header("Umbrales de apuntado")]
    [Tooltip("Magnitud mínima de la dirección de disparo para considerarla válida.")]
    [SerializeField] private float MinAimDirectionSqr = 0.01f;

    [Tooltip("Magnitud mínima del stick derecho del mando para usarlo como dirección de apuntado " +
             "(por debajo de este valor se usa la posición del ratón).")]
    [SerializeField] private float GamepadAimDeadzone = 0.1f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Acción de Input System para disparar.</summary>
    private InputAction _attackAction;

    /// <summary>Acción de Input System para apuntar con el stick derecho del mando.</summary>
    private InputAction _aimGamepad;

    /// <summary>Acción de Input System para apuntar con la posición del ratón.</summary>
    private InputAction _aimMouse;

    /// <summary>
    /// Acumulador de tiempo desde el último disparo.
    /// Usa deltaTime para ser independiente del timeScale y de Time.time.
    /// Se inicializa a FireRate para poder disparar desde el primer frame.
    /// </summary>
    private float _fireCooldownTimer = 0f;

    /// <summary>Cámara principal, usada para convertir la posición del ratón a coordenadas de mundo.</summary>
    private Camera _mainCamera;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Obtiene las acciones de Input System, valida que el prefab de bala
    /// esté asignado y deja el arma lista para disparar desde el primer frame.
    /// </summary>
    private void Start()
    {
        _attackAction = InputSystem.actions.FindAction("Attack");
        if (_attackAction == null)
        {
            Debug.LogError("[PlayerShoot] Acción 'Attack' no encontrada.");
            enabled = false;
            return;
        }

        _aimMouse = InputSystem.actions.FindAction("HeadPoint1");
        if (_aimMouse == null)
        {
            Debug.LogError("[PlayerShoot] Acción 'HeadPoint1' no encontrada.");
            enabled = false;
            return;
        }

        _aimGamepad = InputSystem.actions.FindAction("HeadPoint2");
        if (_aimGamepad == null)
        {
            Debug.LogError("[PlayerShoot] Acción 'HeadPoint2' no encontrada.");
            enabled = false;
            return;
        }

        if (BulletPrefab == null)
        {
            Debug.LogError("[PlayerShoot] No hay prefab de bala asignado en el Inspector.");
            enabled = false;
            return;
        }

        _mainCamera = Camera.main;

        if (ShootOrigin == null)
            ShootOrigin = transform;

        _attackAction.Enable();
        _aimGamepad.Enable();
        _aimMouse.Enable();

        // Listo para disparar desde el primer frame
        _fireCooldownTimer = FireRate;
    }

    /// <summary>
    /// Cada frame, acumula el tiempo desde el último disparo y dispara
    /// si el botón de ataque está pulsado y el cooldown ha terminado.
    /// </summary>
    private void Update()
    {
        _fireCooldownTimer += Time.deltaTime;

        if (_attackAction.IsInProgress() && _fireCooldownTimer >= FireRate)
        {
            Shoot();
            _fireCooldownTimer = 0f;
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
    /// Calcula la dirección de disparo y, si es válida, instancia la bala
    /// e inicializa su dirección y daño.
    /// </summary>
    private void Shoot()
    {
        Vector2 shootDirection = GetAimDirection();
        if (shootDirection.sqrMagnitude < MinAimDirectionSqr) return;

        GameObject bulletObj = Instantiate(BulletPrefab, ShootOrigin.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Init(shootDirection, BulletDamage);
        else
            Debug.LogWarning("[PlayerShoot] El prefab de bala no tiene el componente Bullet.");
    }

    /// <summary>
    /// Devuelve la dirección normalizada de disparo.
    /// Ratón: apunta hacia el cursor en coordenadas de mundo.
    /// Mando: usa el joystick derecho directamente si supera la zona muerta.
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

} // class PlayerShoot
  // Alexia Pérez Santana — Marián Navarro Santoyo