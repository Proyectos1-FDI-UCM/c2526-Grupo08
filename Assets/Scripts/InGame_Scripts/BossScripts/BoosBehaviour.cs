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
/// Coexiste con BossFisrtShoot: el dash de ese script sobreescribe
/// la velocidad temporalmente; cuando el damping la reduce, este
/// script retoma el control en el siguiente frame.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BoosBehaviour : MonoBehaviour
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

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea el Rigidbody2D antes que Start para que esté disponible
    /// desde el primer frame.
    /// </summary>
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
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
        ActualizeTimer();
        MoveTowardsObjective();
    }

    /// <summary>
    /// Dibuja el área de movimiento en el editor para facilitar el ajuste.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Usamos la posición actual en editor; en runtime usamos _centroArea
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

        // Lerp entre la velocidad actual y la deseada para suavizar
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, velocidadDeseada, SmoothSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Elige un punto aleatorio dentro del rectángulo centrado en _centroArea
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
    }

    public void BuffSpeed(float multiplicador) //Esto lo ha hecho Marián por si hay dudas
    {
        
        Speed *= multiplicador;

        SmoothSpeed *= multiplicador; //Esto para que el jefe cambie de dirección agresivamente

        Debug.Log($"<color=cyan>[Boss] Velocidad aumentada a: {Speed}</color>");
    }

    #endregion

} // class BoosBehaviour