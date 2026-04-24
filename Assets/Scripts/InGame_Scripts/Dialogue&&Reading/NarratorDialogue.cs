//---------------------------------------------------------
// Trigger narrativo de zona. Colócalo en un GameObject con Collider2D (IsTrigger).
// Al entrar el jugador reproduce una secuencia de diálogo UNA sola vez.
// Si RequireItem está configurado, solo se activa cuando el jugador
// tiene la cantidad necesaria de ese objeto (sustituye a LevelWin para
// los ascensores, unificando la lógica de requisito + cinemática + transición).
// No usa corrutinas: compatible con la restricción del proyecto.
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
/// Casos de uso configurables desde el Inspector:
///
///   A) Intro de nivel (sin requisito, sin transición)
///      · RequireItem = None, TriggerLevelTransitionOnEnd = false
///
///   B) Ascensor entre niveles (sustituye a LevelWin)
///      · RequireItem = Fusibles / Cards, RequiredAmount = 3
///      · TriggerLevelTransitionOnEnd = true, NextSceneName = "Level_2"
///      · Si el jugador no tiene los ítems, el diálogo NO se lanza
///        y en su lugar se muestra FeedbackText en la caja de diálogo.
///
///   C) Trigger narrativo sin requisito y sin transición
///      · Igual que A, PauseWhileActive = true/false según convenga.
/// </summary>
public class NarratorDialogue : MonoBehaviour
{
    // ---- ENUMS ----
    #region Enums

    public enum ItemRequirement
    {
        None,
        Fusibles,
        Cards,
        Keys,
        SpecialKey
    }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Sistema de diálogo")]
    [Tooltip("Referencia al DialogueSystem de la escena (uno por escena).")]
    [SerializeField] private DialogueSystem DialogueSystemRef;

    [Header("Contenido narrativo")]
    [Tooltip("Líneas que se muestran al activarse el trigger.")]
    [SerializeField] private List<DialogueSystem.DialogueLine> Lines = new List<DialogueSystem.DialogueLine>();

    [Header("Requisito de inventario")]
    [Tooltip("Objeto que el jugador debe tener para que el trigger se active.\n" +
             "None = sin requisito (intro de nivel, etc.).")]
    [SerializeField] private ItemRequirement RequireItem = ItemRequirement.None;

    [Tooltip("Cantidad mínima requerida. Solo se usa si RequireItem != None.")]
    [SerializeField] private int RequiredAmount = 1;

    [Tooltip("Texto que aparece en la caja de diálogo cuando el jugador NO cumple el requisito.\n" +
             "Ejemplo: 'Necesitas 3 fusibles para activar el ascensor.'")]
    [SerializeField]
    [TextArea(1, 3)]
    private string FeedbackText = "Necesitas más objetos para continuar.";

    [Tooltip("Sprite que aparece junto al FeedbackText (puede dejarse vacío).")]
    [SerializeField] private Sprite FeedbackSprite;

    [Header("Comportamiento")]
    [Tooltip("Pausa el juego (timeScale = 0) mientras el diálogo está activo.")]
    [SerializeField] private bool PauseWhileActive = true;

    [Tooltip("Después del diálogo principal, carga la siguiente escena " +
             "(llama a LevelManager.CompleteLevel o SceneManager según disponibilidad).")]
    [SerializeField] private bool TriggerLevelTransitionOnEnd = false;

    [Tooltip("Nombre exacto de la escena a cargar. Solo se usa si TriggerLevelTransitionOnEnd = true.")]
    [SerializeField] private string NextSceneName = "";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Impide que el trigger se active más de una vez.</summary>
    private bool _hasTriggered = false;

    /// <summary>
    /// True mientras se muestra el feedback de "te faltan ítems".
    /// Se usa para saber que el próximo tick de Update debe cerrar la caja
    /// sin llamar a transición ni a SetLines.
    /// </summary>
    private bool _showingFeedback = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) { return; }
        if (DialogueSystemRef == null)
        {
            Debug.LogWarning($"[NarratorDialogue] '{gameObject.name}': DialogueSystemRef no asignado.");
            return;
        }

        // --- Comprobación de requisito de inventario ---
        if (RequireItem != ItemRequirement.None)
        {
            Inventory inv = other.GetComponent<Inventory>();
            if (inv == null)
            {
                Debug.LogWarning($"[NarratorDialogue] '{gameObject.name}': el jugador no tiene Inventory.");
                return;
            }

            if (!PlayerHasRequirement(inv))
            {
                // Mostrar feedback una sola vez por entrada
                // (se puede volver a mostrar si el jugador sale y vuelve a entrar)
                ShowFeedback();
                return;
            }
        }

        // --- El jugador cumple el requisito (o no hay requisito) ---
        if (_hasTriggered) { return; }
        _hasTriggered = true;

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

    #endregion

    // ---- CALLBACKS ----
    #region Callbacks

    /// <summary>
    /// Callback cuando termina el diálogo principal.
    /// Restaura el timeScale y ejecuta la transición de nivel si está configurada.
    /// </summary>
    private void OnMainDialogueEnd()
    {
        if (PauseWhileActive) { Time.timeScale = 1f; }

        // Desactivar el collider para que no se vuelva a disparar en esta sesión
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) { col.enabled = false; }

        if (!TriggerLevelTransitionOnEnd || string.IsNullOrEmpty(NextSceneName)) { return; }

        if (LevelManager.HasInstance())
        {
            // Buscar referencias de Health e Inventory para guardar checkpoint
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Health h = player.GetComponent<Health>();
                Inventory inv = player.GetComponent<Inventory>();
                if (h != null && inv != null)
                {
                    LevelManager.Instance.CompleteLevel(NextSceneName);
                    return;
                }
            }
        }

        // Fallback sin LevelManager (ejecución directa desde editor)
        SceneManager.LoadScene(NextSceneName);
    }

    /// <summary>
    /// Callback cuando termina el feedback de "te faltan ítems".
    /// Solo restaura el timeScale; el collider sigue activo para que
    /// el jugador pueda volver a intentarlo.
    /// </summary>
    private void OnFeedbackEnd()
    {
        _showingFeedback = false;
        if (PauseWhileActive) { Time.timeScale = 1f; }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Comprueba si el jugador tiene la cantidad requerida del ítem configurado.
    /// </summary>
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

    /// <summary>
    /// Muestra una línea de feedback (sin pausar necesariamente el juego)
    /// para informar al jugador de qué le falta.
    /// </summary>
    private void ShowFeedback()
    {
        if (_showingFeedback) { return; } // ya está mostrando feedback
        _showingFeedback = true;

        var feedbackLine = new DialogueSystem.DialogueLine
        {
            SpeakerName = "",
            CharacterSprite = FeedbackSprite,
            Text = FeedbackText
        };

        DialogueSystemRef.SetLines(new List<DialogueSystem.DialogueLine> { feedbackLine });

        // El feedback siempre pausa para que el jugador lo lea con calma
        Time.timeScale = 0f;
        DialogueSystemRef.StartDialogue(OnFeedbackEnd);
    }

    #endregion

    // ---- GIZMO ----
    #region Gizmo de editor

    private void OnDrawGizmos()
    {
        // Verde si no tiene requisito, amarillo si tiene requisito de ítem
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