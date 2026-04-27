//---------------------------------------------------------
// Controla el comportamiento del enemigo móvil: patrullaje por puntos
// definidos en el inspector y persecución de Cori al detectarla.
// Incluye cambio de sprite según la dirección de movimiento.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Gestiona los dos estados del enemigo móvil: PATRULLA y PERSECUCION.
/// En patrulla se mueve en línea recta de un punto al siguiente.
/// Al detectar a Cori dentro del área de detección inicia la persecución.
/// La persecución se mantiene hasta que Cori salga del área extendida.
/// Al perder a Cori vuelve al punto de patrullaje más cercano.
/// El sprite cambia según la dirección de movimiento usando el mismo
/// sistema de enum + ChangeSprite que PlayerMovement.
/// </summary>
public class EnemyPatrol : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Patrullaje")]
    [Tooltip("Puntos por los que patrullará el enemigo en orden")]
    [SerializeField] private Transform[] PatrolPoints;

    [Tooltip("Distancia mínima al punto para considerarlo alcanzado")]
    [SerializeField] private float PointReachedDistance = 0.1f;

    [Header("Velocidad")]
    [Tooltip("Velocidad base del enemigo (2 pasos/s como el jugador, multiplicada por su factor)")]
    [SerializeField] private float Speed = 2f;

    [Header("Detección")]
    [Tooltip("Radio del área de detección interna (3 casillas)")]
    [SerializeField] private float DetectionRadius = 3f;

    [Tooltip("Radio del área extendida de persecución (3 + 1.5 casillas)")]
    [SerializeField] private float ChaseRadius = 4.5f;

    [Header("Sprites direccionales")]
    [Tooltip("Sprite cuando el enemigo mira/camina hacia arriba")]
    [SerializeField] private Sprite SpriteUp;

    [Tooltip("Sprite cuando el enemigo mira/camina hacia abajo")]
    [SerializeField] private Sprite SpriteDown;

    [Tooltip("Sprite cuando el enemigo mira/camina hacia la izquierda (se espeja para la derecha)")]
    [SerializeField] private Sprite SpriteLeft;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Rigidbody2D del enemigo para moverlo con física</summary>
    private Rigidbody2D _rb;

    /// <summary>SpriteRenderer del enemigo para cambiar el sprite</summary>
    private SpriteRenderer _spriteRenderer;

    /// <summary>Transform del jugador cuando es detectado</summary>
    private Transform _playerTransform;

    /// <summary>Índice del punto de patrullaje actual</summary>
    private int _currentPatrolIndex = 0;

    /// <summary>Estado actual del enemigo</summary>
    private enum EnemyState { Patrol, Chase }
    private EnemyState _state = EnemyState.Patrol;

    /// <summary>Indica si el enemigo ya detectó a Cori y activó la persecución</summary>
    private bool _chaseActivated = false;

    /// <summary>
    /// Propiedad pública de solo lectura que indica si la persecución está activa.
    /// La usa EnemyShoot para saber cuándo debe disparar.
    /// </summary>
    public bool IsChasing => _chaseActivated;

    /// <summary>
    /// Enum de dirección, igual que en PlayerMovement para mantener consistencia
    /// </summary>
    private enum Direction { Up, Down, Right, Left }

    /// <summary>Dirección actual del sprite, para evitar cambios innecesarios cada frame</summary>
    private Direction _currentDirection = Direction.Down;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Se ejecuta al iniciar. Cachea componentes y valida la configuración.
    /// </summary>
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (PatrolPoints == null || PatrolPoints.Length == 0)
        {
            Debug.LogWarning($"[EnemyPatrol] {gameObject.name}: no hay puntos de patrullaje asignados.");
        }
        else
        {
            for (int i = 0; i < PatrolPoints.Length; i++)
            {
                if (PatrolPoints[i] == null)
                    Debug.LogWarning($"[EnemyPatrol] {gameObject.name}: PatrolPoints[{i}] es null. " +
                                     "Asigna el Transform o reduce el tamaño del array en el Inspector.");
            }
        }

        // Encontrar al jugador por tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning($"[EnemyPatrol] {gameObject.name}: no se encontró ningún GameObject con tag 'Player'.");
        }
    }

    /// <summary>
    /// Se ejecuta cada frame. Gestiona la máquina de estados y actualiza el sprite.
    /// </summary>
    private void Update()
    {
        CheckDetection();

        switch (_state)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                Chase();
                break;
        }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Comprueba si Cori está dentro del área de detección o del área extendida
    /// y cambia el estado en consecuencia.
    /// </summary>
    private void CheckDetection()
    {
        if (_playerTransform == null) { return; }

        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

        if (!_chaseActivated)
        {
            // Cori entra en el área de detección interna → activar persecución
            if (distanceToPlayer <= DetectionRadius)
            {
                _chaseActivated = true;
                _state = EnemyState.Chase;
            }
        }
        else
        {
            // La persecución se mantiene hasta que Cori salga del área extendida
            if (distanceToPlayer > ChaseRadius)
            {
                ResetToNearestPatrolPoint();
            }
        }
    }

    /// <summary>
    /// Mueve al enemigo en línea recta de un punto de patrullaje al siguiente.
    /// Actualiza el sprite según la dirección de movimiento.
    /// </summary>
    private void Patrol()
    {
        if (PatrolPoints == null || PatrolPoints.Length == 0) { return; }

        Transform target = PatrolPoints[_currentPatrolIndex];
        // Slot asignado pero vacío → saltar al siguiente punto sin excepción
        if (target == null)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % PatrolPoints.Length;
            return;
        }
        Vector2 destination = target.position;

        MoveTowards(destination);
        UpdateSpriteFromDirection(destination - (Vector2)transform.position);

        // Comprobar si ha llegado al punto actual
        if (Vector2.Distance(transform.position, destination) <= PointReachedDistance)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % PatrolPoints.Length;
        }
    }

    /// <summary>
    /// Mueve al enemigo en línea recta hacia Cori y actualiza el sprite.
    /// No esquiva obstáculos: el deslizamiento del Rigidbody2D lo libera.
    /// </summary>
    private void Chase()
    {
        if (_playerTransform == null) { return; }

        Vector2 directionToPlayer = (Vector2)_playerTransform.position - (Vector2)transform.position;
        MoveTowards(_playerTransform.position);
        UpdateSpriteFromDirection(directionToPlayer);
    }

    /// <summary>
    /// Aplica velocidad al Rigidbody2D en dirección al destino indicado.
    /// </summary>
    /// <param name="destination">Posición destino en el mundo</param>
    private void MoveTowards(Vector2 destination)
    {
        Vector2 direction = (destination - (Vector2)transform.position).normalized;
        _rb.linearVelocity = direction * Speed;
    }

    /// <summary>
    /// Determina la dirección dominante del vector de movimiento y cambia el sprite.
    /// Mismo criterio que PlayerMovement: si el componente X domina → izquierda/derecha,
    /// si domina Y → arriba/abajo.
    /// </summary>
    /// <param name="moveDirection">Vector de dirección de movimiento</param>
    private void UpdateSpriteFromDirection(Vector2 moveDirection)
    {
        // Si el vector es casi cero no cambiamos el sprite (el enemigo está parado)
        if (moveDirection.sqrMagnitude < 0.01f) { return; }

        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
        {
            // Movimiento principalmente horizontal
            if (moveDirection.x > 0)
            {
                ChangeSprite(Direction.Right);
            }
            else
            {
                ChangeSprite(Direction.Left);
            }
        }
        else
        {
            // Movimiento principalmente vertical
            if (moveDirection.y > 0)
            {
                ChangeSprite(Direction.Up);
            }
            else
            {
                ChangeSprite(Direction.Down);
            }
        }
    }

    /// <summary>
    /// Cambia el sprite y el flip horizontal según la dirección indicada.
    /// Usa el mismo patrón que PlayerMovement: SpriteLeft se espeja para la derecha
    /// invirtiendo la escala X del GameObject.
    /// Solo actúa si la dirección es distinta a la actual, para no hacer cambios innecesarios.
    /// </summary>
    /// <param name="newDirection">Nueva dirección a aplicar</param>
    private void ChangeSprite(Direction newDirection)
    {
        if (newDirection == _currentDirection) { return; }

        Vector3 currentScale = gameObject.transform.localScale;

        switch (newDirection)
        {
            case Direction.Up:
                _spriteRenderer.sprite = SpriteUp;
                SetScaleX(Mathf.Abs(currentScale.x));
                break;

            case Direction.Down:
                _spriteRenderer.sprite = SpriteDown;
                SetScaleX(Mathf.Abs(currentScale.x));
                break;

            case Direction.Left:
                _spriteRenderer.sprite = SpriteLeft;
                SetScaleX(Mathf.Abs(currentScale.x));
                break;

            case Direction.Right:
                // Se usa el mismo sprite que izquierda pero con la escala X negativa (espejado)
                _spriteRenderer.sprite = SpriteLeft;
                SetScaleX(-Mathf.Abs(currentScale.x));
                break;
        }

        _currentDirection = newDirection;
    }

    /// <summary>
    /// Ajusta la escala X del GameObject para espejar el sprite horizontalmente.
    /// Mismo método que en PlayerMovement.
    /// </summary>
    /// <param name="x">Valor positivo para normal, negativo para espejado</param>
    private void SetScaleX(float x)
    {
        Vector3 scale = gameObject.transform.localScale;
        scale.x = x;
        gameObject.transform.localScale = scale;
    }

    /// <summary>
    /// Cancela la persecución, encuentra el punto de patrullaje más cercano
    /// y vuelve al estado de patrulla desde ese punto.
    /// </summary>
    private void ResetToNearestPatrolPoint()
    {
        _chaseActivated = false;
        _state = EnemyState.Patrol;
        _rb.linearVelocity = Vector2.zero;

        if (PatrolPoints == null || PatrolPoints.Length == 0) { return; }

        // Buscar el punto más cercano a la posición actual del enemigo
        float minDistance = float.MaxValue;
        int nearestIndex = 0;

        for (int i = 0; i < PatrolPoints.Length; i++)
        {
            float dist = Vector2.Distance(transform.position, PatrolPoints[i].position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestIndex = i;
            }
        }

        _currentPatrolIndex = nearestIndex;
    }

    /// <summary>
    /// Dibuja los radios de detección y las rutas de patrullaje en el editor.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Área de detección interna (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRadius);

        // Área extendida de persecución (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ChaseRadius);

        // Líneas entre puntos de patrullaje (cian)
        if (PatrolPoints != null && PatrolPoints.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < PatrolPoints.Length; i++)
            {
                if (PatrolPoints[i] == null) continue;
                int next = (i + 1) % PatrolPoints.Length;
                if (PatrolPoints[next] == null) continue;
                Gizmos.DrawLine(PatrolPoints[i].position, PatrolPoints[next].position);
                Gizmos.DrawSphere(PatrolPoints[i].position, 0.1f);
            }
        }
    }

    #endregion

} // class EnemyPatrol