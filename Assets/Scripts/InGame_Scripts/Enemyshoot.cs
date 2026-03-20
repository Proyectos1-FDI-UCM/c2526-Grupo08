//---------------------------------------------------------
// Controla el disparo del enemigo normal (pistola).
// Solo dispara cuando la persecución está activa (EnemyPatrol.IsChasing).
// Cadencia: 1 disparo cada 1,5 segundos.
// El sonido se reproduce 0,3 s antes de que se instancie la bala,
// para que el jugador pueda escucharlo y esquivar a tiempo.
// El rango de la bala es el mismo que el del jugador: 12 casillas.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Gestiona el ciclo de disparo del enemigo normal.
/// Se apoya en EnemyPatrol.IsChasing para saber cuándo activarse.
/// Cada 1,5 segundos:
///   1. Reproduce el AudioClip de disparo (advertencia sonora).
///   2. Transcurridos 0,3 s, instancia la bala apuntando al jugador.
///   3. Espera los 1,2 s restantes antes de repetir.
/// La lógica se gestiona con timers en Update, sin corrutinas.
/// </summary>
[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(AudioSource))]
public class EnemyShoot : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Bullet Setup")]
    [Tooltip("Prefab de la bala del enemigo. Debe tener el componente Bullet.")]
    [SerializeField] private GameObject _bulletPrefab;

    [Tooltip("Punto desde donde sale la bala. Si es null, sale desde el centro del enemigo.")]
    [SerializeField] private Transform _shootOrigin;

    [Header("Shoot Settings")]
    [Tooltip("Tiempo total entre disparos en segundos. (GDD: 1,5 s)")]
    [SerializeField] private float _fireInterval = 1.5f;

    [Tooltip("Adelanto del sonido respecto al disparo real en segundos. (GDD: 0,3 s)")]
    [SerializeField] private float _soundLeadTime = 0.3f;

    [Header("Audio")]
    [Tooltip("Sonido que se reproduce como advertencia antes de que salga la bala.")]
    [SerializeField] private AudioClip _shootSound;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Referencia al componente EnemyPatrol del mismo GameObject.</summary>
    private EnemyPatrol _enemyPatrol;

    /// <summary>AudioSource del enemigo para reproducir el sonido de disparo.</summary>
    private AudioSource _audioSource;

    /// <summary>Transform del jugador, buscado por tag al iniciar.</summary>
    private Transform _playerTransform;

    /// <summary>
    /// Acumulador de tiempo del ciclo de disparo.
    /// Avanza cada frame mientras la persecución está activa.
    /// Se inicializa en (_fireInterval - _soundLeadTime) para que el primer sonido
    /// se emita tras ese tiempo y no de forma inmediata al detectar al jugador.
    /// </summary>
    private float _shootTimer = 0f;

    /// <summary>
    /// Indica si el sonido de advertencia ya se reprodujo en el ciclo actual.
    /// Permite que FireBullet() se ejecute exactamente _soundLeadTime después.
    /// </summary>
    private bool _soundPlayed = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea componentes y localiza al jugador por tag.
    /// Inicializa el timer para que el primer disparo no ocurra de inmediato.
    /// </summary>
    private void Start()
    {
        _enemyPatrol = GetComponent<EnemyPatrol>();
        _audioSource = GetComponent<AudioSource>();

        // Buscar al jugador por tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning($"[EnemyShoot] {gameObject.name}: no se encontró ningún GameObject con tag 'Player'.");
        }

        // Validar prefab
        if (_bulletPrefab == null)
        {
            Debug.LogError($"[EnemyShoot] {gameObject.name}: no hay prefab de bala asignado.");
            enabled = false;
            return;
        }

        // Si no se asignó punto de origen, usamos el propio transform del enemigo
        if (_shootOrigin == null)
            _shootOrigin = transform;

        // El timer arranca en el punto del ciclo donde se emite el sonido,
        // así el primer disparo no ocurre instantáneamente al detectar al jugador.
        _shootTimer = _fireInterval - _soundLeadTime;
        _soundPlayed = false;
    }

    /// <summary>
    /// Gestiona el temporizador de disparo frame a frame.
    /// Solo avanza cuando EnemyPatrol.IsChasing es true.
    /// Ciclo de _fireInterval segundos:
    ///   · Al alcanzar (_fireInterval - _soundLeadTime) → reproduce el sonido.
    ///   · Al alcanzar _fireInterval                   → instancia la bala y reinicia.
    /// </summary>
    private void Update()
    {
        if (_playerTransform == null) return;
        if (!_enemyPatrol.IsChasing)
        {
            // Reseteamos el timer al salir de la persecución para que el ciclo
            // comience desde el principio la próxima vez que se active.
            _shootTimer = _fireInterval - _soundLeadTime;
            _soundPlayed = false;
            return;
        }

        _shootTimer += Time.deltaTime;

        // Fase 1: reproducir sonido de advertencia
        if (!_soundPlayed && _shootTimer >= _fireInterval - _soundLeadTime)
        {
            PlayShootSound();
            _soundPlayed = true;
        }

        // Fase 2: instanciar la bala y reiniciar el ciclo
        if (_shootTimer >= _fireInterval)
        {
            FireBullet();
            _shootTimer = 0f;
            _soundPlayed = false;
        }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Reproduce el AudioClip de disparo como advertencia sonora para el jugador.
    /// </summary>
    private void PlayShootSound()
    {
        if (_shootSound == null || _audioSource == null) return;
        _audioSource.PlayOneShot(_shootSound);
    }

    /// <summary>
    /// Instancia la bala y la inicializa hacia la posición actual del jugador.
    /// Reutiliza el componente Bullet (mismo que las balas del jugador).
    /// </summary>
    private void FireBullet()
    {
        if (_playerTransform == null) return;

        // Dirección desde el origen del disparo hacia el jugador
        Vector2 direction = ((Vector2)_playerTransform.position
                            - (Vector2)_shootOrigin.position).normalized;

        GameObject bulletObj = Instantiate(_bulletPrefab, _shootOrigin.position, Quaternion.identity);

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(direction, 20);
        }
        else
        {
            Debug.LogWarning($"[EnemyShoot] {gameObject.name}: el prefab de bala no tiene el componente Bullet.");
            Destroy(bulletObj);
        }
    }

    #endregion

} // class EnemyShoot