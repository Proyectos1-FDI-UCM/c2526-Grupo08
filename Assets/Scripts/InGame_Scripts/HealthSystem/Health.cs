//---------------------------------------------------------
// Gestiona los puntos de vida de cualquier personaje (jugador o enemigo).
// Celia García Riaza
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Componente de vida genérico para jugador y enemigos.
///
/// CORRECCIÓN respecto a la versión anterior:
///   · Die() comprobaba MagicPointsPrefab != null pero luego siempre llamaba
///     Instantiate(MagicPointsPrefab, EnemyGameObject.transform.position, ...)
///     → si MagicPointsPrefab O EnemyGameObject eran null, lanzaba
///     NullReferenceException. Ahora ambos se validan antes de instanciar.
///   · Si EnemyGameObject es null en la muerte normal, se usa gameObject
///     como posición del drop de magia (igual que para la destrucción).
/// </summary>
public class Health : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Tooltip("Vida máxima. (GDD: jugador 200, normal 100, rápido 50, fuerte 250)")]
    [SerializeField] private int MaxHealth = 200;

    [Tooltip("Barra de vida que muestra la vida en pantalla.")]
    [SerializeField] private UIBar HealthBar;

    [Tooltip("Solo enemigos: GameObject a destruir al morir. " +
             "Si está vacío se destruye este mismo GameObject.")]
    [SerializeField] private GameObject EnemyGameObject;

    [Tooltip("Marcar true solo en el jugador.")]
    [SerializeField] private bool IsPlayer = false;

    [Tooltip("Marcar true solo en el jefe.")]
    [SerializeField] private bool IsBoss = false;

    [Tooltip("Prefab del punto de magia. Opcional: solo si el enemigo debe soltar magia al morir.")]
    [SerializeField] private GameObject MagicPointsPrefab;

    [Tooltip("Prefab de la llave que soltará el enemigo al morir. Opcional.")]
    [SerializeField] private GameObject KeyPrefab;

    [Tooltip("Marcar true solo en el enemigo especial.")]
    [SerializeField] private bool IsSpecialEnemy = false;

    [Tooltip("Duración en segundos del flash de color al recibir daño o curación.")]
    [SerializeField] private float ColorDuration = 0.3f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    private int _currentHealth;
    private bool _isImmune = false;
    private bool _isDead = false;

    private SpriteRenderer _spriteRenderer;
    private Color _ogColor;
    private float _colorTimer;

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        _currentHealth = MaxHealth;

        if (HealthBar != null)
        {
            HealthBar.SetMaxValue(MaxHealth);
            HealthBar.SetValue(_currentHealth);
        }

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _ogColor = _spriteRenderer.color;
    }

    private void Update()
    {
        if (_colorTimer > 0f)
        {
            _colorTimer -= Time.deltaTime;
            if (_colorTimer <= 0f && _spriteRenderer != null)
                _spriteRenderer.color = _ogColor;
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>Aplica daño. Si la vida llega a 0 ejecuta la lógica de muerte.</summary>
    public void Damage(int damageAmount)
    {
        if (_isDead || _isImmune) { return; }

        _currentHealth -= damageAmount;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        if (HealthBar != null)
            HealthBar.SetValue(_currentHealth);

        if (_currentHealth > 0 && _spriteRenderer != null)
        {
            _spriteRenderer.color = Color.red;
            _colorTimer = ColorDuration;
        }

        if (_currentHealth <= 0) { Die(); }
    }

    /// <summary>Cura al personaje sin superar el máximo de vida.</summary>
    public void Healing(int healAmount)
    {
        if (_isDead) { return; }

        _currentHealth = Mathf.Min(_currentHealth + healAmount, MaxHealth);

        if (HealthBar != null)
            HealthBar.SetValue(_currentHealth);

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.green;
            _colorTimer = ColorDuration;
        }
    }

    public void SetImmune(bool immune) => _isImmune = immune;
    public bool IsImmune() => _isImmune;
    public int GetCurrentHealth() => _currentHealth;

    /// <summary>Restaura la vida al valor del checkpoint. Llamado por LevelManager.</summary>
    public void SetHealthFromCheckpoint(int savedHealth)
    {
        _currentHealth = Mathf.Clamp(savedHealth, 0, MaxHealth);
        _isDead = false;

        if (HealthBar != null)
        {
            HealthBar.SetMaxValue(MaxHealth);
            HealthBar.SetValue(_currentHealth);
        }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void Die()
    {
        if (_isDead) { return; }
        _isDead = true;

        if (IsPlayer)
        {
            if (BossManager.HasInstance())
                BossManager.Instance.OnPlayerDeath();
            else if (LevelManager.HasInstance())
                LevelManager.Instance.OnPlayerDeath();
            else
                Debug.LogWarning("[Health] No hay BossManager ni LevelManager en la escena.");
            return;
        }

        if (IsBoss)
        {
            if (BossManager.HasInstance())
                BossManager.Instance.OnBossDeath();
            else if (LevelManager.HasInstance())
                LevelManager.Instance.OnBossDeath();
            else
                Debug.LogWarning("[Health] No hay BossManager ni LevelManager en la escena.");
            return;
        }

        // --- Enemigo especial ---
        if (IsSpecialEnemy)
        {
            SpecialEnemyDeath specialDeath = GetComponent<SpecialEnemyDeath>();
            if (specialDeath != null)
                specialDeath.OnDefeated();
            else
                Debug.LogWarning($"[Health] {gameObject.name} es IsSpecialEnemy pero no tiene SpecialEnemyDeath.");
            return;
        }

        // --- Muerte normal de enemigo ---
        // El GameObject a destruir: EnemyGameObject si está asignado, sino este mismo.
        GameObject toDestroy = EnemyGameObject != null ? EnemyGameObject : gameObject;

        // Llave opcional
        if (KeyPrefab != null)
            Instantiate(KeyPrefab, toDestroy.transform.position, Quaternion.identity);

        // Drop de magia opcional — CORRECCIÓN: doble null-check
        if (MagicPointsPrefab != null)
            Instantiate(MagicPointsPrefab, toDestroy.transform.position, Quaternion.identity);

        Destroy(toDestroy);
    }

    #endregion

} // class Health
  // Celia García Riaza