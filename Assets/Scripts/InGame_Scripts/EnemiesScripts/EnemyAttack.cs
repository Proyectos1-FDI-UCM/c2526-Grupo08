//---------------------------------------------------------
// Controla el ataque cuerpo a cuerpo del enemigo normal.
// Solo ataca cuando la persecución está activa (EnemyPatrol.IsChasing)
// y el jugador está físicamente en contacto con el enemigo.
// Cadencia: 1 ataque cada 1,5 segundos.
// El sonido se reproduce 0,3 s antes del daño real.
// La lógica se gestiona con timers en Update, sin corrutinas.
// Laura Garay Zubiaguirre
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
/// El componente Health del jugador se cachea la primera vez que
/// entra en contacto, evitando GetComponent en ApplyDamage().
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

    [Header("Feedback Visual")]
    [Tooltip("El SpriteRenderer secundario que contiene la imagen del Slash.")]
    [SerializeField] private SpriteRenderer _slashSpriteRenderer;

    [Tooltip("Duración en segundos que la imagen del slash se mantendrá visible.")]
    [SerializeField] private float _slashDuration = 0.15f;

    [Tooltip("Distancia a la que aparecerá del enemigo")]
    [SerializeField] private float _slashOffsetDistance = 0.6f;

    [Header("Referencias")]
    [Tooltip("Script de patrulla del mismo enemigo (se obtiene automáticamente en Start).")]
    [SerializeField] private EnemyPatrol _enemyPatrol;

    [Header("Referencia al jugador")]
    [Tooltip("GameObject del jugador, asignado automáticamente cuando entra en el área de ataque.")]
    [SerializeField] private GameObject _player;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

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

    /// <summary>Componente Health del jugador, cacheado al detectarlo por primera vez.</summary>
    private Health _playerHealth;

    /// <summary>Timer interno para controlar la desaparción del slash(efecto visual)</summary>

    private float _slashVisualTimer = 0f;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea los componentes necesarios.
    /// </summary>
    private void Start()
    {
        // Esto busca el componente automáticamente en el mismo objeto
        _enemyPatrol = GetComponent<EnemyPatrol>();
        _audioSource = GetComponent<AudioSource>();

        if (_enemyPatrol == null)
        {
            Debug.LogError("¡OJO! No he encontrado el script EnemyPatrol en " + gameObject.name);
        }

        //Nos aseguramos de que el feedback visual este apagado (invisible)

        if (_slashSpriteRenderer != null)
            _slashSpriteRenderer.enabled = false;

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
        if (_slashSpriteRenderer != null && _slashSpriteRenderer.enabled)
        {
            _slashVisualTimer += Time.deltaTime;
            if (_slashVisualTimer >= _slashDuration)
            {
                _slashSpriteRenderer.enabled = false;
                _slashVisualTimer = 0f;
            }
        }

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

        // Fase 2: aplicar daño, feedback visual y reiniciar ciclo
        if (_attackTimer >= _attackInterval)
        {
            if (_playerInRange)
            {
                TriggerVisualFeedback();
                ApplyDamage();
            }

            ResetTimer();
        }
    }

    /// <summary>
    /// Detecta cuando el jugador entra en contacto físico con el enemigo
    /// y cachea su componente Health la primera vez.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detecta si lo que entró en el área es el jugador
        PlayerMovement playerMovement = other.gameObject.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            _playerInRange = true;
            _player = other.gameObject;

            if (_playerHealth == null)
                _playerHealth = _player.GetComponent<Health>();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Por seguridad, si el jugador sigue dentro, mantenemos el flag en true
        if (other.gameObject.GetComponent<PlayerMovement>() != null)
        {
            _playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Cuando el jugador sale del área, deja de recibir daño
        if (other.gameObject.GetComponent<PlayerMovement>() != null)
        {
            _playerInRange = false;
        }
    }
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Aplica daño al componente Health del jugador (cacheado previamente).
    /// </summary>
    private void ApplyDamage()
    {
        if (_playerHealth == null)
        {
            Debug.Log("[EnemyMeleeAttack] No hay Health cacheado del jugador.");
            return;
        }

        _playerHealth.Damage((int)_damageAmount);
        Debug.Log($"[EnemyMeleeAttack] Daño de {(int)_damageAmount} enviado al script Health.");
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


    /// <summary>
    /// Calcula la dirección hacia el jugador, rota el sprite del slash (hijo del enemigo) y lo muestra.
    /// </summary>
    private void TriggerVisualFeedback()
    {
        if (_slashSpriteRenderer == null || _player == null) return;

        // 1. Calcular vector dirección (Posición Jugador - Posición Enemigo)
        Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;

        // 2. Calcular el ángulo matemático en base a ese vector
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        Vector3 worldOffset = directionToPlayer * _slashOffsetDistance;
        
        // Aplicamos la posición y rotación globales de forma directa
        _slashSpriteRenderer.transform.position = transform.position + worldOffset;
        _slashSpriteRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 4. ¡EL TRUCO ANTIGIRO!: Si el padre tiene escala negativa en X, 
        // contrarrestamos volteando el sprite del hijo para que no se invierta su matriz.
        Vector3 currentScale = _slashSpriteRenderer.transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * Mathf.Sign(transform.lossyScale.x);
        currentScale.y = Mathf.Abs(currentScale.y) * Mathf.Sign(transform.lossyScale.y);
        _slashSpriteRenderer.transform.localScale = currentScale;

        // 3. Rotar el GameObject del slash hacia el jugador
        //_slashSpriteRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 4. Reposicionar el slash un pelín al frente del enemigo en esa dirección
        //_slashSpriteRenderer.transform.localPosition = directionToPlayer * _slashOffsetDistance;

        // 5. Encender el Sprite y resetear su timer de apagado
        _slashSpriteRenderer.enabled = true;
        _slashVisualTimer = 0f;
    
    }
    #endregion

}
// class EnemyMeleeAttack
// Laura Garay Zubiaguirre
//Adriana Ferández Luna