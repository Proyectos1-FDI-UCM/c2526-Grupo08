//---------------------------------------------------------
// Componente que controla la cámara principal del juego en vista top-down.
// Sigue al jugador con retraso configurable y añade temblor al usar habilidades mágicas.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using System.Collections;
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
    [SerializeField] private float _followDelay = 1f;

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

    /// <summary>Indica si hay un temblor en curso para evitar solapamientos.</summary>
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
        // Guardamos la Z original para no alterarla nunca (cámara top-down 2D)
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

        // Posicionamos la cámara directamente sobre el jugador al arrancar (sin retraso inicial)
        _basePosition = new Vector3(_target.position.x, _target.position.y, _depthZ);
        transform.position = _basePosition;
    }

    /// <summary>
    /// LateUpdate se ejecuta después de todos los Update, ideal para mover la cámara
    /// una vez que el jugador ya ha actualizado su posición en ese frame.
    /// </summary>
    private void LateUpdate()
    {
        if (_target == null) return;

        // Posición objetivo: encima del jugador, manteniendo la Z de la cámara
        Vector3 desiredPosition = new Vector3(_target.position.x, _target.position.y, _depthZ);

        // Suavizamos el movimiento con SmoothDamp usando el retraso configurado
        _basePosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _smoothVelocity,
            _followDelay
        );

        // Aplicamos límites de sala si están activados
        if (_useBounds)
        {
            // El área visible es 18x10 unidades → semiancho = 9, semialto = 5
            float halfWidth = 9f;
            float halfHeight = 5f;

            _basePosition.x = Mathf.Clamp(_basePosition.x, _boundsMin.x + halfWidth, _boundsMax.x - halfWidth);
            _basePosition.y = Mathf.Clamp(_basePosition.y, _boundsMin.y + halfHeight, _boundsMax.y - halfHeight);
        }

        // Asignamos posición base (si hay temblor activo, la corrutina añade el offset encima)
        if (!_isShaking)
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
        if (_isShaking)
        {
            // Paramos la corrutina anterior para reiniciarla limpiamente
            StopCoroutine(nameof(ShakeCoroutine));
        }
        StartCoroutine(nameof(ShakeCoroutine));
    }

    #endregion


    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Corrutina que genera el efecto de temblor de cámara durante un tiempo definido.
    /// Desplaza la cámara aleatoriamente alrededor de su posición base con intensidad decreciente.
    /// </summary>
    private IEnumerator ShakeCoroutine()
    {
        _isShaking = true;
        float elapsed = 0f;

        while (elapsed < _shakeDuration)
        {
            elapsed += Time.deltaTime;

            // Calculamos el progreso del temblor (0 = inicio, 1 = fin)
            float progress = elapsed / _shakeDuration;

            // La intensidad se reduce progresivamente conforme pasa el tiempo
            float currentIntensity = _shakeIntensity * (1f - progress);

            // Offset aleatorio en X e Y
            float offsetX = Random.Range(-currentIntensity, currentIntensity);
            float offsetY = Random.Range(-currentIntensity, currentIntensity);

            // Aplicamos el offset sobre la posición base calculada en LateUpdate
            transform.position = _basePosition + new Vector3(offsetX, offsetY, 0f);

            yield return null; // Esperamos al siguiente frame
        }

        // Al terminar, dejamos la cámara exactamente en su posición base
        transform.position = _basePosition;
        _isShaking = false;
    }

    #endregion

} // class CameraController
 