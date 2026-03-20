//---------------------------------------------------------
// Componente que controla la cámara principal del juego en vista top-down.
// Sigue al jugador con retraso configurable y añade temblor al usar habilidades mágicas.
// El temblor se gestiona con un timer en LateUpdate, sin corrutinas.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Controla el comportamiento de la cámara principal del juego.
/// La cámara sigue al jugador en vista top-down con un retraso suavizado,
/// y ejecuta un efecto de temblor cuando el jugador usa una habilidad mágica.
/// El área de visión cubre 18 casillas de ancho y 10 de alto (formato 16:9),
/// siendo cada casilla una unidad de Unity.
/// </summary>
public class CameraController : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Target")]
    [Tooltip("Transform del jugador al que debe seguir la cámara.")]
    [SerializeField] private Transform _target;

    [Header("Follow Settings")]
    [Tooltip("Retraso en segundos que tarda la cámara en alcanzar la posición del jugador. (GDD: 1 segundo)")]
    [SerializeField] private float _followDelay = 0.5f;

    [Header("Screen Shake")]
    [Tooltip("Duración en segundos del temblor al usar una habilidad mágica.")]
    [SerializeField] private float _shakeDuration = 0.3f;

    [Tooltip("Intensidad máxima del desplazamiento durante el temblor (en unidades de Unity).")]
    [SerializeField] private float _shakeIntensity = 0.1f;

    [Header("Room Bounds")]
    [Tooltip("Activar si se quiere limitar el movimiento de la cámara a los bordes de la sala.")]
    [SerializeField] private bool _useBounds = false;

    [Tooltip("Límite mínimo en X e Y de la sala (esquina inferior-izquierda).")]
    [SerializeField] private Vector2 _boundsMin = new Vector2(-9f, -5f);

    [Tooltip("Límite máximo en X e Y de la sala (esquina superior-derecha).")]
    [SerializeField] private Vector2 _boundsMax = new Vector2(9f, 5f);

    #endregion


    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Velocidad de suavizado usada internamente por SmoothDamp.</summary>
    private Vector3 _smoothVelocity = Vector3.zero;

    /// <summary>Posición base de la cámara sin aplicar el offset del temblor.</summary>
    private Vector3 _basePosition;

    /// <summary>Tiempo transcurrido desde que comenzó el temblor actual.</summary>
    private float _shakeElapsed = 0f;

    /// <summary>True mientras hay un temblor activo.</summary>
    private bool _isShaking = false;

    /// <summary>Profundidad Z fija de la cámara (debe mantenerse para vista top-down 2D).</summary>
    private float _depthZ;

    #endregion


    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Awake se llama antes que Start. Se usa para guardar la profundidad Z inicial de la cámara.
    /// </summary>
    private void Awake()
    {
        _depthZ = transform.position.z;
    }

    /// <summary>
    /// Start se llama en el primer frame. Valida que el objetivo esté asignado
    /// y coloca la cámara directamente sobre el jugador al inicio sin retraso.
    /// </summary>
    private void Start()
    {
        if (_target == null)
        {
            Debug.LogError("[CameraController] No hay objetivo asignado a la cámara. " +
                           "Arrastra el GameObject del jugador al campo 'Target' en el inspector.");
            enabled = false;
            return;
        }

        _basePosition = new Vector3(_target.position.x, _target.position.y, _depthZ);
        transform.position = _basePosition;
    }

    /// <summary>
    /// LateUpdate se ejecuta después de todos los Update, ideal para mover la cámara
    /// una vez que el jugador ya ha actualizado su posición en ese frame.
    /// También gestiona el timer del efecto de temblor sin necesidad de corrutinas.
    /// </summary>
    private void LateUpdate()
    {
        if (_target == null) return;

        // --- Seguimiento suavizado ---
        Vector3 desiredPosition = new Vector3(_target.position.x, _target.position.y, _depthZ);

        _basePosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _smoothVelocity,
            _followDelay
        );

        // Aplicamos límites de sala si están activados
        if (_useBounds)
        {
            float halfWidth = 9f;
            float halfHeight = 5f;

            _basePosition.x = Mathf.Clamp(_basePosition.x, _boundsMin.x + halfWidth, _boundsMax.x - halfWidth);
            _basePosition.y = Mathf.Clamp(_basePosition.y, _boundsMin.y + halfHeight, _boundsMax.y - halfHeight);
        }

        // --- Efecto de temblor ---
        if (_isShaking)
        {
            _shakeElapsed += Time.deltaTime;

            if (_shakeElapsed < _shakeDuration)
            {
                // La intensidad se reduce progresivamente conforme avanza el temblor
                float progress = _shakeElapsed / _shakeDuration;
                float currentIntensity = _shakeIntensity * (1f - progress);

                float offsetX = Random.Range(-currentIntensity, currentIntensity);
                float offsetY = Random.Range(-currentIntensity, currentIntensity);

                transform.position = _basePosition + new Vector3(offsetX, offsetY, 0f);
            }
            else
            {
                // Temblor terminado: volvemos a la posición base y desactivamos
                transform.position = _basePosition;
                _isShaking = false;
                _shakeElapsed = 0f;
            }
        }
        else
        {
            transform.position = _basePosition;
        }
    }

    #endregion


    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Activa el efecto de temblor de cámara. Llamar desde cualquier script
    /// de habilidad mágica cuando se ejecute la habilidad.
    /// Si ya hay un temblor en curso, se reinicia desde el principio.
    /// Ejemplo de uso: FindObjectOfType&lt;CameraController&gt;().TriggerShake();
    /// </summary>
    public void TriggerShake()
    {
        _isShaking = true;
        _shakeElapsed = 0f;
    }

    #endregion

} // class CameraController