//---------------------------------------------------------
// Ataque de Dash del jefe: al ejecutarse, frena ligeramente la velocidad
// actual del jefe y le aplica un impulso de física hacia el jugador.
// Si colisiona con algo que tenga Health, le aplica daño.
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Ataque de Dash del jefe (Raven). ExecuteDashAttack es llamado por
/// BossPhaseController como uno de los ataques aleatorios: reduce la
/// velocidad actual del Rigidbody2D y le aplica un impulso hacia
/// targetPlayer. Si el jefe colisiona con un objeto con componente
/// Health durante el dash, le aplica damageAmount puntos de daño.
/// AplicarBuffFaseFinal aumenta la fuerza del dash y su frecuencia
/// al activarse la Fase 3 (Enrage).
/// </summary>
public class BossFirstShoot : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("References")]
    [Tooltip("Transform del jugador, hacia el que se dirige el Dash.")]
    [SerializeField] private Transform targetPlayer;

    [Header("Timer Settings (Seconds)")]
    [Tooltip("Tiempo mínimo de espera considerado al calcular el próximo dash (reservado, ver nota en CalculateNextDash).")]
    [SerializeField] private float minWaitTime = 7f;

    [Tooltip("Tiempo máximo de espera considerado al calcular el próximo dash (reservado, ver nota en CalculateNextDash).")]
    [SerializeField] private float maxWaitTime = 15f;

    [Header("Movement Settings")]
    [Tooltip("Fuerza del impulso aplicado al Rigidbody2D al ejecutar el dash.")]
    [SerializeField] private float dashForce = 20f;

    [Tooltip("Fricción lineal (linearDamping) del Rigidbody2D, para que el jefe se frene tras el dash.")]
    [SerializeField] private float dashDrag = 3f;

    [Tooltip("Puntos de daño que aplica el jefe al colisionar con algo que tenga Health durante el dash.")]
    [SerializeField] private int damageAmount = 30;

    [Header("Dash")]
    [Tooltip("Fracción de la velocidad actual que conserva el jefe justo antes de aplicar el impulso del dash.")]
    [SerializeField] private float dashVelocityRetention = 0.3f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Rigidbody2D del jefe, configurado en Awake para el dash.</summary>
    private Rigidbody2D rb;

    /// <summary>
    /// Instante (Time.time) calculado por CalculateNextDash para un posible
    /// próximo dash. Actualmente no se consulta en ningún Update: el
    /// disparo del dash lo decide BossPhaseController llamando a
    /// ExecuteDashAttack directamente. Se conserva por si en el futuro
    /// se quiere un dash también automático.
    /// </summary>
    private float nextDashTime;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea el Rigidbody2D y lo configura para el dash (sin gravedad,
    /// con fricción, rotación bloqueada e interpolación), y calcula el
    /// primer tiempo de espera reservado.
    /// </summary>
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

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Ejecuta el ataque de Dash: reduce la velocidad actual del jefe a
    /// dashVelocityRetention y le aplica un impulso de fuerza dashForce
    /// en dirección a targetPlayer.
    /// </summary>
    public void ExecuteDashAttack()
    {
        if (targetPlayer == null) return; // Seguridad

        Vector2 dashDirection = (targetPlayer.position - transform.position).normalized;

        rb.linearVelocity *= dashVelocityRetention;
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);

        Debug.Log("¡Raven ejecutando Dash desde el Controlador de Fases!");
    }

    /// <summary>
    /// Aplica el buff de la Fase 3 (Enrage): aumenta la fuerza del dash y
    /// reduce los tiempos de espera mínimo y máximo dividiéndolos por
    /// multiplicador.
    /// </summary>
    /// <param name="multiplicador">Factor de potenciación de la Fase 3.</param>
    public void AplicarBuffFaseFinal(float multiplicador)
    {
        dashForce *= multiplicador;
        minWaitTime /= multiplicador;
        maxWaitTime /= multiplicador;
        Debug.Log("[BossFirstShoot] Buff de velocidad de ataque aplicado.");
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Calcula un tiempo de espera aleatorio entre minWaitTime y maxWaitTime
    /// y lo guarda en nextDashTime. Ver nota en el campo nextDashTime.
    /// </summary>
    private void CalculateNextDash()
    {
        float wait = Random.Range(minWaitTime, maxWaitTime);
        nextDashTime = Time.time + wait;
    }

    /// <summary>
    /// Al colisionar físicamente con algo que tenga componente Health
    /// (por ejemplo el jugador durante el dash), le aplica damageAmount
    /// puntos de daño.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Health healthComponent = collision.gameObject.GetComponent<Health>();

        if (healthComponent != null)
        {
            healthComponent.Damage(damageAmount);
        }
    }

    #endregion

} // class BossFirstShoot
  // Marián Navarro Santoyo