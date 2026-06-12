//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Marián Navarro y Laura Garay
// No way down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class BossPhaseController : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints
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
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    private Health _health;
    private BossBehaviour _movement;
    private BossFisrtShoot _dash;
    private SecondAttackBoss _blades;
    private AbilityBoss1 _crystals;
    private AbilityBoss2 _summons;

    private bool _phase2Activated = false;
    private bool _phase3Activated = false;
    private bool _isPlayerDetected = false;
    private float _attackCooldownTimer = 0f;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    // Por defecto están los típicos (Update y Start) pero:
    // - Hay que añadir todos los que sean necesarios
    // - Hay que borrar los que no se usen 


    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>

    private void Update()
    {
        // 1. Si no hay Cori, no hacemos nada más
        if (!_isPlayerDetected)
        {
            CheckForPlayer();
            return;
        }

        // --- ¡ESTO ES LO NUEVO! ---
        // Siempre leemos la vida, incluso durante la espera inicial, 
        // para que las fases se activen si le pegamos mientras espera.
        CheckHealthAndPhases();

        // 2. Gestión de la espera inicial de 5 segundos
        if (_attackCooldownTimer > 0)
        {
            _attackCooldownTimer -= Time.deltaTime;
            return; // Durante estos 5 segundos, el Boss no se mueve ni ataca, pero SÍ escucha su vida
        }

        // 3. Si llegamos aquí, los 5 segundos han terminado: ¡A PELEAR!
        if (_movement != null)
        {
            _movement.SetMovementActive(true);
        }

        HandleAttackCycle();
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

    /// <summary>Desactiva al jefe al ser derrotado, sin destruirlo para evitar referencias rotas.</summary>
    private void MuerteBoss()
    {
        Debug.Log("Raven derrotado. Desactivando objeto...");
        gameObject.SetActive(false);
    }

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
  
}// class BossPhaseController 
 // namespace
#endregion