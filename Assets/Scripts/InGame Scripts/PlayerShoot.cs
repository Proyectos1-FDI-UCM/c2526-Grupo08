//---------------------------------------------------------
// Componente que gestiona el disparo básico del jugador.
// Instancia balas en la dirección del cursor (teclado/ratón) 
// o del joystick derecho (mando), con cooldown configurable.
// Alexia y Marián
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el sistema de disparo básico del jugador.
/// Al pulsar el botón de ataque, instancia un prefab de bala
/// que se dirige hacia la posición del cursor del ratón (teclado)
/// o la dirección del joystick derecho (mando).
/// Según el GDD: cooldown de 0,4 segundos, daño de 20, rango de 12 casillas.
/// </summary>
public class PlayerShoot : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Bullet Setup")]
    [Tooltip("Prefab de la bala que se instanciará al disparar. Debe tener el componente Bullet.")]
    [SerializeField] private GameObject _bulletPrefab;

    [Tooltip("Punto desde donde sale la bala. Si es null, sale desde el centro del jugador.")]
    [SerializeField] private Transform _shootOrigin;

    [Header("Shoot Settings")]
    [Tooltip("Tiempo mínimo entre disparos en segundos. (GDD: 0,4 segundos)")]
    [SerializeField] private float _fireRate = 0.4f;

    // ---- SOLO PARA PRUEBA - BORRAR ANTES DE ENTREGAR ----
    [Header("TEST - Camera Shake (BORRAR ANTES DE ENTREGAR)")]
    [Tooltip("TEST: Activar temblor de cámara al disparar para probar CameraController.")]
    [SerializeField] private bool _testShakeOnShoot = true;
    // ---- FIN BLOQUE DE PRUEBA ----

    #endregion


    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Acción de input del ataque básico.</summary>
    private InputAction _attackAction;

    /// <summary>Acción que devuelve la posición del cursor o el joystick derecho.</summary>
    private InputAction _aimAction;

    /// <summary>Tiempo en que se podrá volver a disparar.</summary>
    private float _nextFireTime = 0f;

    /// <summary>Cámara principal, necesaria para convertir posición de pantalla a mundo.</summary>
    private Camera _mainCamera;

    // ---- SOLO PARA PRUEBA - BORRAR ANTES DE ENTREGAR ----
    /// <summary>TEST: Referencia a la cámara para probar el shake. Borrar con el bloque de prueba.</summary>
    private CameraController _cameraController;
    // ---- FIN BLOQUE DE PRUEBA ----

    #endregion


    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Start se llama en el primer frame. Busca las acciones de input y valida el prefab.
    /// </summary>
    private void Start()
    {
        // Buscamos la acción de disparo en el Input Actions Asset
        _attackAction = InputSystem.actions.FindAction("Attack");
        if (_attackAction == null)
        {
            Debug.LogError("[PlayerShoot] Acción 'Attack' no encontrada en el Input Actions Asset.");
            enabled = false;
            return;
        }

        // Buscamos la acción de apuntar (posición del cursor o joystick derecho)
        _aimAction = InputSystem.actions.FindAction("HeadPoint");
        if (_aimAction == null)
        {
            Debug.LogError("[PlayerShoot] Acción 'HeadPoint' no encontrada en el Input Actions Asset.");
            enabled = false;
            return;
        }

        // Validamos que el prefab esté asignado
        if (_bulletPrefab == null)
        {
            Debug.LogError("[PlayerShoot] No has asignado el prefab de la bala en el inspector.");
            enabled = false;
            return;
        }

        _mainCamera = Camera.main;

        // Si no se ha asignado un punto de origen, usamos el transform del jugador
        if (_shootOrigin == null)
        {
            _shootOrigin = transform;
        }

        // ---- SOLO PARA PRUEBA - BORRAR ANTES DE ENTREGAR ----
        // Buscamos el CameraController en la escena para probar el shake
        _cameraController = FindObjectOfType<CameraController>();
        if (_cameraController == null)
        {
            Debug.LogWarning("[TEST] No se encontró CameraController en la escena. " +
                             "Añade el script CameraController a la Main Camera para probar el shake.");
        }
        // ---- FIN BLOQUE DE PRUEBA ----
    }

    /// <summary>
    /// Update comprueba cada frame si se ha pulsado el botón de disparo
    /// y ha pasado suficiente tiempo desde el último disparo.
    /// </summary>
    private void Update()
    {
        // Comprobamos: ¿botón pulsado? Y ¿ha pasado el cooldown?
        if (_attackAction.IsPressed() && Time.time >= _nextFireTime)
        {
            Shoot();
            // Actualizamos el tiempo del próximo disparo permitido
            _nextFireTime = Time.time + _fireRate;
        }
    }

    #endregion


    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Calcula la dirección de disparo según el dispositivo de entrada y lanza la bala.
    /// Con ratón: apunta hacia el cursor en el mundo.
    /// Con mando: apunta en la dirección del joystick derecho.
    /// </summary>
    private void Shoot()
    {
        Vector2 shootDirection = GetAimDirection();

        // Si la dirección es prácticamente cero (joystick sin mover), no disparamos
        if (shootDirection.sqrMagnitude < 0.01f) return;

        // Instanciamos la bala en el punto de origen
        GameObject bulletObj = Instantiate(_bulletPrefab, _shootOrigin.position, Quaternion.identity);

        // Inicializamos la bala con la dirección calculada
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(shootDirection);
        }
        else
        {
            Debug.LogWarning("[PlayerShoot] El prefab de bala no tiene el componente Bullet.");
        }

        // ---- SOLO PARA PRUEBA - BORRAR ANTES DE ENTREGAR ----
        // Activamos el temblor de cámara al disparar para probar que CameraController funciona
        if (_testShakeOnShoot && _cameraController != null)
        {
            _cameraController.TriggerShake();
        }
        // ---- FIN BLOQUE DE PRUEBA ----
    }

    /// <summary>
    /// Devuelve la dirección normalizada hacia la que debe disparar el jugador.
    /// Si se usa ratón, calcula el vector desde el jugador hasta el cursor en el mundo.
    /// Si se usa mando, usa directamente el joystick derecho.
    /// </summary>
    /// <returns>Vector2 normalizado con la dirección de disparo.</returns>
    private Vector2 GetAimDirection()
    {
        Vector2 rawAim = _aimAction.ReadValue<Vector2>();

        // Detectamos si es posición de pantalla (ratón) o dirección de joystick (mando).
        // El ratón devuelve coordenadas de pantalla (valores grandes como 960, 540).
        // El joystick devuelve valores entre -1 y 1.
        bool isMouse = Mouse.current != null && _aimAction.activeControl?.device is Mouse;

        if (isMouse)
        {
            // Convertimos la posición del cursor de espacio pantalla a espacio mundo
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(rawAim.x, rawAim.y, 0f));
            // Calculamos la dirección desde el jugador hasta el cursor
            return ((Vector2)worldPos - (Vector2)transform.position).normalized;
        }
        else
        {
            // El joystick ya da una dirección directa
            return rawAim.normalized;
        }
    }

    #endregion

} // class PlayerShoot