//---------------------------------------------------------
// Sistema de diálogo genérico por líneas. Muestra una caja
// con imagen del personaje, nombre, texto y hint de tecla.
// Funciona con Time.timeScale = 0 (usa unscaledDeltaTime).
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
/// Sistema de diálogo reutilizable. Singleton local por escena.
///
/// Flujo de uso:
///   1. (Opcional) dialogueSystem.SetLines(miLista)
///   2. dialogueSystem.StartDialogue(miCallback)
///   3. El jugador pulsa Interact (F / B mando) para avanzar
///   4. Al terminar llama miCallback y oculta la caja
///
/// SINGLETON: NarratorDialogue puede encontrarlo automáticamente con
/// DialogueSystem.Instance sin necesidad de asignarlo en el Inspector.
/// Si hay referencia directa en el Inspector, tiene prioridad.
///
/// Estructura de UI esperada (asignar en Inspector):
///   DialogueBox     → panel contenedor (desactivado por defecto)
///   CharacterImage  → Image del personaje
///   SpeakerNameText → TMP_Text con el nombre del hablante
///   DialogueText    → TMP_Text con el texto de la línea
///   ContinueHint    → TMP_Text con el hint de tecla
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    // ---- SINGLETON ----
    #region Singleton

    private static DialogueSystem _instance;

    /// <summary>Instancia única de la escena. Disponible tras Awake().</summary>
    public static DialogueSystem Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogWarning("[DialogueSystem] No hay instancia en esta escena. " +
                                 "Asegúrate de que el GameObject con DialogueSystem está en la escena.");
            return _instance;
        }
    }
    public static bool HasInstance() => _instance != null;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("[DialogueSystem] Ya existe una instancia. Destruyendo duplicado.");
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // Registrar input y ocultar la caja desde Awake para que esté listo
        // antes de que cualquier NarratorDialogue lo busque en Start
        _interactAction = InputSystem.actions?.FindAction("Interact");
        if (_interactAction == null)
            Debug.LogWarning("[DialogueSystem] Acción 'Interact' no encontrada en el InputSystem.");

        if (DialogueBox != null)
            DialogueBox.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    #endregion

    // ---- CLASE DE DATOS ----
    #region Clase de datos

    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Nombre del personaje. Vacío = narración sin nombre.")]
        public string SpeakerName;

        [Tooltip("Sprite del personaje. Vacío = sin imagen (narración pura).")]
        public Sprite CharacterSprite;

        [Tooltip("Texto de la línea.")]
        [TextArea(2, 5)]
        public string Text;
    }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("UI — Referencias")]
    [SerializeField] private GameObject DialogueBox;
    [SerializeField] private Image CharacterImage;
    [SerializeField] private TMP_Text SpeakerNameText;
    [SerializeField] private TMP_Text DialogueText;
    [SerializeField] private TMP_Text ContinueHint;

    [Header("Líneas por defecto (Inspector)")]
    [Tooltip("Líneas usadas si nadie llama SetLines() antes de StartDialogue().\n" +
             "SpecialEnemyInteraction y BossManager usan este campo.")]
    [SerializeField] private List<DialogueLine> DialogueLines = new List<DialogueLine>();

    [Header("Hint")]
    [SerializeField] private string HintText = "F  /  B (mando)  →  continuar";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    private List<DialogueLine> _activeLines;
    private int _currentLineIndex = 0;
    private bool _isActive = false;
    private Action _onDialogueEnd;

    private InputAction _interactAction;

    // Cooldown para evitar que el mismo frame que abre el diálogo también lo avance
    private float _inputCooldown = 0f;
    private const float INPUT_COOLDOWN = 0.25f;

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour

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
    /// Inyecta líneas en runtime. Llamar ANTES de StartDialogue().
    /// Permite reutilizar un único DialogueSystem para toda la escena.
    /// </summary>
    public void SetLines(List<DialogueLine> lines)
    {
        _activeLines = lines;
    }

    /// <summary>
    /// Arranca el diálogo. Usa las líneas de SetLines() si existen;
    /// si no, usa las del Inspector.
    /// La gestión de timeScale la hace el llamador (NarratorDialogue).
    /// </summary>
    public void StartDialogue(Action onEnd)
    {
        // Fallback a las líneas del Inspector
        if (_activeLines == null || _activeLines.Count == 0)
            _activeLines = DialogueLines;

        if (_activeLines == null || _activeLines.Count == 0)
        {
            Debug.LogWarning("[DialogueSystem] No hay líneas configuradas para el diálogo.");
            onEnd?.Invoke();
            return;
        }

        _onDialogueEnd = onEnd;
        _currentLineIndex = 0;
        _isActive = true;
        _inputCooldown = INPUT_COOLDOWN;

        _interactAction?.Enable();

        if (DialogueBox != null) DialogueBox.SetActive(true);
        if (ContinueHint != null) ContinueHint.text = HintText;

        ShowCurrentLine();
    }

    /// <summary>Devuelve true si hay un diálogo activo en este momento.</summary>
    public bool IsActive() => _isActive;

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void ShowCurrentLine()
    {
        DialogueLine line = _activeLines[_currentLineIndex];

        if (SpeakerNameText != null)
        {
            bool hasName = !string.IsNullOrEmpty(line.SpeakerName);
            SpeakerNameText.gameObject.SetActive(hasName);
            if (hasName) SpeakerNameText.text = line.SpeakerName;
        }

        if (CharacterImage != null)
        {
            CharacterImage.sprite = line.CharacterSprite;
            CharacterImage.gameObject.SetActive(line.CharacterSprite != null);
        }

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
        _activeLines = null; // limpiar para la próxima secuencia

        if (DialogueBox != null) DialogueBox.SetActive(false);

        Action cb = _onDialogueEnd;
        _onDialogueEnd = null;
        cb?.Invoke();
    }

    #endregion

} // class DialogueSystem