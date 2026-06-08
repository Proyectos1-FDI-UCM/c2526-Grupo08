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
    [Tooltip("Detection range to trigger the fight when Cori enters.")]
    [SerializeField] private float _detectionRange = 12f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Combat Timing")]
    [Tooltip("Idle time between attacks to prevent overlapping.")]
    [SerializeField] private float _timeBetweenAttacks = 2.0f;

    [Header("Combat Timing")]
    [SerializeField] private float _initialDelay = 5.0f; // Los 5 segundos de espera



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
    private BoosBehaviour _movement;
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

    private void MuerteBoss()
    {
        Debug.Log("Raven derrotado. Desactivando objeto...");

        // Desactivamos el objeto para que desaparezca y no de errores de referencia
        // Es mejor que Destroy porque así no rompe scripts que le estén mirando
        gameObject.SetActive(false);
    }

    private void CheckHealthAndPhases()
    {
        int currentHealth = _health.GetCurrentHealth();

        // FASE 2: Empieza cuando la vida baja de 1000 (y es mayor que 500)
        if (currentHealth <= 1000 && currentHealth > 500 && !_phase2Activated)
        {
            ActivatePhase2();
        }

        // FASE 3 (Enrage): Empieza cuando la vida baja de 500
        if (currentHealth <= 500 && !_phase3Activated)
        {
            ActivatePhase3();
        }
    }
    private void CheckForPlayer()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, _detectionRange, _playerLayer);
        if (player != null)
        {
            _isPlayerDetected = true;

            // --- CAMBIO CLAVE ---
            // En lugar de empezar en 0, le decimos que espere el delay inicial
            _attackCooldownTimer = _initialDelay;

            Debug.Log("Player Detected: Raven waiting " + _initialDelay + "s before attacking.");
        }
    }

    private void Awake()
    {
        _health = GetComponent<Health>();
        _movement = GetComponent<BoosBehaviour>();
        _dash = GetComponent<BossFisrtShoot>();
        _blades = GetComponent<SecondAttackBoss>();
        _crystals = GetComponent<AbilityBoss1>();
        _summons = GetComponent<AbilityBoss2>();
    }

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

    private void ExecuteRandomAttack()
    {
        // 0 = Dash, 1 = Cuchillas, 2 = Minions (solo en Fase 2)
        int maxAttack = _phase2Activated ? 3 : 2;
        int choice = Random.Range(0, maxAttack);

        switch (choice)
        {
            case 0: _dash.ExecuteDashAttack(); break;
            case 1: _blades.ExecuteBladeAttack(); break;
            case 2: _summons.ExecuteSummoning(); break; // Los minions ahora sí saldrán
        }
    }

    private void ActivatePhase2()
    {
        _phase2Activated = true;
        if (_crystals != null) _crystals.SetAbilityActive(true);

        // ACTIVAR MINIONS AQUÍ
        if (_summons != null) _summons.ActivarInvocacion();

        _timeBetweenAttacks *= 0.85f;
    }

    private void ActivatePhase3()
    {
        _phase3Activated = true;
        float multiplier = 1.5f;

        // Buff speeds
        if (_movement != null) _movement.BuffSpeed(multiplier);
        if (_dash != null) _dash.AplicarBuffFaseFinal(multiplier);
        if (_blades != null) _blades.AplicarBuffFaseFinal(multiplier);

        // Drastically reduce time between attacks
        _timeBetweenAttacks /= multiplier;

        Debug.Log("Phase 3 Activated: Enrage mode (x1.5 Speed).");
    }
}// class BossPhaseController 
 // namespace
    #endregion