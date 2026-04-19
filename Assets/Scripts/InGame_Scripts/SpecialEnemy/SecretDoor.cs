//---------------------------------------------------------
// Puerta que solo se abre con la llave especial del enemigo especial.
// A diferencia de Door.cs (llaves genéricas), esta comprueba
// inventory.hasSpecialKey. Se coloca en la habitación secreta de la planta 2.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Puerta que requiere la llave especial (hasSpecialKey en Inventory).
/// Solo existe una en toda la partida: la habitación secreta de la planta 2,
/// donde está la bala explosiva y el documento de lore.
///
/// Al abrirse, el GameObject de la puerta se destruye.
/// Se puede cambiar por una animación si se añade un Animator.
/// </summary>
public class SecretDoor : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Feedback")]
    [Tooltip("Mensaje en consola (o UI futura) cuando el jugador no tiene la llave especial")]
    [SerializeField] private string LockedMessage = "Necesitas la llave especial para abrir esta puerta.";

    [Tooltip("Si está activado, muestra el mensaje en pantalla a través de un TMP_Text de la escena (opcional)")]
    [SerializeField] private TMPro.TMP_Text LockedFeedbackText;

    [Tooltip("Tiempo en segundos que el mensaje de puerta cerrada permanece visible")]
    [SerializeField] private float FeedbackDuration = 2f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Acumulador para ocultar el texto de feedback tras FeedbackDuration segundos.</summary>
    private float _feedbackTimer = 0f;

    /// <summary>True mientras el texto de feedback está visible.</summary>
    private bool _showingFeedback = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Update()
    {
        // Ocultar el texto de feedback cuando expire el timer
        if (_showingFeedback)
        {
            _feedbackTimer -= Time.deltaTime;
            if (_feedbackTimer <= 0f)
            {
                _showingFeedback = false;
                if (LockedFeedbackText != null) LockedFeedbackText.gameObject.SetActive(false);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Inventory inventory = collision.gameObject.GetComponent<Inventory>();
        if (inventory == null) return;

        if (inventory.hasSpecialKey)
        {
            OpenDoor();
        }
        else
        {
            Debug.Log($"[SecretDoor] {LockedMessage}");
            MostrarFeedback();
        }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>Destruye la puerta. Aquí se puede añadir animación en el futuro.</summary>
    private void OpenDoor()
    {
        Debug.Log("[SecretDoor] Puerta secreta abierta.");
        Destroy(gameObject);
    }

    /// <summary>Muestra el texto de feedback durante FeedbackDuration segundos.</summary>
    private void MostrarFeedback()
    {
        if (LockedFeedbackText == null) return;

        LockedFeedbackText.text = LockedMessage;
        LockedFeedbackText.gameObject.SetActive(true);
        _feedbackTimer = FeedbackDuration;
        _showingFeedback = true;
    }

    #endregion

} // class SecretDoor