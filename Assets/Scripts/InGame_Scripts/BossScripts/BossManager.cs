//---------------------------------------------------------
// Coordina los diálogos narrativos del combate final (Raven).
// Se registra como punto de intercepción entre Health y LevelManager:
// cuando el boss muere, primero reproduce el final bueno;
// cuando el jugador muere en esta escena, primero reproduce el final malo.
// En ambos casos, al terminar el diálogo llama a LevelManager.
// No usa corrutinas: compatible con la restricción del proyecto.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton de la escena del boss.
///
/// Responsabilidades:
///   · Reproducir el diálogo del final bueno cuando el boss llega a 0 HP.
///   · Reproducir el diálogo del final malo cuando el jugador muere en esta escena.
///   · Llamar a LevelManager.OnBossDeath() / OnPlayerDeath() al terminar cada diálogo.
///   · Desactivar BossPhaseController durante los diálogos para que el boss no siga atacando.
///
/// Requiere:
///   · Un DialogueSystem en la escena (asignar en Inspector).
///   · BossPhaseController en el mismo GameObject del boss (se busca automáticamente).
///   · LevelManager en la escena (singleton local).
///
/// IMPORTANTE — pequeño cambio en Health.cs:
///   En el bloque 'else if (IsBoss)' de Die(), sustituir la llamada directa
///   a LevelManager.Instance.OnBossDeath() por:
///       if (BossManager.HasInstance())
///           BossManager.Instance.OnBossDeath();
///       else if (LevelManager.HasInstance())
///           LevelManager.Instance.OnBossDeath();
///   Así el diálogo se reproduce antes de mostrar el panel de victoria.
///   (Ver región "CAMBIO EN Health.cs" al final del archivo para el bloque exacto.)
/// </summary>
public class BossManager : MonoBehaviour
{
    // ---- SINGLETON LOCAL ----
    #region Singleton local de escena

    private static BossManager _instance;

    /// <summary>Acceso global al BossManager de la escena activa.</summary>
    public static BossManager Instance
    {
        get
        {
            Debug.Assert(_instance != null, "[BossManager] No hay instancia en esta escena.");
            return _instance;
        }
    }

    /// <summary>True si hay un BossManager activo en la escena.</summary>
    public static bool HasInstance() => _instance != null;

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.LogWarning("[BossManager] Duplicado detectado. Solo debe haber uno por escena.");
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (this == _instance) { _instance = null; }
    }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Sistema de diálogo")]
    [Tooltip("DialogueSystem de la escena del boss.")]
    [SerializeField] private DialogueSystem DialogueSystemRef;

    [Header("Final bueno — Cori derrota a Raven")]
    [Tooltip("Líneas del diálogo que aparecen cuando el boss llega a 0 HP.\n" +
             "Ver StoryDialogues.cs → [LEVEL_BOSS] FINAL BUENO para los textos.")]
    [SerializeField]
    private List<DialogueSystem.DialogueLine> GoodEndingLines
        = new List<DialogueSystem.DialogueLine>();

    [Header("Final malo — Cori muere en el combate")]
    [Tooltip("Líneas del diálogo que aparecen cuando el jugador muere en esta escena.\n" +
             "Ver StoryDialogues.cs → [LEVEL_BOSS] FINAL MALO para los textos.")]
    [SerializeField]
    private List<DialogueSystem.DialogueLine> BadEndingLines
        = new List<DialogueSystem.DialogueLine>();

    [Header("Referencias")]
    [Tooltip("GameObject del boss (Raven). Se usa para desactivar BossPhaseController " +
             "durante los diálogos finales.")]
    [SerializeField] private GameObject BossGameObject;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Evita que el final se dispare dos veces (boss + jugador a la vez).</summary>
    private bool _endingTriggered = false;

    #endregion

    // ---- MÉTODOS PÚBLICOS — llamados desde Health.cs ----
    #region Métodos públicos

    /// <summary>
    /// Llamado desde Health.Die() cuando IsBoss = true y vida = 0.
    /// Pausa el combate, reproduce el final bueno y al terminar
    /// llama a LevelManager.OnBossDeath().
    /// </summary>
    public void OnBossDeath()
    {
        if (_endingTriggered) { return; }
        _endingTriggered = true;

        // Detener el boss
        StopBoss();

        // Pausar el juego durante el diálogo
        Time.timeScale = 0f;

        PlayDialogue(GoodEndingLines, () =>
        {
            Time.timeScale = 1f;
            if (LevelManager.HasInstance())
                LevelManager.Instance.OnBossDeath();
        });
    }

    /// <summary>
    /// Llamado desde Health.Die() cuando IsPlayer = true y la escena activa es el boss.
    /// Reproduce el final malo y al terminar llama a LevelManager.OnPlayerDeath().
    /// </summary>
    public void OnPlayerDeath()
    {
        if (_endingTriggered) { return; }
        _endingTriggered = true;

        // Detener el boss para que no siga atacando mientras pasa la cinemática
        StopBoss();

        Time.timeScale = 0f;

        PlayDialogue(BadEndingLines, () =>
        {
            // El panel de muerte lo muestra LevelManager; Time.timeScale lo gestiona él
            if (LevelManager.HasInstance())
                LevelManager.Instance.OnPlayerDeath();
        });
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Arranca el DialogueSystem con las líneas indicadas.
    /// Si no hay líneas configuradas, llama directamente al callback.
    /// </summary>
    private void PlayDialogue(List<DialogueSystem.DialogueLine> lines, System.Action onEnd)
    {
        if (DialogueSystemRef == null)
        {
            Debug.LogWarning("[BossManager] DialogueSystemRef no asignado. Saltando diálogo.");
            Time.timeScale = 1f;
            onEnd?.Invoke();
            return;
        }

        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("[BossManager] Lista de líneas vacía. Saltando diálogo.");
            Time.timeScale = 1f;
            onEnd?.Invoke();
            return;
        }

        DialogueSystemRef.SetLines(lines);
        DialogueSystemRef.StartDialogue(onEnd);
    }

    /// <summary>
    /// Desactiva BossPhaseController para que el boss deje de atacar
    /// durante la cinemática final.
    /// </summary>
    private void StopBoss()
    {
        if (BossGameObject == null) { return; }

        BossPhaseController phase = BossGameObject.GetComponent<BossPhaseController>();
        if (phase != null) { phase.enabled = false; }

        BoosBehaviour movement = BossGameObject.GetComponent<BoosBehaviour>();
        if (movement != null) { movement.enabled = false; }

        Rigidbody2D rb = BossGameObject.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; }
    }

    #endregion

} // class BossManager

