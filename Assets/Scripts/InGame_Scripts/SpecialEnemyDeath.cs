//---------------------------------------------------------
// Gestiona el estado "derrotado" del enemigo especial de la planta 2.
// Al llamar a OnDefeated(): para movimiento y ataques, rota el sprite
// para simular que ha caído al suelo y avisa a SpecialEnemyInteraction.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Se activa cuando Health llama a OnDefeated() en lugar de destruir el enemigo.
/// Desactiva EnemyPatrol, EnemyShoot y EnemyAttack, congela el Rigidbody2D
/// y rota el sprite 90 grados para indicar que está en el suelo.
/// Habilita SpecialEnemyInteraction para que el jugador pueda acercarse.
/// </summary>
public class SpecialEnemyDeath : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Referencias")]
    [Tooltip("Barra de vida del enemigo (el objeto entero, no solo el fill). Se oculta al caer.")]
    [SerializeField] private GameObject HealthBarObject;

    [Header("Rotación al caer")]
    [Tooltip("Grados de rotación Z al caer al suelo (90 o -90 según la orientación del sprite)")]
    [SerializeField] private float FallRotationZ = 90f;

    [Tooltip("Velocidad de la rotación de caída (grados por segundo)")]
    [SerializeField] private float FallRotationSpeed = 200f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Indica si el enemigo ya ha sido derrotado para evitar doble llamada.</summary>
    private bool _isDefeated = false;

    /// <summary>Indica si la animación de caída ha terminado.</summary>
    private bool _fallComplete = false;

    /// <summary>Rotación objetivo en Z cuando termina la caída.</summary>
    private Quaternion _targetRotation;

    private Rigidbody2D _rb;
    private EnemyPatrol _patrol;
    private EnemyShoot _shoot;
    private EnemyMeleeAttack _attack;
    private SpecialEnemyInteraction _interaction;


    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _patrol = GetComponent<EnemyPatrol>();
        _shoot = GetComponent<EnemyShoot>();
        _attack = GetComponent<EnemyMeleeAttack>();
        _interaction = GetComponent<SpecialEnemyInteraction>();
    }

    private void Update()
    {
        if (!_isDefeated || _fallComplete) return;

        // Rotar suavemente hacia la rotación de caída
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            _targetRotation,
            FallRotationSpeed * Time.deltaTime
        );

        // Comprobar si ha terminado la rotación
        if (Quaternion.Angle(transform.rotation, _targetRotation) < 0.5f)
        {
            transform.rotation = _targetRotation;
            _fallComplete = true;

            // Habilitar interacción solo cuando ya está tumbado
            if (_interaction != null)
                _interaction.EnableInteraction();
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Llamado por Health cuando la vida llega a 0 y IsSpecialEnemy es true.
    /// Para todos los comportamientos activos e inicia la animación de caída.
    /// </summary>
    public void OnDefeated()
    {
        if (_isDefeated) return;
        _isDefeated = true;

        // Parar movimiento físico
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Kinematic; // evitar que resbale
        }

        // Desactivar comportamientos de combate
        if (_patrol != null) _patrol.enabled = false;
        if (_shoot != null) _shoot.enabled = false;
        if (_attack != null) _attack.enabled = false;

        // Desactivar colisión de daño (el jugador puede acercarse sin recibir daño)
        // Mantener el collider del trigger para la detección de proximidad
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            // Solo desactivar los que no son trigger (los de daño físico)
            if (!col.isTrigger)
                col.enabled = false;
        }

        // Ocultar la barra de vida (ya llegó a 0, no tiene sentido mostrarla)
        if (HealthBarObject != null)
            HealthBarObject.SetActive(false);

        // Preparar rotación objetivo
        _targetRotation = Quaternion.Euler(0f, 0f, FallRotationZ);
    }

    /// <summary>Devuelve true si el enemigo ya está en estado derrotado.</summary>
    public bool IsDefeated() => _isDefeated;

    #endregion

} // class SpecialEnemyDeath