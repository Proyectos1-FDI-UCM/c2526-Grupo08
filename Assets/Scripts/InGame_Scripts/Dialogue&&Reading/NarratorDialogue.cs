//---------------------------------------------------------
// Trigger narrativo de zona. Colócalo en un GameObject con Collider2D (IsTrigger)
// para activarlo al entrar el jugador, o marca AutoStartOnLoad para que arranque
// automáticamente al inicio de la escena (intro de nivel).
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trigger narrativo de zona reutilizable.
///
/// Modos de activación:
///   · AutoStartOnLoad = true  → arranca solo al iniciar la escena (intro de nivel).
///                               No necesita collider.
///   · AutoStartOnLoad = false → se activa cuando el jugador entra en el collider.
///
/// CORRECCIONES respecto a la versión anterior:
///   · AutoStartDelay por defecto sube a 0.5 s (el valor anterior de 0.1 s era
///     insuficiente en escenas con muchos objetos y daba lugar a que el
///     DialogueSystem no estuviera listo cuando se llamaba StartDialogue()).
///   · Se añade un log de error claro cuando DialogueSystemRef no está asignado
///     en modo AutoStart, para facilitar el diagnóstico en consola.
///   · OnTriggerEnter2D comprueba _hasTriggered ANTES del requisito de inventario
///     para no mostrar el feedback de "te falta X" más de una vez si ya se lanzó.
/// </summary>
public class NarratorDialogue : MonoBehaviour
{
    // ---- ENUMS ----
    #region Enums

    public enum ItemRequirement { None, Fusibles, Cards, Keys, SpecialKey }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Sistema de diálogo")]
    [Tooltip("DialogueSystem de la escena (uno por escena). OBLIGATORIO.")]
    [SerializeField] private DialogueSystem DialogueSystemRef;

    [Header("Activación")]
    [Tooltip("Si es true, el diálogo arranca automáticamente al iniciar la escena.\n" +
             "Úsalo para las intros de nivel. No necesita Collider2D.\n" +
             "Si es false, se activa cuando el jugador entra en el collider.")]
    [SerializeField] private bool AutoStartOnLoad = false;

    [Tooltip("Segundos de espera antes de arrancar el diálogo automático.\n" +
             "0.5 s garantiza que el DialogueSystem ya esté inicializado.")]
    [SerializeField] private float AutoStartDelay = 0.5f;

    [Header("Contenido narrativo")]
    [Tooltip("Líneas del diálogo. Rellena en el Inspector usando StoryDialogues.cs como referencia.")]
    [SerializeField] private List<DialogueSystem.DialogueLine> Lines = new List<DialogueSystem.DialogueLine>();

    [Header("Requisito de inventario (solo modo collider)")]
    [Tooltip("Objeto que el jugador debe tener para activar el trigger. None = sin requisito.")]
    [SerializeField] private ItemRequirement RequireItem = ItemRequirement.None;

    [Tooltip("Cantidad mínima requerida.")]
    [SerializeField] private int RequiredAmount = 1;

    [Tooltip("Texto cuando el jugador NO tiene los ítems.")]
    [SerializeField]
    [TextArea(1, 3)]
    private string FeedbackText = "Necesitas más objetos para continuar.";

    [Tooltip("Sprite junto al FeedbackText (puede dejarse vacío).")]
    [SerializeField] private Sprite FeedbackSprite;

    [Header("Comportamiento")]
    [Tooltip("Pausa el juego durante el diálogo.")]
    [SerializeField] private bool PauseWhileActive = true;

    [Tooltip("Al terminar el diálogo, carga la siguiente escena.")]
    [SerializeField] private bool TriggerLevelTransitionOnEnd = false;

