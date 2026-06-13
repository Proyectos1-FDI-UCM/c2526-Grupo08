//---------------------------------------------------------
// Script que controla el brazo del personaje: sigue su posición,
// rota hacia el cursor y ajusta su visibilidad y orden de dibujado
// según el estado de animación del jugador.
// Adriana Fernández Luna
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el brazo (arma) del jugador: copia el color del sprite
/// del jugador, posiciona el brazo según la dirección de movimiento,
/// lo rota para que apunte hacia el cursor y ajusta su sorting order
/// y visibilidad según el estado del Animator del jugador (dash, pickup).
/// </summary>
public class ArmPivot : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Tooltip("SpriteRenderer del brazo, cuyo color y visibilidad se sincronizan con el del jugador.")]
    [SerializeField]
    private SpriteRenderer _armRenderer;

    [Tooltip("SpriteRenderer del jugador, usado como referencia de color y sorting order.")]
    [SerializeField]
    private SpriteRenderer _playerRenderer;

    [Tooltip("Posición del brazo cuando el jugador mira hacia arriba.")]
    [SerializeField]
    private Transform _pivotUp;

    [Tooltip("Posición del brazo cuando el jugador mira hacia abajo.")]
    [SerializeField]
    private Transform _pivotDown;

    [Tooltip("Posición del brazo cuando el jugador está quieto mirando hacia un lado.")]
    [SerializeField]
    private Transform _pivotRight;

    [Tooltip("Posición del brazo cuando el jugador está caminando hacia un lado.")]
    [SerializeField]
    private Transform _pivotRightWalk;

    [Tooltip("Umbral de Speed del Animator por encima del cual se considera que el jugador está caminando " +
             "(usa _pivotRightWalk en vez de _pivotRight).")]
    [SerializeField]
    private float WalkSpeedThreshold = 0.1f;

    [Tooltip("Umbral del valor MoveY del Animator para considerar que el jugador mira arriba/abajo.")]
    [SerializeField]
    private float VerticalAimThreshold = 0.9f;

    [Tooltip("Umbral del valor MoveY del Animator para decidir si el brazo se dibuja por delante o por detrás del jugador.")]
    [SerializeField]
    private float SortingOrderAimThreshold = 0.5f;

    [Tooltip("Ángulo en grados que se suma a la rotación calculada hacia el cursor, " +
             "para compensar la orientación del sprite del brazo.")]
    [SerializeField]
    private float ArmAngleOffset = 90f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Animator del jugador (objeto padre), del que se leen los parámetros de movimiento y estado.</summary>
    private Animator _playerAnimator;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea el Animator del jugador, que es el GameObject padre de este brazo.
    /// </summary>
    private void Awake()
    {
        _playerAnimator = transform.parent.GetComponent<Animator>();
    }

    /// <summary>
    /// Cada frame, sincroniza el color del brazo con el del jugador, lo oculta
    /// durante el dash o el pickup, y en caso contrario lo posiciona según la
    /// dirección de movimiento, lo rota hacia el cursor y ajusta su sorting order.
    /// </summary>
    void Update()
    {
        // Cambio de color con el personaje
        _armRenderer.color = _playerRenderer.color;

        // Cambio de posición dependiendo de la dirección del personaje
        float moveX = _playerAnimator.GetFloat("MoveX");
        float moveY = _playerAnimator.GetFloat("MoveY");
        float speed = _playerAnimator.GetFloat("Speed");
        bool isDashing = _playerAnimator.GetBool("IsDashing");
        bool isPickingUp = _playerAnimator.GetBool("IsPickingUp");

        if (isDashing || isPickingUp)
        {
            _armRenderer.enabled = false;
        }
        else
        {
            _armRenderer.enabled = true;

            if (moveY > VerticalAimThreshold)
                transform.position = _pivotUp.position;
            else if (moveY < -VerticalAimThreshold)
                transform.position = _pivotDown.position;
            else if (speed > WalkSpeedThreshold)
                transform.position = _pivotRightWalk.position;
            else
                transform.position = _pivotRight.position;

            // Dirección hacia el cursor
            Vector3 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            Vector2 dir = worldPos - transform.position;

            // Rotación del pivote
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + ArmAngleOffset;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Sorting order: por debajo cuando apunta arriba, por encima el resto
            if (moveY > SortingOrderAimThreshold)
                _armRenderer.sortingOrder = _playerRenderer.sortingOrder - 1;
            else
                _armRenderer.sortingOrder = _playerRenderer.sortingOrder + 1;
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Esta clase no expone métodos públicos.
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados
    // Esta clase no tiene métodos privados adicionales.
    #endregion

} // class ArmPivot
  // Adriana Fernández Luna