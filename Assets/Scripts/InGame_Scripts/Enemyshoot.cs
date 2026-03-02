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

using System.Collections;
using UnityEngine;

/// <summary>
/// Gestiona el ciclo de disparo del enemigo normal.
/// Se apoya en EnemyPatrol.IsChasing para saber cuándo activarse.
/// Usa una Coroutine que cada 1,5 segundos:
///   1. Reproduce el AudioClip de disparo (advertencia sonora).
///   2. Espera 0,3 s.
///   3. Instancia la bala apuntando a la posición actual de Cori.
///   4. Espera los 1,2 s restantes antes de repetir.
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

    /// <summary>Referencia a la Coroutine activa para poder detenerla.</summary>
    private Coroutine _shootCoroutine;

    /// <summary>Indica si la Coroutine de disparo está en marcha.</summary>
    private bool _isShooting = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea componentes y localiza al jugador por tag.
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
        {
            _shootOrigin = transform;
        }
    }

    /// <summary>
    /// Cada frame comprueba si la persecución acaba de activarse o desactivarse
    /// para arrancar o detener la Coroutine de disparo.
    /// </summary>
    private void Update()
    {
        if (_playerTransform == null) return;

        bool chasing = _enemyPatrol.IsChasing;

        // Arrancar la Coroutine cuando empieza la persecución
        if (chasing && !_isShooting)
        {
            _shootCoroutine = StartCoroutine(ShootLoop());
            _isShooting = true;
        }
        // Detener la Coroutine cuando la persecución termina
        else if (!chasing && _isShooting)
        {
            StopCoroutine(_shootCoroutine);
            _isShooting = false;
        }
    }

    /// <summary>
    /// Asegurarse de parar la Coroutine si el GameObject se desactiva o destruye.
    /// </summary>
    private void OnDisable()
    {
        if (_isShooting && _shootCoroutine != null)
        {
            StopCoroutine(_shootCoroutine);
            _isShooting = false;
        }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Bucle principal de disparo. Se ejecuta mientras la persecución esté activa.
    /// Ciclo de 1,5 s:
    ///   → Reproduce el sonido de advertencia.
    ///   → Espera 0,3 s.
    ///   → Instancia la bala.
    ///   → Espera los 1,2 s restantes del ciclo.
    /// </summary>
    private IEnumerator ShootLoop()
    {
        // Pequeña pausa inicial para que el primer disparo no sea inmediato al detectar
        yield return new WaitForSeconds(_fireInterval - _soundLeadTime);

        while (true)
        {
            // 1. Reproducir el sonido de advertencia
            PlayShootSound();

            // 2. Esperar el adelanto: el jugador tiene este tiempo para esquivar
            yield return new WaitForSeconds(_soundLeadTime);

            // 3. Instanciar la bala apuntando a la posición actual de Cori
            FireBullet();

            // 4. Esperar el resto del intervalo antes del siguiente ciclo
            yield return new WaitForSeconds(_fireInterval - _soundLeadTime);
        }
    }

    /// <summary>
    /// Reproduce el AudioClip de disparo como advertencia sonora para el jugador.
    /// </summary>
    private void PlayShootSound()
    {
        if (_shootSound == null || _audioSource == null) return;

        _audioSource.PlayOneShot(_shootSound);
    }

    /// <summary>
    /// Instancia la bala y la inicializa hacia la posición actual de Cori.
    /// Reutiliza el componente Bullet (mismo que las balas del jugador).
    /// </summary>
    private void FireBullet()
    {
        if (_playerTransform == null) return;

        // Calculamos la dirección desde el origen del disparo hacia Cori
        Vector2 direction = ((Vector2)_playerTransform.position - (Vector2)_shootOrigin.position).normalized;

        // Instanciamos la bala
        GameObject bulletObj = Instantiate(_bulletPrefab, _shootOrigin.position, Quaternion.identity);

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(direction);
        }
        else
        {
            Debug.LogWarning($"[EnemyShoot] {gameObject.name}: el prefab de bala no tiene el componente Bullet.");
            Destroy(bulletObj);
        }
    }

    #endregion

} // class EnemyShoot