    [Tooltip("Nombre de la escena a cargar. Solo si TriggerLevelTransitionOnEnd = true.")]
    [SerializeField] private string NextSceneName = "";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    private bool _hasTriggered = false;
    private bool _showingFeedback = false;
    private float _autoStartTimer = 0f;
    private bool _waitingAutoStart = false;

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        if (AutoStartOnLoad)
        {
            // Validación temprana: avisa en consola si falta la referencia
            if (DialogueSystemRef == null)
            {
                Debug.LogError($"[NarratorDialogue] '{gameObject.name}': AutoStartOnLoad=true pero " +
                               "DialogueSystemRef NO está asignado en el Inspector. " +
                               "Arrastra el GameObject que tiene DialogueSystem al campo correspondiente.");
                return;
            }

            if (Lines == null || Lines.Count == 0)
            {
                Debug.LogError($"[NarratorDialogue] '{gameObject.name}': AutoStartOnLoad=true pero " +
                               "el array Lines está vacío. Rellena las líneas en el Inspector.");
                return;
            }

            _waitingAutoStart = true;
            _autoStartTimer = AutoStartDelay;
        }
    }

    private void Update()
    {
        if (!_waitingAutoStart) { return; }

        _autoStartTimer -= Time.deltaTime;
        if (_autoStartTimer <= 0f)
        {
            _waitingAutoStart = false;
            LanzarDialogo();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (AutoStartOnLoad) { return; }  // modo auto: ignorar triggers de collider
        if (_hasTriggered) { return; }  // ya se activó, no repetir
        if (!other.CompareTag("Player")) { return; }

        if (DialogueSystemRef == null)
        {
            Debug.LogError($"[NarratorDialogue] '{gameObject.name}': DialogueSystemRef no asignado.");
            return;
        }

        // Comprobar requisito de inventario
        if (RequireItem != ItemRequirement.None)
        {
            Inventory inv = other.GetComponent<Inventory>();
            if (inv == null) { return; }

            if (!PlayerHasRequirement(inv))
            {
                ShowFeedback();
                return;
            }
        }

        LanzarDialogo();
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void LanzarDialogo()
    {
        if (_hasTriggered) { return; }
        _hasTriggered = true;

        if (DialogueSystemRef == null)
        {
            Debug.LogError($"[NarratorDialogue] '{gameObject.name}': DialogueSystemRef no asignado.");
            OnMainDialogueEnd();
            return;
        }

        if (Lines == null || Lines.Count == 0)
        {
            Debug.LogWarning($"[NarratorDialogue] '{gameObject.name}': sin líneas configuradas.");
            OnMainDialogueEnd();
            return;
        }

        DialogueSystemRef.SetLines(Lines);
        if (PauseWhileActive) { Time.timeScale = 0f; }
        DialogueSystemRef.StartDialogue(OnMainDialogueEnd);
    }

    private void OnMainDialogueEnd()
    {
        if (PauseWhileActive) { Time.timeScale = 1f; }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) { col.enabled = false; }

        if (!TriggerLevelTransitionOnEnd || string.IsNullOrEmpty(NextSceneName)) { return; }

        if (LevelManager.HasInstance())
            LevelManager.Instance.CompleteLevel(NextSceneName);
        else
            SceneManager.LoadScene(NextSceneName);
    }

    private void OnFeedbackEnd()
    {
        _showingFeedback = false;
        if (PauseWhileActive) { Time.timeScale = 1f; }
    }

    private bool PlayerHasRequirement(Inventory inv)
    {
        return RequireItem switch
        {
            ItemRequirement.Fusibles => inv.GetFusibleCount() >= RequiredAmount,
            ItemRequirement.Cards => inv.GetCardCount() >= RequiredAmount,
            ItemRequirement.Keys => inv.GetKeyCount() >= RequiredAmount,
            ItemRequirement.SpecialKey => inv.hasSpecialKey,
            _ => true
        };
    }

    private void ShowFeedback()
    {
        if (_showingFeedback) { return; }
        _showingFeedback = true;

        var feedbackLine = new DialogueSystem.DialogueLine
        {
            SpeakerName = "",
            CharacterSprite = FeedbackSprite,
            Text = FeedbackText
        };

        DialogueSystemRef.SetLines(new List<DialogueSystem.DialogueLine> { feedbackLine });
        Time.timeScale = 0f;
        DialogueSystemRef.StartDialogue(OnFeedbackEnd);
    }

    #endregion

    // ---- GIZMO ----
    #region Gizmo

    private void OnDrawGizmos()
    {
        if (AutoStartOnLoad)
        {
            Gizmos.color = new Color(0.9f, 0.2f, 0.9f, 0.6f);
            Gizmos.DrawSphere(transform.position, 0.3f);
            return;
        }

        Gizmos.color = RequireItem == ItemRequirement.None
            ? new Color(0.3f, 0.9f, 0.8f, 0.3f)
            : new Color(1f, 0.85f, 0.1f, 0.3f);

        Collider2D col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            Gizmos.DrawCube(
                transform.position + (Vector3)box.offset,
                new Vector3(box.size.x * transform.localScale.x,
                            box.size.y * transform.localScale.y, 0.1f));
        }
        else
        {
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }

    #endregion

} // class NarratorDialogue