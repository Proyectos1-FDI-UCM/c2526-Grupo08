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

    #endregion
    [SerializeField]
    private float Velocidad = 2f;
    [SerializeField]
    private float VelocidadDeslizar = 2f;
    [SerializeField]
    private Sprite SpriteUp;
    [SerializeField]
    private Sprite SpriteDown;
    [SerializeField]
    private Sprite SpriteLeft;

    private InputAction dashAction;
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 15f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    [SerializeField] private TrailRenderer tr;
    private Vector2 lastMoveDirection = Vector2.right;

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

    private Vector2 _movement;

    private InputAction _moveAction;


    private bool _sliding = false;

    private SpriteRenderer SpriteRenderer;

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
    void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();

        _rb = GetComponent<Rigidbody2D>();

        _moveAction = InputSystem.actions.FindAction("Move");
        if (_moveAction == null)
        {
            Debug.Log("Accion no encontrada, no funciona el PlayerControler");
            Destroy(this);
        }

        dashAction = InputSystem.actions.FindAction("Dash");
        if (dashAction == null)
        {
            Debug.Log("Accion Dash no encontrada");
        }
    }

    private void Update()
    {
        moveAction.Enable();
        dashAction.Enable();
        dashAction.performed += OnDash;
    }
    private void OnDisable()
    {
        dashAction.performed -= OnDash;
        moveAction.Disable();
        dashAction.Disable();
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (canDash && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }
    #endregion

    void FixedUpdate()
    {
        if (isDashing) return;

        _movement = _moveAction.ReadValue<Vector2>().normalized;
        
        if (Movimiento != Vector2.zero)
        {
            lastMoveDirection = Movimiento;
        }

        //Calculamos la velocidad normal
        Vector2 VelocidadFinal = _movement * Velocidad;
        
        //Calculamos el desliz en caso de que lo haya
        if (_sliding) VelocidadFinal.y = -VelocidadDeslizar;
        
        //Aplicamos la velocidad
        _rb.linearVelocity = VelocidadFinal;

        //Transformamos las coordenadas del mouse a la pantalla en la variable Mouse
        Vector3 ScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Vector3 WorldPos = Camera.main.ScreenToWorldPoint(ScreenPos);

        Vector2 Mouse = WorldPos - transform.position;


        //Detectamos la dirección en la que se encuentra el ratón y dependiendo de esta cambiamos el sprite.
        if ((Mathf.Abs(Mouse.x) > Mathf.Abs(Mouse.y)))
        {
            if (Mouse.x > 0)
            {
                Debug.Log("Derecha");

                ChangeSprite(Direction.Right);
            }
            else
            {
                Debug.Log("Izquierda");

                ChangeSprite(Direction.Left);
            }
        }
        else
        {
            if (Mouse.y > 0)
            {
                Debug.Log("Arriba");

                ChangeSprite(Direction.Up);
            }
            else
            {
                Debug.Log("Abajo");

                ChangeSprite(Direction.Down);
            }
        }
    }

    //Empleamos este método para aplicar desliz cada vez que el jugador se choca con una pared
    private void OnCollisionStay2D(Collision2D collision)
    {
        ContactPoint2D contactPoint = collision.GetContact(0); //Detecta el punto donde la colisión ocurre

        if (Mathf.Abs(contactPoint.normal.x) > 0.98f) //Si choca contra un objeto vertical
        {
            if ((contactPoint.normal.x > 0 && _movement.x < 0) || //Pared y desplazamiento a la izquierda
                (contactPoint.normal.x < 0 && _movement.x > 0))   //Pared y desplazamiento a la derecha
            {
                _sliding = true;
                return;
            }
        }
        _sliding = false;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        if (lastMoveDirection == Vector2.zero)
        {
            isDashing = false;
            yield break;
        }
        tr.emitting = true;
        Rigidbody.linearVelocity = lastMoveDirection * dashingPower;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
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
            Debug.Log("Sprite era" + CurrentDirection);

            switch (New)
            {
                case Direction.Up:

                    SpriteRenderer.sprite = SpriteUp;

                    SetScaleX(Mathf.Abs(CurrentScale.x));

                    break;

                case Direction.Down:

                    SpriteRenderer.sprite = SpriteDown;

                    SetScaleX(Mathf.Abs(CurrentScale.x));

                    break;

                case Direction.Left:

                    SpriteRenderer.sprite = SpriteLeft;

                    SetScaleX(Mathf.Abs(CurrentScale.x));

                    break;

                case Direction.Right:

                    SpriteRenderer.sprite = SpriteLeft;

                    SetScaleX(-Mathf.Abs(CurrentScale.x));

                    break;
            }

            CurrentDirection = New;

            Debug.Log("Sprite ha cambiadp a" + CurrentDirection);

        }

    }

    #endregion

} // class Movement 
// Adriana Fernández Luna
//Celia García Riaza