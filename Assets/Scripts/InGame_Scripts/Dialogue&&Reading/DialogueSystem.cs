//---------------------------------------------------------
// Sistema de diálogo genérico. Muestra una caja en la parte inferior con
// imagen del personaje, nombre del hablante, texto y hint de tecla.
// Funciona con Time.timeScale = 0 (usa unscaledDeltaTime).
// Cualquier script puede inyectar sus propias líneas con SetLines()
// antes de llamar StartDialogue(), de forma que un único DialogueSystem
// por escena sirve para todos los triggers narrativos.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sistema de diálogo por líneas reutilizable.
///
/// Flujo de uso:
///   1. (Opcional) dialogueSystem.SetLines(miLista)   — inyectar líneas en runtime
///   2. dialogueSystem.StartDialogue(miCallback)       — iniciar
///   3. El jugador pulsa F / B para avanzar
///   4. Al terminar la última línea se llama miCallback y la caja se oculta
///
/// Si NO se llama SetLines(), usa las líneas configuradas en el Inspector
/// (compatibilidad con SpecialEnemyInteraction y BossManager).
///
/// Estructura de UI esperada (asignar en Inspector):
///   DialogueBox     → panel contenedor inferior (desactivado por defecto)
///   CharacterImage  → Image del personaje (izquierda)
///   SpeakerNameText → TMP_Text con el nombre del hablante (puede ser null)
///   DialogueText    → TMP_Text con el contenido de la línea
///   ContinueHint    → TMP_Text con el hint de tecla (inferior derecha)
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    // ---- CLASE DE DATOS ----
    #region Clase de datos

    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Nombre del personaje que habla. Vacío = narración sin nombre.")]
        public string SpeakerName;

        [Tooltip("Sprite del personaje (semi-realista). Vacío = sin imagen (narración).")]
        public Sprite CharacterSprite;

        [Tooltip("Texto de la línea.")]
        [TextArea(2, 5)]
        public string Text;
    }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("UI — Referencias")]
    [Tooltip("Panel contenedor de la caja de diálogo.")]
    [SerializeField] private GameObject DialogueBox;

    [Tooltip("Image del personaje (esquina inferior izquierda).")]
    [SerializeField] private Image CharacterImage;

    [Tooltip("TMP_Text con el nombre del hablante (encima del texto). Puede dejarse vacío.")]
    [SerializeField] private TMP_Text SpeakerNameText;

    [Tooltip("TMP_Text con el texto de la línea actual.")]
    [SerializeField] private TMP_Text DialogueText;

    [Tooltip("TMP_Text con el hint de tecla (inferior derecha).")]
    [SerializeField] private TMP_Text ContinueHint;

    [Header("Líneas por defecto (Inspector)")]
    [Tooltip("Líneas usadas cuando nadie llama SetLines() antes de StartDialogue().\n" +
             "SpecialEnemyInteraction y BossManager usan este campo.")]
    [SerializeField] private List<DialogueLine> DialogueLines = new List<DialogueLine>();

    [Header("Hint")]
    [Tooltip("Texto del hint de tecla para continuar.")]
    [SerializeField] private string HintText = "F  /  B (mando)  →  continuar";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Lista activa durante la reproducción (inyectada o del Inspector).</summary>
    private List<DialogueLine> _activeLines;

    private int _currentLineIndex = 0;
    private bool _isActive = false;
    private Action _onDialogueEnd;

    private InputAction _interactAction;

    /// <summary>
    /// Cooldown inicial que evita que el mismo frame que abre el diálogo
    /// también lo avance (el jugador ya tenía F pulsado).
    /// Usa unscaledDeltaTime para funcionar con timeScale = 0.
    /// </summary>
    private float _inputCooldown = 0f;
    private const float INPUT_COOLDOWN = 0.25f;

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour

    private void Awake()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
        if (_interactAction == null)
            Debug.LogWarning("[DialogueSystem] Acción 'Interact' no encontrada en el InputSystem.");

        if (DialogueBox != null)
            DialogueBox.SetActive(false);
    }

    private void Update()
    {
        if (!_isActive) { return; }

        if (_inputCooldown > 0f)
        {
            _inputCooldown -= Time.unscaledDeltaTime;
            return;
        }

        if (_interactAction != null && _interactAction.WasPressedThisFrame())
            AdvanceDialogue();
    }

    #endregion

    // ---- API PÚBLICA ----
    #region API Pública

    /// <summary>
    /// Inyecta una lista de líneas en runtime.
    /// DEBE llamarse antes de StartDialogue().
    /// Permite que un único DialogueSystem en escena reproduzca
    /// cualquier secuencia sin duplicar el componente.
    /// </summary>
    public void SetLines(List<DialogueLine> lines)
    {
        _activeLines = lines;
    }

    /// <summary>
    /// Arranca el diálogo desde la línea 0.
    /// Usa las líneas inyectadas por SetLines() si existen;
    /// si no, usa las del Inspector.
    /// La pausa del timeScale la gestiona el llamador.
    /// </summary>
    public void StartDialogue(Action onEnd)
    {
        // Fallback a las líneas del Inspector
        if (_activeLines == null || _activeLines.Count == 0)
            _activeLines = DialogueLines;

        if (_activeLines == null || _activeLines.Count == 0)
        {
            Debug.LogWarning("[DialogueSystem] No hay líneas configuradas.");
            onEnd?.Invoke();
            return;
        }

        _onDialogueEnd = onEnd;
        _currentLineIndex = 0;
        _isActive = true;
        _inputCooldown = INPUT_COOLDOWN;

        _interactAction?.Enable();

        if (DialogueBox != null) { DialogueBox.SetActive(true); }
        if (ContinueHint != null) { ContinueHint.text = HintText; }

        ShowCurrentLine();
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void ShowCurrentLine()
    {
        DialogueLine line = _activeLines[_currentLineIndex];

        // Nombre del personaje
        if (SpeakerNameText != null)
        {
            bool hasName = !string.IsNullOrEmpty(line.SpeakerName);
            SpeakerNameText.gameObject.SetActive(hasName);
            if (hasName) { SpeakerNameText.text = line.SpeakerName; }
        }

        // Imagen
        if (CharacterImage != null)
        {
            CharacterImage.sprite = line.CharacterSprite;
            CharacterImage.gameObject.SetActive(line.CharacterSprite != null);
        }

        // Texto
        if (DialogueText != null)
            DialogueText.text = line.Text;
    }

    private void AdvanceDialogue()
    {
        _currentLineIndex++;
        _inputCooldown = INPUT_COOLDOWN;

        if (_currentLineIndex >= _activeLines.Count)
            EndDialogue();
        else
            ShowCurrentLine();
    }

    private void EndDialogue()
    {
        _isActive = false;
        _activeLines = null; // limpiar para que la próxima llamada pueda inyectar nuevas líneas

        if (DialogueBox != null) { DialogueBox.SetActive(false); }

        Action callback = _onDialogueEnd;
        _onDialogueEnd = null;
        callback?.Invoke();
    }

    #endregion

} // class DialogueSystem