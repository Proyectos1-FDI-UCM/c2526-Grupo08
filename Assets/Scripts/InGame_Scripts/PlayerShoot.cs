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
    [SerializeField] private GameObject _bulletPrefab;

    [Tooltip("Punto desde donde sale la bala. Si es null, sale desde el centro del jugador.")]
    [SerializeField] private Transform _shootOrigin;

    [Header("Shoot Settings")]
    [Tooltip("Tiempo mínimo entre disparos en segundos. (GDD: 0,4 s)")]
    [SerializeField] private float _fireRate = 0.4f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    private InputAction _attackAction;
    private InputAction _aimAction;

    /// <summary>
    /// Acumulador de tiempo desde el último disparo.
    /// Usa deltaTime para ser independiente del timeScale y de Time.time.
    /// Se inicializa a _fireRate para poder disparar desde el primer frame.
    /// </summary>
    private float _fireCooldownTimer = 0f;

    private Camera _mainCamera;

    private Magic _magic;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        _attackAction = InputSystem.actions.FindAction("Attack");
        if (_attackAction == null)
        {
            Debug.LogError("[PlayerShoot] Acción 'Attack' no encontrada.");
            enabled = false;
            return;
        }

        _aimAction = InputSystem.actions.FindAction("HeadPoint");
        if (_aimAction == null)
        {
            Debug.LogError("[PlayerShoot] Acción 'HeadPoint' no encontrada.");
            enabled = false;
            return;
        }

        if (_bulletPrefab == null)
        {
            Debug.LogError("[PlayerShoot] No hay prefab de bala asignado en el Inspector.");
            enabled = false;
            return;
        }

        _magic = GetComponent<Magic>();

        _mainCamera = Camera.main;

        if (_shootOrigin == null)
            _shootOrigin = transform;

        _attackAction.Enable();
        _aimAction.Enable();

        // Listo para disparar desde el primer frame
        _fireCooldownTimer = _fireRate;
    }

    private void Update()
    {
        // Acumular tiempo transcurrido desde el último disparo
        _fireCooldownTimer += Time.deltaTime;

        // Disparar si el botón está pulsado y el cooldown ha pasado
        if (_attackAction.WasPressedThisFrame() && _fireCooldownTimer >= _fireRate)
        {
            Shoot();
            //_magic.DecreaseMagic();  DE PRUEBA, BORRAR LUEGO
            _fireCooldownTimer = 0f;
        }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Calcula la dirección de disparo y lanza la bala.
    /// </summary>
    private void Shoot()
    {
        Vector2 shootDirection = GetAimDirection();
        if (shootDirection.sqrMagnitude < 0.01f) return;

        GameObject bulletObj = Instantiate(_bulletPrefab, _shootOrigin.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Init(shootDirection, 20);
        else
            Debug.LogWarning("[PlayerShoot] El prefab de bala no tiene el componente Bullet.");
    }

    /// <summary>
    /// Devuelve la dirección normalizada de disparo.
    /// Ratón: apunta hacia el cursor en coordenadas de mundo.
    /// Mando: usa el joystick derecho directamente.
    /// </summary>
    private Vector2 GetAimDirection()
    {
        Vector2 rawAim = _aimAction.ReadValue<Vector2>();
        bool isMouse = Mouse.current != null && _aimAction.activeControl?.device is Mouse;
        //bool isController = Gamepad.current != null && _aimAction.activeControl?.device is Gamepad;

        if (isMouse)
        {
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(rawAim.x, rawAim.y, 0f));
            return ((Vector2)worldPos - (Vector2)transform.position).normalized;
        }
        else 
        {
            return rawAim.normalized;
        }
    }

    #endregion

} // class PlayerShoot