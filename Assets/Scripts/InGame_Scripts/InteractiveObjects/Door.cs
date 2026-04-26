//---------------------------------------------------------
// Gestiona el comportamiento de las puertas con llave del juego.
// Al colisionar con el jugador comprueba si tiene llave:
//   · Si la tiene: abre la puerta y consume la llave.
//   · Si no la tiene: muestra feedback visual indicando que falta la llave.
// Marián Navarro, lex
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Puerta que requiere una llave genérica para abrirse.
/// Al intentar pasar, el jugador recibe feedback visual a través de FeedbackUI.
///
/// Modos de apertura (configurables en Inspector):
///   · Destruir    → la puerta desaparece del mundo
///   · Desactivar  → el GameObject se desactiva (recomendado si hay animaciones)
///
/// SETUP EN INSPECTOR:
///   · Añade este script al GameObject de la puerta.
///   · El Collider2D de la puerta debe ser un collider físico (IsTrigger = false)
///     para que OnCollisionEnter2D se dispare.
///   · Asigna el SpriteRenderer si quieres efecto visual al abrir.
/// </summary>
public class Door : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Comportamiento")]
    [Tooltip("Si es true, la puerta se destruye al abrirse.\n" +
             "Si es false, el GameObject se desactiva (útil para animaciones futuras).")]
    [SerializeField] private bool DestroyOnOpen = true;

    [Header("Feedback")]
    [Tooltip("Texto principal que aparece en el panel cuando la puerta está bloqueada.")]
    [SerializeField] private string MensajeBloqueada = "Puerta bloqueada";

    [Tooltip("Texto secundario cuando está bloqueada (motivo).")]
    [SerializeField] private string SubmensajeBloqueada = "Necesitas una llave";

    [Tooltip("Texto que aparece al abrir la puerta.")]
    [SerializeField] private string MensajeAbierta = "¡Puerta abierta!";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Evita que la puerta procese más colisiones tras abrirse.</summary>
    private bool _isOpen = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isOpen) { return; }

        Inventory inventory = collision.gameObject.GetComponent<Inventory>();
        if (inventory == null) { return; }

        if (inventory.hasKey)
        {
            OpenDoor(inventory);
        }
        else
        {
            MostrarFeedbackBloqueada();
        }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Abre la puerta: consume la llave, muestra feedback y elimina/desactiva el objeto.
    /// </summary>
    private void OpenDoor(Inventory inventory)
    {
        _isOpen = true;

        // Consumir la llave del inventario
        inventory.hasKey = false;

        // Feedback visual
        if (FeedbackUI.HasInstance())
            FeedbackUI.Instance.MostrarPuerta(bloqueada: false, MensajeAbierta);

        Debug.Log("[Door] Puerta abierta.");

        if (DestroyOnOpen)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    /// <summary>
    /// Muestra el panel de puerta bloqueada. No consume ningún recurso.
    /// </summary>
    private void MostrarFeedbackBloqueada()
    {
        if (FeedbackUI.HasInstance())
            FeedbackUI.Instance.MostrarPuerta(bloqueada: true, MensajeBloqueada, SubmensajeBloqueada);

        Debug.Log("[Door] Bloqueada: el jugador no tiene llave.");
    }

    #endregion

} // class Door