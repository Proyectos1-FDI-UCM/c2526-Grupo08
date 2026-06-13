//---------------------------------------------------------
// Movimiento base del jefe: deambula de forma aleatoria
// por el área de combate con transiciones suaves de velocidad.
// Alexia Perez y Marián Navarro
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Controla el movimiento base del jefe.
/// Elige puntos aleatorios dentro del área de combate y se desplaza
/// hacia ellos con interpolación suave de velocidad (sin Coroutines).
/// Coexiste con BossFirstShoot: el dash de ese script sobreescribe
/// la velocidad temporalmente; cuando el damping la reduce, este
/// script retoma el control en el siguiente frame.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BossBehaviour : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Movimiento")]
    [Tooltip("Velocidad máxima de desplazamiento base del jefe.")]
    [SerializeField] private float Speed = 2.5f;

    [Tooltip("Velocidad de interpolación de la velocidad (suavidad). " +
             "Valores bajos = más suave pero más lento en reaccionar. " +
             "Recomendado: 3-6.")]
    [SerializeField] private float SmoothSpeed = 4f;

    [Tooltip("Distancia mínima al punto objetivo para considerarlo alcanzado " +
             "y elegir uno nuevo.")]
    [SerializeField] private float MinimumDistanceArrive = 0.4f;

    [Header("Temporización")]
    [Tooltip("Tiempo mínimo en segundos antes de elegir un nuevo punto aleatorio.")]
    [SerializeField] private float MinimumTimeBetweenPoints = 2f;

    [Tooltip("Tiempo máximo en segundos antes de elegir un nuevo punto aleatorio.")]
    [SerializeField] private float MaxTimeBetweenPoints = 5f;

    [Header("Área de movimiento")]
    [Tooltip("Tamaño del rectángulo (Ancho X, Alto Y) dentro del cual el jefe " +
             "elige sus puntos aleatorios. Centrado en la posición inicial del jefe.")]
    [SerializeField] private Vector2 MovementArea = new Vector2(8f, 6f);

    [Header("Visualización del Gizmo")]
    [Tooltip("Color del área de movimiento en el editor.")]
    [SerializeField] private Color ColorGizmo = new Color(0f, 1f, 1f, 0.2f);

    [Header("Animación")]
    [Tooltip("Magnitud al cuadrado de la velocidad por debajo de la cual se considera que el jefe está parado.")]
    [SerializeField] private float MinMoveSqrMagnitude = 0.01f;

    [Tooltip("Tiempo de suavizado (dampTime) usado al actualizar MoveX/MoveY en el Animator.")]
    [SerializeField] private float AnimatorDampTime = 0.15f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Rigidbody2D del jefe, cacheado en Awake.</summary>
    private Rigidbody2D _rb;

    /// <summary>Punto aleatorio actual hacia el que se mueve el jefe.</summary>
    private Vector2 _goalPoint;

    /// <summary>Posición inicial del jefe, usada como centro del área de movimiento.</summary>
    private Vector2 _areaCenter;

    /// <summary>Timer que cuenta el tiempo hasta elegir un nuevo punto.</summary>
    private float _timerChangePoint;

    /// <summary>Tiempo aleatorio hasta el próximo cambio de punto.</summary>
    private float _timeUntilChange;

    /// <summary>Animator del jefe, usado para reflejar el movimiento en sus animaciones.</summary>
    private Animator _animator;

    /// <summary>Última dirección de movimiento hacia el punto objetivo, usada para la animación.</summary>
    private Vector2 _currentDirection;

    /// <summary>Indica si el movimiento del jefe está activo. Lo controla BossPhaseController.</summary>
    private bool _isActive = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea el Rigidbody2D y el Animator antes que Start para que estén
    /// disponibles desde el primer frame.
    /// </summary>
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Guarda el centro del área y elige el primer punto aleatorio.
    /// </summary>
    private void Start()
    {
        _areaCenter = transform.position;
        ChooseNewPoint();
    }

    /// <summary>
    /// Cada frame: mueve el jefe hacia el punto objetivo con velocidad suavizada.
    /// Cambia de punto cuando llega o cuando expira el timer.
    /// </summary>
    private void Update()
    {
        if (!_isActive) return;

        ActualizeTimer();
        MoveTowardsObjective();
        UpdateAnimation();
    }

    /// <summary>
    /// Dibuja el área de movimiento en el editor para facilitar el ajuste.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 centro = Application.isPlaying ? (Vector3)_areaCenter : transform.position;

        Gizmos.color = ColorGizmo;
        Gizmos.DrawCube(centro, new Vector3(MovementArea.x, MovementArea.y, 0.1f));

        // Borde sólido
        Gizmos.color = new Color(ColorGizmo.r, ColorGizmo.g, ColorGizmo.b, 1f);
        Gizmos.DrawWireCube(centro, new Vector3(MovementArea.x, MovementArea.y, 0.1f));

        // Punto objetivo actual (solo en runtime)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_goalPoint, 0.2f);
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Multiplica la velocidad base y la suavidad por multiplicador.
    /// Usado por las fases del jefe para volverlo más agresivo.
    /// </summary>
    /// <param name="multiplicador">Factor por el que se multiplican Speed y SmoothSpeed.</param>
    public void BuffSpeed(float multiplicador)
    {
        Speed *= multiplicador;

        // Multiplicamos también SmoothSpeed para que el jefe cambie de dirección más agresivamente
        SmoothSpeed *= multiplicador;

        Debug.Log($"<color=cyan>[Boss] Velocidad aumentada a: {Speed}</color>");
    }

    /// <summary>
    /// Activa o desactiva el movimiento del jefe. Si se desactiva, detiene
    /// inmediatamente el Rigidbody2D.
    /// </summary>
    /// <param name="state">True para activar el movimiento, false para detenerlo.</param>
    public void SetMovementActive(bool state)
    {
        _isActive = state;
        if (!state && _rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Avanza el timer y elige un nuevo punto cuando expira.
    /// También cambia si el jefe ya llegó al punto actual.
    /// </summary>
    private void ActualizeTimer()
    {
        _timerChangePoint += Time.deltaTime;

        bool tiempoAgotado = _timerChangePoint >= _timeUntilChange;
        bool llegado = Vector2.Distance(transform.position, _goalPoint) <= MinimumDistanceArrive;

        if (tiempoAgotado || llegado)
        {
            ChooseNewPoint();
        }
    }

    /// <summary>
    /// Aplica velocidad suavizada hacia el punto objetivo usando Lerp.
    /// Esto evita el movimiento brusco: la velocidad aumenta y disminuye gradualmente.
    /// </summary>
    private void MoveTowardsObjective()
    {
        Vector2 direccion = (_goalPoint - (Vector2)transform.position).normalized;
        Vector2 velocidadDeseada = direccion * Speed;

        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, velocidadDeseada, SmoothSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Elige un punto aleatorio dentro del rectángulo centrado en _areaCenter
    /// y reinicia el timer con un tiempo aleatorio entre los límites configurados.
    /// </summary>
    private void ChooseNewPoint()
    {
        float mitadX = MovementArea.x / 2f;
        float mitadY = MovementArea.y / 2f;

        _goalPoint = new Vector2(
            _areaCenter.x + Random.Range(-mitadX, mitadX),
            _areaCenter.y + Random.Range(-mitadY, mitadY)
        );

        _timerChangePoint = 0f;
        _timeUntilChange = Random.Range(MinimumTimeBetweenPoints, MaxTimeBetweenPoints);
        _currentDirection = (_goalPoint - (Vector2)transform.position).normalized;
    }

    /// <summary>
    /// Actualiza los parámetros del Animator (Speed, MoveX, MoveY) y el
    /// volteo del sprite según la dirección hacia el punto objetivo.
    /// </summary>
    private void UpdateAnimation()
    {
        Vector2 direction = _currentDirection;

        if (_rb.linearVelocity.sqrMagnitude < MinMoveSqrMagnitude)
        {
            _animator.SetFloat("Speed", 0f);
            return;
        }

        _animator.SetFloat("Speed", 1f);
        _animator.SetFloat("MoveX", direction.x, AnimatorDampTime, Time.deltaTime);
        _animator.SetFloat("MoveY", direction.y, AnimatorDampTime, Time.deltaTime);

        Vector3 scale = transform.localScale;
        if (direction.x > 0)
            scale.x = Mathf.Abs(scale.x);
        else if (direction.x < 0)
            scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    #endregion

} // class BossBehaviour
  // Alexia Perez y Marián Navarro