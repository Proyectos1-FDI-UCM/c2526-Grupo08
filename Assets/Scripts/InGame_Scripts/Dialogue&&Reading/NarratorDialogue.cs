//---------------------------------------------------------
// Trigger narrativo de zona.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trigger narrativo reutilizable.
///
/// Modos de activación:
///   · AutoStartOnLoad = true  → arranca automáticamente al iniciar la escena.
///   · AutoStartOnLoad = false → se activa cuando el jugador entra en el collider.
///
/// CORRECCIONES:
///   · Usa DialogueSystem.Instance si DialogueSystemRef no está asignado.
///   · Timer con unscaledDeltaTime.
///   · AutoStartDelay por defecto 1 s.
///   · IMPORTANTE: si el DialogueSystem no tiene el DialogueBox asignado,
///     el juego se quedaba congelado con timeScale=0. Ahora verificamos
///     que el sistema está listo antes de llamar StartDialogue().
/// </summary>
public class NarratorDialogue : MonoBehaviour
{
    // ---- ENUMS ----
    public enum ItemRequirement { None, Fusibles, Cards, Keys, SpecialKey }

    // ---- INSPECTOR ----
    #region Atributos del Inspector

    [Header("Sistema de diálogo")]
    [Tooltip("Opcional. Si se deja vacío se usa DialogueSystem.Instance automáticamente.")]
    [SerializeField] private DialogueSystem DialogueSystemRef;

    [Header("Activación")]
    [SerializeField] private bool AutoStartOnLoad = false;

    [Tooltip("Espera en segundos antes de arrancar (unscaled). 1 s recomendado al cargar desde menú.")]
    [SerializeField] private float AutoStartDelay = 1f;

    [Header("Contenido narrativo")]
    [SerializeField] private List<DialogueSystem.DialogueLine> Lines = new List<DialogueSystem.DialogueLine>();

    [Header("Requisito de inventario (solo modo collider)")]
    [SerializeField] private ItemRequirement RequireItem = ItemRequirement.None;
    [SerializeField] private int RequiredAmount = 1;
    [SerializeField][TextArea(1, 3)] private string FeedbackText = "Necesitas más objetos para continuar.";
    [SerializeField] private Sprite FeedbackSprite;

    [Header("Comportamiento")]
    [SerializeField] private bool PauseWhileActive = true;
    [SerializeField] private bool TriggerLevelTransitionOnEnd = false;
    [SerializeField] private string NextSceneName = "";

    #endregion

    // ---- PRIVADOS ----
    #region Atributos Privados

    private bool _hasTriggered = false;
    private bool _showingFeedback = false;
    private float _autoStartTimer = 0f;
    private bool _waitingAutoStart = false;

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour

    private void Start()
    {
        if (!AutoStartOnLoad) { return; }

        // Resolver referencia
        if (DialogueSystemRef == null)
            DialogueSystemRef = DialogueSystem.Instance;

        if (DialogueSystemRef == null)
        {
            Debug.LogError($"[NarratorDialogue] '{gameObject.name}': no se encontró DialogueSystem en escena. " +
                           "Añade un GameObject con el componente DialogueSystem.");
            return;
        }

        if (Lines == null || Lines.Count == 0)
        {
            Debug.LogError($"[NarratorDialogue] '{gameObject.name}': Lines está vacío. " +
                           "Rellena las líneas en el Inspector.");
            return;
        }

        _waitingAutoStart = true;
        _autoStartTimer = AutoStartDelay;
        Debug.Log($"[NarratorDialogue] '{gameObject.name}': AutoStart en {AutoStartDelay}s.");
    }

    private void Update()
    {
        if (!_waitingAutoStart) { return; }

        _autoStartTimer -= Time.unscaledDeltaTime;
        if (_autoStartTimer <= 0f)
        {
            _waitingAutoStart = false;
            LanzarDialogo();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (AutoStartOnLoad) { return; }
        if (_hasTriggered) { return; }
        if (!other.CompareTag("Player")) { return; }

        if (DialogueSystemRef == null)
            DialogueSystemRef = DialogueSystem.Instance;

        if (DialogueSystemRef == null)
        {
            Debug.LogError($"[NarratorDialogue] '{gameObject.name}': DialogueSystem no encontrado.");
            return;
        }

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

        // Resolver referencia de nuevo por si Awake aún no había corrido
        if (DialogueSystemRef == null)
            DialogueSystemRef = DialogueSystem.Instance;

        if (DialogueSystemRef == null)
        {
            Debug.LogError($"[NarratorDialogue] '{gameObject.name}': DialogueSystem es null al lanzar.");
            return;
        }

        if (Lines == null || Lines.Count == 0)
        {
            Debug.LogWarning($"[NarratorDialogue] '{gameObject.name}': sin líneas. Saltando diálogo.");
            OnMainDialogueEnd();
            return;
        }

        _hasTriggered = true;

        DialogueSystemRef.SetLines(Lines);

        // Pausar DESPUÉS de verificar que el sistema está listo
        // para no congelar el juego si algo falla
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

        var line = new DialogueSystem.DialogueLine
        {
            SpeakerName = "",
            CharacterSprite = FeedbackSprite,
            Text = FeedbackText
        };

        DialogueSystemRef.SetLines(new List<DialogueSystem.DialogueLine> { line });
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