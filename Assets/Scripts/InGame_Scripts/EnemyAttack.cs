//---------------------------------------------------------
// Controla el ataque cuerpo a cuerpo del enemigo normal.
// Solo ataca cuando la persecución está activa (EnemyPatrol.IsChasing)
// y el jugador está físicamente en contacto con el enemigo.
// Cadencia: 1 ataque cada 1,5 segundos.
// El sonido se reproduce 0,3 s antes del daño real.
// La lógica se gestiona con timers en Update, sin corrutinas.
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Gestiona el ciclo de ataque cuerpo a cuerpo del enemigo normal.
/// Se apoya en EnemyPatrol.IsChasing para saber cuándo activarse,
/// y en colisiones físicas (_playerInRange) para confirmar el contacto.
/// Ciclo de _attackInterval segundos:
///   1. Reproduce el AudioClip de ataque (advertencia sonora).
///   2. Transcurridos _soundLeadTime s, aplica el daño al jugador.
///   3. Espera el resto del intervalo antes de repetir.
/// </summary>
[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(AudioSource))]
public class EnemyMeleeAttack : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Configuración de Daño")]
    [Tooltip("Puntos de daño que aplica cada golpe al jugador.")]
    [SerializeField] private float _damageAmount = 10f;

    [Tooltip("Tiempo total entre ataques en segundos.")]
    [SerializeField] private float _attackInterval = 1.5f;

    [Tooltip("Adelanto del sonido respecto al daño real en segundos.")]
    [SerializeField] private float _soundLeadTime = 0.3f;

    [Header("Audio")]
    [Tooltip("Sonido que se reproduce como advertencia antes de aplicar el daño.")]
    [SerializeField] private AudioClip _attackSound;

    [Header("Referencias")]
    [SerializeField] private EnemyPatrol _enemyPatrol;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Referencia al componente EnemyPatrol del mismo GameObject.</summary>

    /// <summary>AudioSource del enemigo para reproducir el sonido de ataque.</summary>
    private AudioSource _audioSource;

    /// <summary>True mientras el jugador está físicamente en contacto con el enemigo.</summary>
    private bool _playerInRange = false;

    /// <summary>
    /// Acumulador de tiempo del ciclo de ataque.
    /// Solo avanza cuando IsChasing es true.
    /// </summary>
    private float _attackTimer = 0f;

    /// <summary>
    /// Indica si el sonido de advertencia ya se reprodujo en el ciclo actual.
    /// </summary>
    private bool _soundPlayed = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea los componentes necesarios.
    /// </summary>
    /*private void Start()
    {
        _enemyPatrol = GetComponent<EnemyPatrol>();
        _audioSource = GetComponent<AudioSource>();

        ResetTimer();
    }*/
    private void Start()
    {
        // Esto busca el componente automáticamente en el mismo objeto
        _enemyPatrol = GetComponent<EnemyPatrol>();
        _audioSource = GetComponent<AudioSource>();

        if (_enemyPatrol == null)
        {
            Debug.LogError("¡OJO! No he encontrado el script EnemyPatrol en " + gameObject.name);
        }

        ResetTimer();
    }

    /// <summary>
    /// Gestiona el temporizador de ataque frame a frame.
    /// Solo avanza cuando EnemyPatrol.IsChasing es true.
    /// Ciclo de _attackInterval segundos:
    ///   · Al alcanzar (_attackInterval - _soundLeadTime) → reproduce el sonido (si el jugador está cerca).
    ///   · Al alcanzar _attackInterval                    → aplica el daño (si el jugador está cerca) y reinicia.
    /// Si el jugador no está en rango en ninguna de las dos fases, el ciclo simplemente se reinicia
    /// sin efecto, evitando la antigua espera de 0,1 s por frame de la versión con corrutina.
    /// </summary>
    private void Update()
    {
        if (!_enemyPatrol.IsChasing)
        {
            ResetTimer();
            return;
        }

        _attackTimer += Time.deltaTime;

        // Fase 1: reproducir sonido de advertencia
        if (!_soundPlayed && _attackTimer >= _attackInterval - _soundLeadTime)
        {
            if (_playerInRange && _attackSound != null)
                _audioSource.PlayOneShot(_attackSound);

            _soundPlayed = true;
        }

        // Fase 2: aplicar daño y reiniciar ciclo
        if (_attackTimer >= _attackInterval)
        {
            if (_playerInRange)
                ApplyDamage();

            ResetTimer();
        }
    }

    /// <summary>
    /// Detecta cuando el jugador entra en contacto físico con el enemigo.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detecta si lo que entró en el área es el jugador
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            Debug.Log("Jugador entró en el área de ataque.");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Por seguridad, si el jugador sigue dentro, mantenemos el flag en true
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Cuando el jugador sale del área, deja de recibir daño
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            Debug.Log("Jugador salió del área de ataque.");
        }
    }
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Aplica daño al componente Health del jugador.
    /// </summary>
    private void ApplyDamage()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Health health = player.GetComponent<Health>();
        if (health != null)
        {
            health.Damage((int)_damageAmount);
            Debug.Log($"[EnemyMeleeAttack] Daño de {(int)_damageAmount} enviado al script Health.");
        }
    }

    /// <summary>
    /// Reinicia el timer y el flag de sonido al inicio de cada ciclo
    /// o al salir de la persecución.
    /// </summary>
    private void ResetTimer()
    {
        _attackTimer = 0f;
        _soundPlayed = false;
    }

    #endregion

}