//---------------------------------------------------------
// Componente que desplaza al jugador de forma constante (rb.velocity)
// Adriana Fernández Luna
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el movimiento bidimensional del jugador en 8 direcciones
/// utilizando entrada de teclado (WASD) y mando (joystick izquierdo).
/// El desplazamiento se realiza a velocidad constante en todas las
/// direcciones, independientemente de la intensidad del joystick.
/// También gestiona el dash, la animación de apuntado (mando/ratón)
/// y la animación de recoger objetos.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Tooltip("Velocidad de desplazamiento del jugador (en unidades/segundo).")]
    [SerializeField]
    private float Velocidad = 8f;

    [Tooltip("Sprite del jugador mirando hacia arriba.")]
    [SerializeField]
    private Sprite SpriteUp;

    [Tooltip("Sprite del jugador mirando hacia abajo.")]
    [SerializeField]
    private Sprite SpriteDown;

    [Tooltip("Sprite del jugador mirando hacia la izquierda (se invierte en X para la derecha).")]
    [SerializeField]
    private Sprite SpriteLeft;

    [Tooltip("TrailRenderer que se activa mientras el jugador está haciendo dash.")]
    [SerializeField]
    private TrailRenderer tr;

    [Tooltip("Duración en segundos del dash.")]
    [SerializeField]
    private float _dashingTime = 0.2f;

    [Header("Apuntado")]
    [Tooltip("Magnitud mínima del stick derecho del mando para usarlo como dirección de apuntado " +
             "(por debajo de este valor se usa la posición del ratón).")]
    [SerializeField]
    private float GamepadAimDeadzone = 0.1f;

    [Header("Recoger objetos")]
    [Tooltip("Duración en segundos de la animación de recoger un objeto antes de volver al estado normal.")]
    [SerializeField]
    private float PickupDuration = 0.6f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Rigidbody2D del jugador, usado para aplicar la velocidad de movimiento y dash.</summary>
    private Rigidbody2D _rb;

    /// <summary>Vector de movimiento normalizado leído del Input System en el frame actual.</summary>
    private Vector2 Movement;

    // Variables dash

    /// <summary>True si el dash está disponible (no está en cooldown).</summary>
    private bool _canDash = true;

    /// <summary>True mientras el jugador está ejecutando el dash.</summary>
    private bool _isDashing;

    /// <summary>Velocidad aplicada al jugador durante el dash.</summary>
    private float _dashingPower = 30f;

    /// <summary>Tiempo de espera en segundos antes de poder volver a hacer dash.</summary>
    private float _dashingCooldown = 1.5f;

    /// <summary>Permite controlar si el jugador está en animación de recoger objeto.</summary>
    private bool _isPickingUp;

    /// <summary>Instante (Time.time) en el que termina el dash actual.</summary>
    private float _dashEndTime;

    /// <summary>Instante (Time.time) en el que vuelve a estar disponible el dash.</summary>
    private float _dashCooldownEnd;

    /// <summary>Dirección normalizada en la que se realiza el dash actual.</summary>
    private Vector2 _dashDir;

    /// <summary>Última dirección de movimiento distinta de cero, usada como dirección del dash si el jugador está quieto.</summary>
    private Vector2 _lastMoveDirection = Vector2.right;

    /// <summary>Animator del jugador, usado para los parámetros de movimiento, dash y pickup.</summary>
    private Animator _animator;

    /// <summary>Acción de Input System para el movimiento (WASD / stick izquierdo).</summary>
    private InputAction MoveAction;

    /// <summary>Acción de Input System para el dash.</summary>
    private InputAction DashAction;

    /// <summary>SpriteRenderer del jugador, usado por ChangeSprite (actualmente sin uso activo).</summary>
    private SpriteRenderer _spriteRenderer;

    /// <summary>Componente Health del jugador, usado para activar/desactivar la inmunidad durante el dash.</summary>
    private Health _health;

    /// <summary>Componente ChargedAttack del jugador; si está cargando un ataque, se bloquea el movimiento y el dash.</summary>
    private ChargedAttack _chargedAttack;

    /// <summary>Direcciones posibles para ChangeSprite (actualmente sin uso activo).</summary>
    private enum Direction { Up, Down, Right, Left }

    /// <summary>Dirección actual del sprite, usada por ChangeSprite (actualmente sin uso activo).</summary>
    private Direction CurrentDirection = Direction.Left;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea los componentes necesarios y obtiene las acciones de
    /// movimiento y dash del Input System.
    /// </summary>
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _rb = GetComponent<Rigidbody2D>();

        _health = GetComponent<Health>();

        _chargedAttack = GetComponent<ChargedAttack>();

        _animator = GetComponent<Animator>();

        MoveAction = InputSystem.actions.FindAction("Move");
        if (MoveAction == null)
        {
            Debug.Log("Accion no encontrada, no funciona el PlayerControler");
            Destroy(this);
        }

        DashAction = InputSystem.actions.FindAction("Dash");
        if (DashAction == null)
        {
            Debug.Log("Accion Dash no encontrada");
        }
    }

    /// <summary>
    /// Activa las acciones de Input System y se suscribe al evento de dash.
    /// </summary>
    private void OnEnable()
    {
        MoveAction.Enable();

        if (DashAction != null)
        {
            DashAction.Enable();
            DashAction.performed += OnDash;
        }
    }

    /// <summary>
    /// Desactiva las acciones de Input System y elimina la suscripción al dash.
    /// </summary>
    private void OnDisable()
    {
        MoveAction.Disable();

        if (DashAction != null)
        {
            DashAction.Disable();
            DashAction.performed -= OnDash;
        }
    }

    /// <summary>
    /// Comprueba cada frame si el dash o su cooldown han terminado.
    /// </summary>
    private void Update()
    {
        if (_isDashing && Time.time >= _dashEndTime)
        {
            EndDash();
        }

        if (!_canDash && Time.time >= _dashCooldownEnd)
        {
            _canDash = true;
        }
    }

    /// <summary>
    /// Aplica el movimiento físico del jugador (normal o dash), calcula
    /// la dirección de apuntado (mando o ratón) para las animaciones,
    /// y voltea el sprite según la dirección horizontal de apuntado.
    /// </summary>
    void FixedUpdate()
    {
        // ChargeAttack
        if (_chargedAttack != null && _chargedAttack.IsCharging())
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Movement = MoveAction.ReadValue<Vector2>().normalized;

        if (Movement != Vector2.zero)
        {
            _lastMoveDirection = Movement;
        }

        Vector2 VelocidadFinal;

        if (_isDashing)
        {
            VelocidadFinal = _dashDir.normalized * _dashingPower;
        }
        else
        {
            VelocidadFinal = Movement * Velocidad;
        }

        // Aplicamos la velocidad
        _rb.linearVelocity = VelocidadFinal;

        Vector2 dir;

        // Tomamos el vector del joystick
        Vector2 gamepad = Vector2.zero;
        if (Gamepad.current != null)
        {
            gamepad = Gamepad.current.rightStick.ReadValue(); // Leído directamente del gamepad para evitar errores
        }

        if (gamepad.magnitude > GamepadAimDeadzone)
        {
            dir = gamepad;
        }
        else
        {
            // Transformamos las coordenadas del mouse a la pantalla en la variable Mouse
            Vector3 ScreenPos = Mouse.current.position.ReadValue();
            Vector3 WorldPos = Camera.main.ScreenToWorldPoint(ScreenPos);
            dir = WorldPos - transform.position;
        }

        // Animaciones
        dir = dir.normalized;

        _animator.SetFloat("MoveX", dir.x);
        _animator.SetFloat("MoveY", dir.y);
        _animator.SetFloat("Speed", Movement.magnitude);

        Vector3 scale = transform.localScale;

        if (dir.x < 0)
        {
            scale.x = -Mathf.Abs(scale.x);
        }
        else if (dir.x > 0)
        {
            scale.x = Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Inicia la animación de "recoger objeto": detiene al jugador,
    /// activa el parámetro IsPickingUp del Animator y lo desactiva
    /// automáticamente tras PickupDuration segundos.
    /// </summary>
    public void PlayPickup()
    {
        Debug.Log("Pickup activado");

        _animator.SetBool("IsPickingUp", true);

        _rb.linearVelocity = Vector2.zero;

        Invoke(nameof(PickupEnd), PickupDuration);
    }

    /// <summary>
    /// Finaliza la animación de "recoger objeto" desactivando el
    /// parámetro IsPickingUp del Animator. Llamado automáticamente
    /// desde PlayPickup mediante Invoke.
    /// </summary>
    public void PickupEnd()
    {
        Debug.Log("END PICKUP EJECUTADO");

        _animator.SetBool("IsPickingUp", false);
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Gestiona la entrada de la acción de dash: si no se está cargando
    /// un ataque cargado y el dash está disponible, lo inicia.
    /// </summary>
    private void OnDash(InputAction.CallbackContext context)
    {
        if (_chargedAttack != null && _chargedAttack.IsCharging())
        {
            return;
        }

        if (_canDash && !_isDashing)
        {
            StartDash();
        }
    }

    /// <summary>
    /// Activa el estado de dash: fija la dirección, el temporizador de
    /// fin de dash y de cooldown, activa el TrailRenderer, hace al
    /// jugador inmune al daño y activa la animación correspondiente.
    /// </summary>
    private void StartDash()
    {
        _isDashing = true;
        _dashDir = _lastMoveDirection.normalized;
        if (_lastMoveDirection == Vector2.zero)
        {
            return;
        }

        _canDash = false;
        _dashEndTime = Time.time + _dashingTime;
        _dashCooldownEnd = Time.time + _dashingCooldown;
        if (tr != null)
        {
            tr.emitting = true;
        }

        if (_health != null)
        {
            _health.SetImmune(true);
        }

        _animator.SetBool("IsDashing", true);
    }

    /// <summary>
    /// Finaliza el estado de dash: desactiva el TrailRenderer, quita la
    /// inmunidad al daño y desactiva la animación de dash.
    /// </summary>
    private void EndDash()
    {
        _isDashing = false;
        if (tr != null)
        {
            tr.emitting = false;
        }

        if (_health != null)
        {
            _health.SetImmune(false);
        }

        _animator.SetBool("IsDashing", false);
    }

    /// <summary>
    /// Invierte el sprite del jugador en el eje X.
    /// Reservado para posibles cambios futuros en los sprites; no se usa
    /// actualmente porque el volteo se gestiona en FixedUpdate.
    /// </summary>
    private void Flip()
    {
        Vector3 CurrentScale = gameObject.transform.localScale;
        CurrentScale.x = -CurrentScale.x;
        gameObject.transform.localScale = CurrentScale;
    }

    /// <summary>
    /// Establece directamente la escala X del jugador (usado por ChangeSprite
    /// para voltear el sprite sin afectar a la escala Y).
    /// </summary>
    private void SetScaleX(float x)
    {
        Vector3 Scale = gameObject.transform.localScale;
        Scale.x = x;
        gameObject.transform.localScale = Scale;
    }

    /// <summary>
    /// Cambia el sprite del jugador según la dirección indicada y ajusta
    /// la escala X para reflejarlo correctamente. Reservado para un
    /// posible sistema de sprites direccional; no se usa actualmente.
    /// </summary>
    private void ChangeSprite(Direction New)
    {
        if (New != CurrentDirection)
        {
            Vector3 CurrentScale = gameObject.transform.localScale;

            switch (New)
            {
                case Direction.Up:
                    _spriteRenderer.sprite = SpriteUp;
                    SetScaleX(Mathf.Abs(CurrentScale.x));
                    break;

                case Direction.Down:
                    _spriteRenderer.sprite = SpriteDown;
                    SetScaleX(Mathf.Abs(CurrentScale.x));
                    break;

                case Direction.Left:
                    _spriteRenderer.sprite = SpriteLeft;
                    SetScaleX(-Mathf.Abs(CurrentScale.x));
                    break;

                case Direction.Right:
                    _spriteRenderer.sprite = SpriteLeft;
                    SetScaleX(Mathf.Abs(CurrentScale.x));
                    break;
            }

            CurrentDirection = New;
        }
    }

    #endregion

} // class PlayerMovement
  // Adriana Fernández Luna
  // Celia García Riaza
  // Carlos Mesa Torres