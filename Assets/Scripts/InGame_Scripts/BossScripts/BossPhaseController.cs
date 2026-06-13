//---------------------------------------------------------
// Controla las fases de combate del jefe (Raven): detecta al jugador,
// gestiona el ciclo de ataques aleatorios y activa las fases 2 (cristales
// + minions, ataques más rápidos) y 3 (Enrage: más velocidad y daño)
// según la vida restante del jefe.
// Marián Navarro y Laura Garay
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Controlador central de la pelea contra el jefe (Raven).
/// Mientras el jugador no ha sido detectado, comprueba periódicamente si
/// está dentro de _detectionRange. Al detectarlo, espera _initialDelay
/// segundos (escuchando ya los cambios de fase por vida) antes de activar
/// el movimiento y empezar el ciclo de ataques aleatorios
/// (Dash, Cuchillas y, a partir de la Fase 2, Minions).
/// Cuando la vida baja de Phase2HealthThreshold activa la Fase 2
/// (cristales + minions, ataques más rápidos), y cuando baja de
/// Phase3HealthThreshold activa la Fase 3 (Enrage: más velocidad y daño).
/// </summary>
public class BossPhaseController : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Detection Settings")]
    [Tooltip("Radio de detección dentro del cual se activa el combate al entrar Cori.")]
    [SerializeField] private float _detectionRange = 12f;

    [Tooltip("Capa física en la que se encuentra el jugador, usada para OverlapCircle.")]
    [SerializeField] private LayerMask _playerLayer;

    [Header("Combat Timing")]
    [Tooltip("Tiempo de espera entre ataques para evitar que se solapen.")]
    [SerializeField] private float _timeBetweenAttacks = 2.0f;

    [Tooltip("Tiempo de espera inicial (en segundos) tras detectar al jugador antes de empezar a atacar.")]
    [SerializeField] private float _initialDelay = 5.0f;

    #endregion

    // ---- CONSTANTES ----
    #region Constantes

    /// <summary>Vida por debajo de la cual se activa la Fase 2.</summary>
    private const int Phase2HealthThreshold = 1000;

    /// <summary>Vida por debajo de la cual se activa la Fase 3 (Enrage).</summary>
    private const int Phase3HealthThreshold = 500;

    /// <summary>Multiplicador aplicado al tiempo entre ataques al activar la Fase 2 (lo reduce un 15%).</summary>
    private const float Phase2AttackSpeedMultiplier = 0.85f;

    /// <summary>Multiplicador de velocidad y daño aplicado en la Fase 3 (Enrage).</summary>
    private const float Phase3SpeedMultiplier = 1.5f;

    /// <summary>Número de ataques posibles en Fase 1 (Dash, Cuchillas).</summary>
    private const int AttackCountPhase1 = 2;

    /// <summary>Número de ataques posibles a partir de la Fase 2 (Dash, Cuchillas, Minions).</summary>
    private const int AttackCountPhase2 = 3;

    /// <summary>Índice del ataque de Dash en ExecuteRandomAttack.</summary>
    private const int AttackDash = 0;

    /// <summary>Índice del ataque de Cuchillas en ExecuteRandomAttack.</summary>
    private const int AttackBlades = 1;

    /// <summary>Índice del ataque de invocación de Minions en ExecuteRandomAttack.</summary>
    private const int AttackSummons = 2;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Componente Health del jefe, usado para leer la vida actual y comprobar las fases.</summary>
    private Health _health;

    /// <summary>Componente de movimiento del jefe, activado tras la espera inicial y potenciado en Fase 3.</summary>
    private BossBehaviour _movement;

    /// <summary>Componente del ataque de Dash, ejecutado en ExecuteRandomAttack y potenciado en Fase 3.</summary>
    private BossFirstShoot _dash;

    /// <summary>Componente del ataque de Cuchillas, ejecutado en ExecuteRandomAttack y potenciado en Fase 3.</summary>
    private SecondAttackBoss _blades;

    /// <summary>Componente de la habilidad de cristales, activada al entrar en Fase 2.</summary>
    private AbilityBoss1 _crystals;

    /// <summary>Componente de invocación de minions, activada al entrar en Fase 2.</summary>
    private AbilityBoss2 _summons;

    /// <summary>True una vez activada la Fase 2.</summary>
    private bool _phase2Activated = false;

    /// <summary>True una vez activada la Fase 3 (Enrage).</summary>
    private bool _phase3Activated = false;

    /// <summary>True una vez que el jugador ha sido detectado dentro de _detectionRange.</summary>
    private bool _isPlayerDetected = false;

    /// <summary>
    /// Temporizador compartido: tras detectar al jugador cuenta la espera
    /// inicial (_initialDelay), y después el cooldown entre ataques (_timeBetweenAttacks).
    /// </summary>
    private float _attackCooldownTimer = 0f;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea todos los componentes del jefe necesarios para gestionar el combate.
    /// </summary>
    private void Start()
    {
        _health = GetComponent<Health>();
        _movement = GetComponent<BossBehaviour>();
        _dash = GetComponent<BossFirstShoot>();
        _blades = GetComponent<SecondAttackBoss>();
        _crystals = GetComponent<AbilityBoss1>();
        _summons = GetComponent<AbilityBoss2>();
    }

    /// <summary>
    /// Mientras no se ha detectado al jugador, comprueba periódicamente si
    /// está dentro del radio de detección. Una vez detectado, comprueba
    /// siempre los cambios de fase por vida (incluso durante la espera
    /// inicial), espera _initialDelay segundos antes de activar el
    /// movimiento, y a partir de entonces gestiona el ciclo de ataques.
    /// </summary>
    private void Update()
    {
        // Si no hay Cori, no hacemos nada más
        if (!_isPlayerDetected)
        {
            CheckForPlayer();
            return;
        }

        // Siempre leemos la vida, incluso durante la espera inicial,
        // para que las fases se activen si le pegamos mientras espera.
        CheckHealthAndPhases();

        // Gestión de la espera inicial (_initialDelay segundos)
        if (_attackCooldownTimer > 0)
        {
            _attackCooldownTimer -= Time.deltaTime;
            return; // Durante esta espera el jefe no se mueve ni ataca, pero sí escucha su vida
        }

        // La espera inicial ha terminado: el jefe empieza a moverse y atacar
        if (_movement != null)
        {
            _movement.SetMovementActive(true);
        }

        HandleAttackCycle();
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Esta clase no expone métodos públicos.
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>Comprueba la vida actual del jefe y activa la Fase 2/3 si corresponde.</summary>
    private void CheckHealthAndPhases()
    {
        int currentHealth = _health.GetCurrentHealth();

        // FASE 2: empieza cuando la vida baja de Phase2HealthThreshold
        // (y aún no ha entrado en Fase 3)
        if (currentHealth <= Phase2HealthThreshold && currentHealth > Phase3HealthThreshold && !_phase2Activated)
        {
            ActivatePhase2();
        }

        // FASE 3 (Enrage): empieza cuando la vida baja de Phase3HealthThreshold
        if (currentHealth <= Phase3HealthThreshold && !_phase3Activated)
        {
            ActivatePhase3();
        }
    }

    /// <summary>Comprueba si el jugador está dentro del radio de detección y, si es así, inicia la espera previa al combate.</summary>
    private void CheckForPlayer()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, _detectionRange, _playerLayer);
        if (player != null)
        {
            _isPlayerDetected = true;
            _attackCooldownTimer = _initialDelay;

            Debug.Log("Player Detected: Raven waiting " + _initialDelay + "s before attacking.");
        }
    }

    /// <summary>Gestiona el cooldown entre ataques y lanza un ataque aleatorio cuando termina.</summary>
    private void HandleAttackCycle()
    {
        if (_attackCooldownTimer > 0)
        {
            _attackCooldownTimer -= Time.deltaTime;
            return;
        }

        ExecuteRandomAttack();
        _attackCooldownTimer = _timeBetweenAttacks;
    }

    /// <summary>Elige y ejecuta un ataque aleatorio entre los disponibles según la fase actual.</summary>
    private void ExecuteRandomAttack()
    {
        int maxAttack = _phase2Activated ? AttackCountPhase2 : AttackCountPhase1;
        int choice = Random.Range(0, maxAttack);

        switch (choice)
        {
            case AttackDash: _dash.ExecuteDashAttack(); break;
            case AttackBlades: _blades.ExecuteBladeAttack(); break;
            case AttackSummons: _summons.ExecuteSummoning(); break;
        }
    }

    /// <summary>Activa la Fase 2: desbloquea cristales y minions, y acelera el ritmo de ataque.</summary>
    private void ActivatePhase2()
    {
        _phase2Activated = true;
        if (_crystals != null) _crystals.SetAbilityActive(true);

        if (_summons != null) _summons.ActivarInvocacion();

        _timeBetweenAttacks *= Phase2AttackSpeedMultiplier;
    }

    /// <summary>Activa la Fase 3 (Enrage): aumenta velocidad de movimiento/ataques y reduce el tiempo entre ataques.</summary>
    private void ActivatePhase3()
    {
        _phase3Activated = true;

        if (_movement != null) _movement.BuffSpeed(Phase3SpeedMultiplier);
        if (_dash != null) _dash.AplicarBuffFaseFinal(Phase3SpeedMultiplier);
        if (_blades != null) _blades.AplicarBuffFaseFinal(Phase3SpeedMultiplier);

        _timeBetweenAttacks /= Phase3SpeedMultiplier;

        Debug.Log($"Phase 3 Activated: Enrage mode (x{Phase3SpeedMultiplier} Speed).");
    }

    #endregion

} // class BossPhaseController
  // Marián Navarro y Laura Garay