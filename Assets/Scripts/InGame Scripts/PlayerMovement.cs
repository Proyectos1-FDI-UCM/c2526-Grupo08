//---------------------------------------------------------
// Componente que desplaza al jugador de forma constante (rb.velocity)
// Adriana Fernández Luna
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

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



    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    #endregion
    private Rigidbody2D Rigidbody;

    private Vector2 Movimiento;

    private InputAction moveAction;

    private bool deslizando = false;

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
        Rigidbody = GetComponent<Rigidbody2D>();

        moveAction = InputSystem.actions.FindAction("Move");
        if (moveAction == null)
        {
            Debug.Log("Accion no encontrada, no funciona el PlayerControler");
            Destroy(this);
        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {

    }

    #endregion

    void FixedUpdate()
    {
        Movimiento = moveAction.ReadValue<Vector2>().normalized;
        Vector2 VelocidadFinal = Movimiento * Velocidad;
        //Rigidbody.linearVelocity = Movimiento * Velocidad;
        if (deslizando) VelocidadFinal.y = -VelocidadDeslizar;
        Rigidbody.linearVelocity = VelocidadFinal;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ContactPoint2D contactPoint = collision.GetContact(0); //Detecta el punto donde la colisión ocurre

        if (Mathf.Abs(contactPoint.normal.x) > 0.98f) //Si choca contra un objeto vertical
        {
            if ((contactPoint.normal.x > 0 && Movimiento.x < 0) || //Pared y desplazamiento a la izquierda
                (contactPoint.normal.x < 0 && Movimiento.x > 0))   //Pared y desplazamiento a la derecha
            {
                deslizando = true;
                return;
            }
        }
        deslizando = false;
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
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    #endregion

} // class Movement 
// Adriana Fernández Luna
