//---------------------------------------------------------
// Gestiona el comportamiento de las puertas con llave del juego.
// Al colisionar con el jugador comprueba si tiene llave:
//   · Si la tiene: abre la puerta y consume la llave.
//   · Si no la tiene: muestra feedback visual indicando que falta la llave.
// Marián Navarro, Alexia
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Puerta que requiere una llave genérica para abrirse.
/// Al intentar pasar, el jugador recibe feedback visual a través de FeedbackUI.
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

    [Header("Feedback — Texto bloqueada")]
    [Tooltip("Texto que aparece en el diálogo cuando el jugador no tiene llave.")]
    [SerializeField] private string MensajeBloqueada = "Necesitas una llave para abrir esta puerta.";

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoAbrir;
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
        inventory.hasKey = false;

        if (sonidoAbrir != null)
        {
            AudioSource.PlayClipAtPoint(sonidoAbrir, transform.position);
        }

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
        if (!DialogueSystem.HasInstance()) { return; }
        if (DialogueSystem.Instance.IsActive()) { return; }

        var linea = new System.Collections.Generic.List<DialogueSystem.DialogueLine>
        {
            new DialogueSystem.DialogueLine
            {
                SpeakerName = "",
                CharacterSprite = null,
                Text = MensajeBloqueada
            }
        };

        DialogueSystem.Instance.SetLines(linea);
        Time.timeScale = 0f;
        DialogueSystem.Instance.StartDialogue(() => Time.timeScale = 1f);

        Debug.Log("[Door] Bloqueada: el jugador no tiene llave.");
    }

    #endregion

} // class Door