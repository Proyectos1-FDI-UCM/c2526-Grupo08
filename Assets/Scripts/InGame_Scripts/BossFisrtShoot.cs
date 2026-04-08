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
    [SerializeField] private float dashForce = 20f;   // La fuerza del impulso inicial
    [SerializeField] private float dashDrag = 3f;    // Fricción para que se detenga tras el dash
    [SerializeField] private int damageAmount = 30;

    private Rigidbody2D rb;
    private float nextDashTime;



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

        // Configuración física para el dash
        rb.gravityScale = 0f;
        rb.linearDamping = dashDrag;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Evitamos que se tumbe (bloqueo de rotación Z)
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CalculateNextDash();

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
        if (targetPlayer == null) return;

        // Comprobamos si ha llegado el momento del dash
        if (Time.time >= nextDashTime)
        {
            Dash();
            CalculateNextDash(); // Se programa el siguiente 
        }
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

    

    private void Dash()
    {
        Vector2 dashDirection = (targetPlayer.position - transform.position).normalized;

        // Reducimos la velocidad previa a la mitad antes del impulso
        // en vez de pararla en seco, para que la transición sea más fluida.
        rb.linearVelocity *= 0.3f;
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);

        Debug.Log("¡Enemigo ejecutando Dash!");
    }

   

    private void CalculateNextDash()
    {
        float wait = Random.Range(minWaitTime, maxWaitTime);
        nextDashTime = Time.time + wait;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        Health healthComponent = collision.gameObject.GetComponent<Health>();

        if (healthComponent != null)
        {
            healthComponent.Damage(damageAmount);
            Debug.Log($"{gameObject.name} daño a {collision.gameObject.name} quitando {damageAmount} de vida");
        }
    }



}
// class BossFisrtShoot 
// namespace
