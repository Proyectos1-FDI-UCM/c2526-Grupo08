//---------------------------------------------------------
// Gestiona el ataque cargado del jugador: al mantener pulsada la acción
// "Charged", acumula carga durante _chargedTime segundos mostrando un
// efecto visual de color y una aureola de partículas, y al completarse
// dispara una bala de mayor daño consumiendo magia.
// Carlos Mesa Torres
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el ataque cargado del jugador: temporiza la carga mientras
/// se mantiene pulsada la acción "Charged", muestra el progreso mediante
/// un cambio de color del sprite y una aureola de partículas, y al
/// completar la carga (o soltar antes) dispara una bala de mayor daño
/// si hay magia suficiente.
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

    [Header("Charged Attack")]
    [Tooltip("Tiempo en segundos que hay que mantener pulsada la acción para completar la carga.")]
    [SerializeField] private float _chargedTime = 1.5f;

    [Tooltip("Daño que aplica la bala del ataque cargado al impactar.")]
    [SerializeField] private int _chargeDamage = 70;

    [Tooltip("Magia que consume disparar el ataque cargado.")]
    [SerializeField] private int _chargedMagicCost = 20;

    [Header("Charge Visual")]
    [Tooltip("SpriteRenderer del jugador, usado para el efecto visual de carga.")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Tooltip("Color hacia el que se interpola el sprite mientras se carga el ataque.")]
    [SerializeField] private Color _chargeColor = Color.cyan;

    [Header("Charge Aura")]
    [Tooltip("Sistema de partículas que rodea al jugador mientras carga el ataque.")]
    [SerializeField] private ParticleSystem _chargeAura;

    [Tooltip("Tasa de emisión de partículas (por segundo) cuando la carga está al 100%.")]
    [SerializeField] private float _maxEmission = 40f;

    [Tooltip("Tasa de emisión de partículas (por segundo) al iniciar la carga (0%).")]
    [SerializeField] private float _minEmission = 5f;

    [Tooltip("Velocidad de rotación en grados/segundo de la aureola de partículas.")]
    [SerializeField] private float _rotationSpeed = 180f;

    [Header("Pulso de color")]
    [Tooltip("Velocidad del pulso senoidal del color durante la carga.")]
    [SerializeField] private float _colorPulseSpeed = 25f;

    [Tooltip("Amplitud del pulso senoidal del color durante la carga.")]
    [SerializeField] private float _colorPulseAmount = 0.1f;

    [Header("Umbrales de apuntado")]
    [Tooltip("Magnitud mínima de la dirección de disparo para considerarla válida.")]
    [SerializeField] private float _minAimDirectionSqr = 0.01f;

    [Tooltip("Magnitud mínima del stick derecho del mando para usarlo como dirección de apuntado " +
             "(por debajo de este valor se usa la posición del ratón).")]
    [SerializeField] private float _gamepadAimDeadzone = 0.1f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Acción de Input System para el ataque cargado.</summary>
    private InputAction _attackAction;

    /// <summary>Acción de Input System para apuntar con el stick derecho del mando.</summary>
    private InputAction _aimGamepad;

    /// <summary>Acción de Input System para apuntar con la posición del ratón.</summary>
    private InputAction _aimMouse;

    /// <summary>Cámara principal, usada para convertir la posición del ratón a coordenadas de mundo.</summary>
    private Camera _mainCamera;

    /// <summary>Componente Magic del jugador, usado para comprobar y restar el coste de magia.</summary>
    private Magic _magic;

    /// <summary>Tiempo acumulado de carga del ataque actual.</summary>
    private float _chargeTimer = 0f;

    /// <summary>True mientras se está acumulando carga.</summary>
    private bool _isCharging = false;

    /// <summary>True si había magia suficiente al iniciar la carga.</summary>
    private bool _canCharge = false;

    /// <summary>Color original del sprite, al que se vuelve al terminar o cancelar la carga.</summary>
    private Color _originalColor;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Obtiene las acciones de Input System, cachea el componente Magic,
    /// guarda el color original del sprite y detiene la aureola de partículas.
    /// </summary>
    private void Start()
    {
        _attackAction = InputSystem.actions.FindAction("Charged");
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

    /// <summary>
    /// Gestiona el ciclo completo de carga: inicia la carga al pulsar si hay
    /// magia suficiente, actualiza el efecto visual y la aureola mientras se
    /// mantiene pulsado, dispara al completarse el tiempo de carga, y cancela
    /// la carga (sin disparar) si se suelta antes de completarla.
    /// </summary>
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
                float pulse = Mathf.Sin(Time.time * _colorPulseSpeed) * _colorPulseAmount;

                _spriteRenderer.color = Color.Lerp(_originalColor, _chargeColor, Mathf.Clamp01(chargePercent + pulse));
            }

            if (_chargeAura != null)
            {
                if (!_chargeAura.isPlaying)
                {
                    _chargeAura.Play();
                }
                var emission = _chargeAura.emission;
                emission.rateOverTime = Mathf.Lerp(_minEmission, _maxEmission, chargePercent);
                _chargeAura.transform.position = transform.position;
                _chargeAura.transform.Rotate(0, 0, _rotationSpeed * Time.deltaTime);
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

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>Indica si el jugador está actualmente cargando el ataque cargado.</summary>
    /// <returns>True si hay una carga en curso.</returns>
    public bool IsCharging()
    {
        return _isCharging;
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Restaura el color original del sprite y detiene la aureola de partículas.
    /// Se llama al completar, cancelar o soltar la carga.
    /// </summary>
    private void ResetChargeVisual()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }

        if (_chargeAura != null)
        {
            _chargeAura.Stop();
        }
    }

    /// <summary>
    /// Si hay magia suficiente y la dirección de apuntado es válida, gasta la
    /// magia, instancia la bala del ataque cargado y le aplica dirección y daño.
    /// </summary>
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

        if (shootDirection.sqrMagnitude < _minAimDirectionSqr)
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

    /// <summary>
    /// Devuelve la dirección normalizada de disparo.
    /// Ratón: apunta hacia el cursor en coordenadas de mundo.
    /// Mando: usa el joystick derecho directamente si supera la zona muerta.
    /// </summary>
    private Vector2 GetAimDirection()
    {
        Vector2 rawAim = _aimGamepad.ReadValue<Vector2>();

        if (rawAim.magnitude > _gamepadAimDeadzone)
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

} // class ChargedAttack
  // Carlos Mesa Torres