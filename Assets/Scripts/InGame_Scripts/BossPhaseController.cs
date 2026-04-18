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
    /// Start is called on the frame when a script is enabled just before 
    /// any of the Update methods are called the first time.
    /// </summary>
    void Start()
    {

    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>

    private void Update()
    {
        // 1. Initial State: Wait for Cori to enter the room
        if (!_isPlayerDetected)
        {
            CheckForPlayer();
            return;
        }

        int currentHealth = _health.GetCurrentHealth();

        // 2. Phase Threshold Logic
        if (currentHealth < 500 && currentHealth >= 200 && !_phase2Activated)
        {
            ActivatePhase2();
        }

        if (currentHealth < 200 && !_phase3Activated)
        {
            ActivatePhase3();
        }

        // 3. Combat Loop: Prevent overlapping attacks
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
    private void CheckForPlayer()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, _detectionRange, _playerLayer);
        if (player != null)
        {
            _isPlayerDetected = true;
            Debug.Log("Player Detected: Raven combat started.");
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
        // Phase 1 has 2 attacks. Phase 2 & 3 have 4 attacks total.
        int attackCount = _phase2Activated ? 4 : 2;
        int choice = Random.Range(0, attackCount);

        switch (choice)
        {
            case 0: _dash.ExecuteDashAttack(); break;    
            case 1: _blades.ExecuteBladeAttack(); break;
            case 2: _crystals.ExecuteGroundCrystals(); break; 
            case 3: _summons.ExecuteSummoning(); break;      
        }
    }

    private void ActivatePhase2()
    {
        _phase2Activated = true;
        if (_crystals != null) _crystals.SetAbilityActive(true);
        if (_summons != null) _summons.enabled = true;

        _timeBetweenAttacks *= 0.85f; // Slightly faster pacing
        Debug.Log("Phase 2 Activated: Ground Crystals and Minions available.");
    }

    private void ActivatePhase3()
    {
        _phase3Activated = true;
        float multiplier = 1.5f;

        // Buff speeds
        if (_movement != null) _movement.AplicarBuffVelocidad(multiplier);
        if (_dash != null) _dash.AplicarBuffFaseFinal(multiplier);
        if (_blades != null) _blades.AplicarBuffFaseFinal(multiplier);

        // Drastically reduce time between attacks
        _timeBetweenAttacks /= multiplier;

        Debug.Log("Phase 3 Activated: Enrage mode (x1.5 Speed).");
    }
}// class BossPhaseController 
// namespace
#endregion
