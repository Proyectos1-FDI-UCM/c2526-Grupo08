//---------------------------------------------------------
// Script que tiene el brazo de el personaje para permitir rotación
// Adriana Fernández Luna
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class ArmPivot : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    [SerializeField]
    private SpriteRenderer _armRenderer;
    [SerializeField]
    private SpriteRenderer _playerRenderer;
    [SerializeField]
    private Transform _pivotUp;
    [SerializeField]
    private Transform _pivotDown;
    [SerializeField]
    private Transform _pivotRight;
    [SerializeField]
    private Transform _pivotRightWalk;
    [SerializeField]
    private float _armShowDelay = 0.2f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    private Animator _playerAnimator;

    #endregion

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

    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        //Cambio de color con el personaje 

        _armRenderer.color = _playerRenderer.color;

        //Cambio de posición dependiendo de la dirección del personaje

        float moveX = _playerAnimator.GetFloat("MoveX");
        float moveY = _playerAnimator.GetFloat("MoveY");
        float speed = _playerAnimator.GetFloat("Speed");
        bool isDashing = _playerAnimator.GetBool("IsDashing");
        bool isPickingUp = _playerAnimator.GetBool("IsPickingUp");

        if (isDashing || isPickingUp)
        {
            _armRenderer.enabled = false;
        }
        else
        {
            _armRenderer.enabled = true;

            if (moveY > 0.9f)
                transform.position = _pivotUp.position;
            else if (moveY < -0.9f)
                transform.position = _pivotDown.position;
            else if (speed > 0.1f)
                transform.position = _pivotRightWalk.position;
            else
                transform.position = _pivotRight.position;

            // Dirección hacia el cursor
            Vector3 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            Vector2 dir = worldPos - transform.position;

            // Rotación del pivote

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;

            transform.rotation = Quaternion.Euler(0, 0, angle);


            // Sorting order: por debajo cuando apunta arriba, por encima el resto

            if (moveY > 0.5f)
                _armRenderer.sortingOrder = _playerRenderer.sortingOrder - 1;
            else
                _armRenderer.sortingOrder = _playerRenderer.sortingOrder + 1;
        }
    }

    private void Awake()
    {
        _playerAnimator = transform.parent.GetComponent<Animator>();
    }


    #endregion

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
}
 // class ArmPivot 
// Adriana Fernández Luna
