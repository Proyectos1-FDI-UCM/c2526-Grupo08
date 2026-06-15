//---------------------------------------------------------
// Controla la patrulla del enemigo entre waypoints y detecta
// al jugador para infligirle daño letal en caso de colisión.
// Adriana Fernández Luna
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Hace que el enemigo se mueva entre una serie de waypoints (patrulla)
/// y, si el jugador entra en contacto con su zona de detección,
/// le aplica daño igual a su vida actual (muerte instantánea).
/// </summary>
public class EnemyCamera : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Waypoints")]
    [Tooltip("Lista ordenada de puntos por los que patrulla el enemigo, en bucle.")]
    [SerializeField] private Transform[] WayPoints;

    [Tooltip("Distancia al cuadrado al waypoint a partir de la cual se considera alcanzado.")]
    [SerializeField] private float PointReachedDistance = 0.1f;

    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento del enemigo entre waypoints.")]
    [SerializeField] private float Speed = 2f;

    [Header("Referencia al jugador")]
    [SerializeField] private GameObject _player;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Rigidbody2D del enemigo, cacheado en Start().</summary>
    private Rigidbody2D _rb;

    /// <summary>Índice del waypoint actual hacia el que se dirige el enemigo.</summary>
    private int _currentPoint = 0;

    /// <summary>Componente Health del jugador, cacheado en Start() para no buscarlo en cada colisión.</summary>
    private Health _playerHealth;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea el Rigidbody2D propio y el componente Health del jugador
    /// (buscado una sola vez por tag "Player").
    /// </summary>
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_player != null)
            _playerHealth = _player.GetComponent<Health>();
    }

    /// <summary>
    /// Cada frame, hace avanzar al enemigo hacia su waypoint actual.
    /// </summary>
    void Update()
    {
        Patrol();
    }
    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Esta clase no expone métodos públicos.
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Mueve al enemigo hacia el waypoint actual y avanza al siguiente
    /// cuando se alcanza la distancia mínima (PointReachedDistance).
    /// </summary>
    private void Patrol()
    {
        if (WayPoints == null || WayPoints.Length == 0) { return; }

        Vector2 target = WayPoints[_currentPoint].position;

        Vector2 direction = (target - (Vector2)transform.position).normalized;
        _rb.linearVelocity = direction * Speed;

        if (Vector2.SqrMagnitude((Vector2)transform.position - target) <= PointReachedDistance)
        {
            _currentPoint = (_currentPoint + 1) % WayPoints.Length;
        }
    }

    /// <summary>
    /// Si el jugador entra en contacto con la zona de detección del enemigo,
    /// le aplica daño igual a su vida actual (muerte instantánea).
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_player != null && _playerHealth != null)
        {
            int vidaActual = _playerHealth.GetCurrentHealth();
            _playerHealth.Damage(vidaActual);
        }
    }
    #endregion
}

// class EnemyCamera
// Adriana Fernández Luna