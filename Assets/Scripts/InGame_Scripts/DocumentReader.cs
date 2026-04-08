//---------------------------------------------------------
// Permite al jugador leer documentos de lore en la escena.
// Al interactuar: pausa el juego, muestra un canvas centrado con el texto
// y el jugador avanza páginas con F / B (Interact). Al terminar, cierra
// el canvas y reanuda el juego. El documento permanece en el suelo y
// puede relerse en cualquier momento.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Componente que se coloca en el GameObject del documento de lore.
/// Gestiona la interacción (prompt de proximidad), la paginación del texto
/// y la pausa del juego mientras se lee.
///
/// Estructura de UI esperada (asignar en Inspector):
///   DocumentCanvas     → Canvas en modo Screen Space - Overlay, oculto por defecto
///   PageText           → TMP_Text con el contenido de la página actual
///   PageIndicator      → TMP_Text con "1 / N" (opcional)
///   ContinueHint       → TMP_Text con el hint de tecla (opcional)
///
/// El array Pages se rellena en el Inspector con el texto del lore dividido en páginas.
/// Cada elemento del array es una página.
///
/// No se usa corrutina: el cooldown de input usa Time.unscaledDeltaTime
/// para funcionar con Time.timeScale = 0.
/// </summary>
public class DocumentReader : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Detección de proximidad")]
    [Tooltip("Radio desde el que el jugador puede interactuar con el documento")]
    [SerializeField] private float InteractionRadius = 1.2f;

    [Header("UI - Prompt de interacción")]
    [Tooltip("GameObject del prompt 'Pulsa F para leer' que aparece encima del documento")]
    [SerializeField] private GameObject PromptUI;

    [Header("UI - Canvas del documento")]
    [Tooltip("Canvas completo del lector de documentos (Screen Space - Overlay). Oculto por defecto.")]
    [SerializeField] private GameObject DocumentCanvas;

    [Tooltip("TMP_Text donde se muestra el texto de la página actual")]
    [SerializeField] private TMP_Text PageText;

    [Tooltip("TMP_Text que muestra '1 / N' (puede dejarse vacío)")]
    [SerializeField] private TMP_Text PageIndicator;

    [Tooltip("TMP_Text con el hint de tecla para continuar (puede dejarse vacío)")]
    [SerializeField] private TMP_Text ContinueHint;

    [Tooltip("Texto del hint de tecla")]
    [SerializeField] private string HintText = "F  /  B (mando)  →  siguiente página";

    [Tooltip("Texto del hint en la última página")]
    [SerializeField] private string HintTextLastPage = "F  /  B (mando)  →  cerrar";

    [Header("Contenido del documento")]
    [Tooltip("Texto de cada página del documento. Cada elemento del array es una página.")]
    [TextArea(4, 10)]
    [SerializeField] private string[] Pages;

    [Header("Componente de movimiento del jugador")]
    [Tooltip("Referencia al PlayerMovement para desactivarlo mientras se lee. " +
             "Si está vacío se busca automáticamente con la tag 'Player'.")]
    [SerializeField] private PlayerMovement PlayerMovementRef;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>True mientras el jugador está dentro del radio de interacción.</summary>
    private bool _playerInRange = false;

    /// <summary>True mientras el canvas del documento está abierto.</summary>
    private bool _isReading = false;

    /// <summary>Índice de la página que se muestra actualmente.</summary>
    private int _currentPage = 0;

    private InputAction _interactAction;
    private Transform _playerTransform;

    /// <summary>
    /// Cooldown de input para evitar que el mismo frame que abre el documento
    /// también avance la primera página. Usa tiempo sin escalar.
    /// </summary>
    private float _inputCooldown = 0f;
    private const float INPUT_COOLDOWN_DURATION = 0.25f;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Awake()
    {
        // Buscar al jugador automáticamente si no se asignó en Inspector
        if (PlayerMovementRef == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
                PlayerMovementRef = player.GetComponent<PlayerMovement>();
            }
            else
                Debug.LogWarning("[DocumentReader] No se encontró jugador con tag 'Player'.");
        }
        else
        {
            _playerTransform = PlayerMovementRef.transform;
        }

        _interactAction = InputSystem.actions.FindAction("Interact");
        if (_interactAction == null)
            Debug.LogWarning("[DocumentReader] Acción 'Interact' no encontrada en el InputSystem.");
    }

    private void Start()
    {
        if (_interactAction != null) _interactAction.Enable();
        if (DocumentCanvas != null) DocumentCanvas.SetActive(false);
        if (PromptUI != null) PromptUI.SetActive(false);
    }

    private void Update()
    {
        // Cooldown de input (funciona con timeScale = 0 gracias a unscaledDeltaTime)
        if (_inputCooldown > 0f)
        {
            _inputCooldown -= Time.unscaledDeltaTime;
            return;
        }

        if (_isReading)
        {
            // Mientras se lee, solo procesamos el avance de página
            if (_interactAction != null && _interactAction.WasPressedThisFrame())
                AvanzarPagina();
        }
        else
        {
            // Fuera de lectura: detectar proximidad y abrir el documento
            ActualizarProximidad();

            if (_playerInRange && _interactAction != null && _interactAction.WasPressedThisFrame())
                AbrirDocumento();
        }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>Calcula si el jugador está en rango y actualiza el prompt.</summary>
    private void ActualizarProximidad()
    {
        if (_playerTransform == null) return;

        bool enRango = Vector2.Distance(transform.position, _playerTransform.position) <= InteractionRadius;

        if (enRango != _playerInRange)
        {
            _playerInRange = enRango;
            if (PromptUI != null) PromptUI.SetActive(_playerInRange);
        }
    }

    /// <summary>
    /// Abre el canvas del documento, pausa el juego y desactiva el movimiento del jugador.
    /// </summary>
    private void AbrirDocumento()
    {
        if (Pages == null || Pages.Length == 0)
        {
            Debug.LogWarning("[DocumentReader] No hay páginas configuradas en el Inspector.");
            return;
        }

        _isReading = true;
        _currentPage = 0;

        // Pausa y bloqueo de movimiento
        Time.timeScale = 0f;
        if (PlayerMovementRef != null) PlayerMovementRef.enabled = false;

        // Mostrar canvas y ocultar prompt
        if (PromptUI != null) PromptUI.SetActive(false);
        if (DocumentCanvas != null) DocumentCanvas.SetActive(true);

        _inputCooldown = INPUT_COOLDOWN_DURATION;
        MostrarPaginaActual();
    }

    /// <summary>Avanza a la siguiente página o cierra si era la última.</summary>
    private void AvanzarPagina()
    {
        _currentPage++;
        _inputCooldown = INPUT_COOLDOWN_DURATION;

        if (_currentPage >= Pages.Length)
            CerrarDocumento();
        else
            MostrarPaginaActual();
    }

    /// <summary>Actualiza los textos de la UI con el contenido de la página actual.</summary>
    private void MostrarPaginaActual()
    {
        if (PageText != null)
            PageText.text = Pages[_currentPage];

        if (PageIndicator != null)
            PageIndicator.text = $"{_currentPage + 1} / {Pages.Length}";

        if (ContinueHint != null)
        {
            bool esUltimaPagina = (_currentPage >= Pages.Length - 1);
            ContinueHint.text = esUltimaPagina ? HintTextLastPage : HintText;
        }
    }

    /// <summary>
    /// Cierra el canvas, reanuda el tiempo y reactiva el movimiento del jugador.
    /// El documento sigue en el suelo para poder releerlo.
    /// </summary>
    private void CerrarDocumento()
    {
        _isReading = false;

        Time.timeScale = 1f;
        if (PlayerMovementRef != null) PlayerMovementRef.enabled = true;

        if (DocumentCanvas != null) DocumentCanvas.SetActive(false);

        // Recalcular proximidad para mostrar el prompt si el jugador sigue cerca
        ActualizarProximidad();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, InteractionRadius);
    }

    #endregion

} // class DocumentReader