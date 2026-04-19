//---------------------------------------------------------
// Componente que desplaza al jugador de forma constante (rb.velocity)
// Adriana Fernández Luna
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
// Añadir aquí el resto de directivas using

/// <summary>
/// Controla el movimiento bidimensional del jugador en 8 direcciones
/// utilizando entrada de teclado (WASD) y mando (joystick izquierdo).
/// El desplazamiento se realiza a velocidad constante en todas las
/// direcciones, independientemente de la intensidad del joystick.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    [SerializeField]
    private float Velocidad = 2f;
    [SerializeField]
    private float VelocidadDeslizar = 2f;
    [SerializeField]
    private float VelociadDeslizarDash = 6f;
    [SerializeField]
    private Sprite SpriteUp;
    [SerializeField]
    private Sprite SpriteDown;
    [SerializeField]
    private Sprite SpriteLeft;
    [SerializeField] 
    private TrailRenderer tr;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    #endregion
    private Rigidbody2D _rb;

    private Vector2 Movement;

    //Variables dash
    private bool _canDash = true;
    private bool _isDashing;
    private float _dashingPower = 15f;
    private float _dashingTime = 0.5f;
    private float _dashingCooldown = 1.5f;
    private float _dashEndTime;
    private float _dashCooldownEnd;
    private Vector2 _lastMoveDirection = Vector2.right;

    private InputAction MoveAction;
    private InputAction DashAction;

    private SpriteRenderer _spriteRenderer;

    private Health _health;

    private ChargedAttack _chargedAttack;
    private enum Direction { Up, Down, Right, Left }
    private Direction CurrentDirection = Direction.Left;
    private Direction NewDirection;

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    // Por defecto están los típicos (Update y Start) pero:
    // - Hay que añadir todos los que sean necesarios
    // - Hay que borrar los que no se usen 

    /// <summary>
    /// Start is called on the frame when a script is enabled just before 
    /// any of the Update methods are called the first time.
    /// </summary>
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _rb = GetComponent<Rigidbody2D>();

        _health = GetComponent<Health>();

        _chargedAttack = GetComponent<ChargedAttack>();

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

    private void OnEnable()
    {
        MoveAction.Enable();

        if (DashAction != null)
        {
            DashAction.Enable();
            DashAction.performed += OnDash;
        }
    }
    private void OnDisable()
    {
        MoveAction.Disable();

        if (DashAction != null)
        {
            DashAction.Disable();
            DashAction.performed -= OnDash;
        }
    }

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

    private void OnDash(InputAction.CallbackContext context)
    {
        if (_chargedAttack !=null && _chargedAttack.IsCharging())
        {
            return;
        }
        
        if (_canDash && !_isDashing)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        if (_lastMoveDirection == Vector2.zero)
        {
            return;
        }
        _canDash = false;
        _isDashing = true;
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
    }

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
    }
    #endregion

    void FixedUpdate()
    {
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
            VelocidadFinal = _lastMoveDirection * _dashingPower;
        }
        else
        {
            VelocidadFinal = Movement * Velocidad;
        }

        //Aplicamos la velocidad
        _rb.linearVelocity = VelocidadFinal;

        Vector2 dir;


        //Tomamos el vector del joystick
        Vector2 gamepad = Vector2.zero;
        if (Gamepad.current != null)
        {
            gamepad = Gamepad.current.rightStick.ReadValue();
        }

        if (gamepad.magnitude > 0.1f)
        {
            dir = gamepad;
        }
        else 
        {
            //Transformamos las coordenadas del mouse a la pantalla en la variable Mouse
            Vector3 ScreenPos = Mouse.current.position.ReadValue();
            Vector3 WorldPos = Camera.main.ScreenToWorldPoint(ScreenPos);
            dir =  WorldPos - transform.position;
        }

        //Detectamos la dirección en la que se encuentra el ratón y dependiendo de esta cambiamos el sprite.
        if ((Mathf.Abs(dir.x) > Mathf.Abs(dir.y)))
        {
            if (dir.x > 0 || dir.x > 0)
            {
                ChangeSprite(Direction.Right);
            }
            else
            {
                ChangeSprite(Direction.Left);
            }
        }
        else
        {
            if (dir.y > 0 || dir.y > 0)
            {
                ChangeSprite(Direction.Up);
            }
            else
            {
                ChangeSprite(Direction.Down);
            }
        }
    }


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
    // Método empleado para invertir el sprite del jugador de derecha a izquierda
    // tan solo cambiamos la escala del sprite a negativo para que funcione
    // dependiendo de en donde esta el cursor/left joystick
    //Se trató de implementar en cuatro direcciones pero fue mejor de otra manera
    //Se conserva en caso de algún cambio en los sprites del futuro.

    private void Flip()
    {

        Vector3 CurrentScale = gameObject.transform.localScale;

        Debug.Log("No ha hecho flip");
        CurrentScale.x = -CurrentScale.x;
        Debug.Log("ha hecho flip");

        gameObject.transform.localScale = CurrentScale;

    }

    private void SetScaleX(float x)
    {
        Vector3 Scale = gameObject.transform.localScale;
        Scale.x = x;
        gameObject.transform.localScale = Scale;
    }

    //Método que cambia de sprite o rota este dependiendo de la constante enum
    //Empleamos este método para poder usar la variable enum que
    //a su vez permite ahorrar cuando no hay nueva direción a la que moverse

    private void ChangeSprite(Direction New)
    {
        if (New != CurrentDirection)
        {
            Vector3 CurrentScale = gameObject.transform.localScale;
            //Debug.Log("Sprite era" + CurrentDirection);

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

                    SetScaleX(Mathf.Abs(CurrentScale.x));

                    break;

                case Direction.Right:

                    _spriteRenderer.sprite = SpriteLeft;

                    SetScaleX(-Mathf.Abs(CurrentScale.x));

                    break;
            }

            CurrentDirection = New;

            //Debug.Log("Sprite ha cambiado a " + CurrentDirection);

        }

    }

    #endregion

} // class Movement 
// Adriana Fernández Luna
//Celia García Riaza
//Carlos Mesa Torres
