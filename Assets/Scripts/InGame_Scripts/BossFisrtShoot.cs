//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Marian Navarro Santoyo
// No way down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class BossFisrtShoot : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    [Header("References")]
    [SerializeField] private Transform targetPlayer;

    [Header("Timer Settings (Seconds)")]
    [SerializeField] private float minWaitTime = 7f;
    [SerializeField] private float maxWaitTime = 15f;
    [SerializeField] private float dashDuration = 3f;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 18f;
    [SerializeField] private float acceleration = 60f;

    private Rigidbody2D rb;
    private float nextAttackTime;
    private float stopAttackTime;


    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        
        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;


        SetNextChaseTime();

    }

    private void FixedUpdate()
    {
        if (targetPlayer == null) return;

        if (Time.fixedTime >= nextAttackTime && Time.fixedTime < stopAttackTime)
        {
            ExecuteDash();
        }
        else
        {
            StopMovement();

            if (Time.fixedTime >= stopAttackTime)
            {
                CalculateNextAttack();
            }
        }
    }


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

    private void SetNextChaseTime()
    {
        if (targetPlayer == null) return;

        // Check if we are within the attack window
        if (Time.fixedTime >= nextAttackTime && Time.fixedTime < stopAttackTime)
        {
            ExecuteDash();
        }
        else
        {
            StopMovement();

            if (Time.fixedTime >= stopAttackTime)
            {
                CalculateNextAttack();
            }
        }
    }

    private void ExecuteDash()
    {
        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        Vector2 targetVelocity = direction * maxSpeed;

        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
    }

    private void StopMovement()
    {
        
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, acceleration * Time.fixedDeltaTime);
    }

    private void CalculateNextAttack()
    {
        float randomWait = Random.Range(minWaitTime, maxWaitTime);
        nextAttackTime = Time.fixedTime + randomWait;
        stopAttackTime = nextAttackTime + dashDuration;
    }


} // class BossFisrtShoot 
// namespace
