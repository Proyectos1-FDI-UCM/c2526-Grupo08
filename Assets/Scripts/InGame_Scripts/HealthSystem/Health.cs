//---------------------------------------------------------
// Gestiona los puntos de vida de cualquier personaje (jugador o enemigo).
// Al llegar a 0: destruye al enemigo o avisa al LevelManager si es el jugador.
// Celia García Riaza
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Componente de vida genérico para jugador y enemigos.
///
/// Jugador (IsPlayer = true):
///   Al morir avisa al LevelManager de la escena, que muestra el panel
///   de muerte y pausa el juego. LevelManager siempre vive en la escena
///   activa, así que la referencia nunca es inválida.
///
/// Enemigo (IsPlayer = false):
///   Al morir destruye el GameObject indicado en EnemyGameObject,
///   o este mismo GameObject si no se asignó ninguno.
/// </summary>
public class Health : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Tooltip("Vida máxima. (GDD: jugador 200, normal 100, rápido 50, fuerte 250)")]
    [SerializeField] private int MaxHealth = 200;

    [Tooltip("Barra de vida que muestra la vida en pantalla. Asignar desde el Inspector.")]
    [SerializeField] private UIBar HealthBar;

    [Tooltip("Solo enemigos: GameObject a destruir al morir. " +
             "Si está vacío se destruye este mismo GameObject.")]
    [SerializeField] private GameObject EnemyGameObject;

    [Tooltip("Marcar true solo en el jugador.")]
    [SerializeField] private bool IsPlayer = false;

    [Tooltip("Marcar true solo en el jefe.")]
    [SerializeField] private bool IsBoss = false;

    [Tooltip("Prefab del punto de magia. Solo poner si el personaje que tiene Health es un enemigo.")]
    [SerializeField] private GameObject MagicPointsPrefab;

    [Tooltip("Prefab de la llave que soltará el enemigo.")]
    [SerializeField] private GameObject KeyPrefab;
    //esto para cuando el enemigo muera en el nivel 2, suelte la llave. Lo está haciendo Marián.

    [Tooltip("Marcar true solo en el enemigo especial.")]
    [SerializeField] private bool IsSpecialEnemy = false;

    [SerializeField] private float ColorDuration = 0.3f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    private int _currentHealth;
    private bool _isImmune = false;

    /// <summary>Evita procesar la muerte más de una vez.</summary>
    private bool _isDead = false;

    private SpriteRenderer _spriteRenderer;
    private Color _ogColor;
    private float _colorTimer;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
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
        _ogColor = _spriteRenderer.color;
    }

    private void Update()
    {
        if (_colorTimer > 0)
        {
            _colorTimer -= Time.deltaTime;

            if (_colorTimer <= 0)
            { 
                _spriteRenderer.color = _ogColor;
            }
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>Aplica daño. Si la vida llega a 0 ejecuta la lógica de muerte.</summary>
    public void Damage(int damageAmount)
    {
        if (_isDead || _isImmune) return;

        _currentHealth -= damageAmount;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        if (HealthBar != null)
            HealthBar.SetValue(_currentHealth);

        //Cambiar color del player a rojo por un tiempo si le provocan daño
        if (_currentHealth > 0)
        {
            _spriteRenderer.color = Color.red;
            _colorTimer = ColorDuration; //Inicio del cronómetro
        }

        if (_currentHealth <= 0) Die();
    }

    /// <summary>Cura al personaje sin superar el máximo de vida.</summary>
    public void Healing(int healAmount)
    {
        if (_isDead) return;

        _currentHealth = Mathf.Min(_currentHealth + healAmount, MaxHealth);

        if (HealthBar != null)
            HealthBar.SetValue(_currentHealth);

        //Cambiar color del player a verde si se cura
        if (_currentHealth > 0)
        {
            _spriteRenderer.color = Color.green;
            _colorTimer = ColorDuration; //Inicio del cronómetro
        }
    }

    /// <summary>Activa o desactiva la inmunidad al daño (durante el dash).</summary>
    public void SetImmune(bool immune) => _isImmune = immune;

    /// <summary>Devuelve si el personaje es inmune en este momento.</summary>
    public bool IsImmune() => _isImmune;

    /// <summary>Devuelve la vida actual.</summary>
    public int GetCurrentHealth() => _currentHealth;

    /// <summary>
    /// Restaura la vida al valor del checkpoint.
    /// Llamado por LevelManager al iniciar la escena.
    /// </summary>
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

    /// <summary>
    /// Lógica de muerte. El jugador avisa al LevelManager (local de escena).
    /// El enemigo se destruye a sí mismo.
    /// </summary>
    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (IsPlayer)
        {
            if (BossManager.HasInstance())
                BossManager.Instance.OnPlayerDeath();
            else if (LevelManager.HasInstance())
                LevelManager.Instance.OnPlayerDeath();
            else
                Debug.LogWarning("[Health] No hay BossManager ni LevelManager en la escena.");
        }
        else if (IsBoss)
        {
            if (BossManager.HasInstance())
                BossManager.Instance.OnBossDeath();
            else if (LevelManager.HasInstance())
                LevelManager.Instance.OnBossDeath();
            else
                Debug.LogWarning("[Health] No hay BossManager ni LevelManager en la escena.");
        }
        else
        {
            if (IsSpecialEnemy)
            {
                SpecialEnemyDeath specialDeath = GetComponent<SpecialEnemyDeath>();

                if (specialDeath != null)
                {
                    specialDeath.OnDefeated();
                }
                else
                {
                    Debug.LogWarning($"[Health] {gameObject.name} es IsSpecialEnemy pero no tiene el componente SpecialEnemyDeath.");
                }

                return;
            }

            // --- Muerte normal de enemigo ---
            GameObject toDestroy = EnemyGameObject != null ? EnemyGameObject : gameObject;

            if (KeyPrefab != null)
            {
                Instantiate(KeyPrefab, toDestroy.transform.position, Quaternion.identity);
            }

            Destroy(toDestroy);

            GameObject _magicPointObj = Instantiate(MagicPointsPrefab, EnemyGameObject.transform.position, Quaternion.identity);
        }
    }

    #endregion

} // class Health