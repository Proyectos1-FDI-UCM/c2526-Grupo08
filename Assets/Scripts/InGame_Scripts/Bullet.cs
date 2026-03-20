//---------------------------------------------------------
// Componente que controla el movimiento y ciclo de vida de una bala.
// Se instancia por el jugador y se desplaza en línea recta hasta
// alcanzar su rango máximo o colisionar con algo.
// Alexia y Marián
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Controla el movimiento de la bala en el espacio 2D.
/// Se mueve en la dirección que se le asigna al instanciarse
/// y se destruye al superar el rango máximo o al impactar.
/// Según el GDD: ataque básico con rango de 12 casillas.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Bullet Settings")]
    [Tooltip("Velocidad de desplazamiento de la bala (en unidades/segundo).")]
    [SerializeField] private float _speed = 15f;

    [Tooltip("Rango máximo en casillas de Unity antes de destruirse. (GDD ataque básico: 12 casillas)")]
    [SerializeField] private float _maxRange = 12f;

    [Tooltip("Daño que inflige la bala al impactar. (GDD ataque básico: 20 de daño)")]
    [SerializeField] private int _damage = 20;

    #endregion


    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Posición donde se instanció la bala, para calcular la distancia recorrida.</summary>
    private Vector2 _spawnPosition;

    /// <summary>Rigidbody2D del GameObject de la bala.</summary>
    private Rigidbody2D _rb;

    /// <summary>Dirección de movimiento de la bala, asignada al instanciarse.</summary>
    private Vector2 _direction;

    private Health _health;

    #endregion


    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Awake se llama antes que Start. Cacheamos el Rigidbody2D aquí
    /// para que esté disponible cuando el jugador llame a Init().
    /// </summary>
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();

        // La bala no debe girar por físicas, solo se mueve en línea recta
        _rb.gravityScale = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    /// <summary>
    /// Start se llama en el primer frame. Guardamos la posición de spawn
    /// para calcular cuánto ha recorrido la bala.
    /// </summary>
    private void Start()
    {
        _spawnPosition = transform.position;
    }

    /// <summary>
    /// Update comprueba cada frame si la bala ha superado el rango máximo.
    /// </summary>
    private void Update()
    {
        // Si ha recorrido más distancia que el rango máximo, se destruye
        float distanceTravelled = Vector2.Distance(_spawnPosition, transform.position);
        if (distanceTravelled >= _maxRange)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Al colisionar con cualquier objeto aplica daño y se destruye.
    /// No necesita guardas de tag porque la Physics2D Layer Collision Matrix
    /// ya impide que esta bala colisione con quien la disparó u otras balas.
    /// Capas configuradas en Project Settings → Physics 2D:
    ///   · PlayerBullet no colisiona con: Player, PlayerBullet
    ///   · EnemyBullet  no colisiona con: Enemy,  EnemyBullet
    /// </summary>
    /// 
    private void OnTriggerEnter2D(Collider2D other)
    {
        _health = other.GetComponent<Health>();
        if (_health != null)
        {
            _health.Damage(_damage);
        }
        Destroy(gameObject);
    }


    #endregion


    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Inicializa la dirección de la bala. Debe llamarse justo después del Instantiate,
    /// antes de que el Start de la bala se ejecute.
    /// </summary>
    /// <param name="direction">Vector normalizado con la dirección de disparo.</param>
    public void Init(Vector2 direction, int damage)
    {
        _direction = direction.normalized;

        _damage = damage;

        // Rotamos el sprite de la bala para que apunte en la dirección correcta
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Asignamos la velocidad al Rigidbody2D (necesita Rigidbody2D, NO Rigidbody 3D)
        _rb.linearVelocity = _direction * _speed;
    }


    #endregion

} // class Bullet