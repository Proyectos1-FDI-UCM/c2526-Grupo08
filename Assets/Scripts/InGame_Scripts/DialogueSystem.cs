//---------------------------------------------------------
// Sistema de diálogo genérico para el enemigo especial y el jefe final.
// Muestra una caja en la parte inferior con imagen del personaje a la izquierda,
// texto a la derecha y el hint de tecla abajo a la derecha.
// Funciona con Time.timeScale = 0 (usa unscaledDeltaTime para el cooldown).
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
/// Sistema de diálogo por líneas. Se llama con StartDialogue(callback).
/// Avanza línea a línea con F / B en mando.
/// Al terminar todas las líneas llama al callback y se oculta.
///
/// Estructura de UI esperada (asignar en Inspector):
///   DialogueBox     → panel contenedor inferior
///   CharacterImage  → Image del personaje (izquierda)
///   DialogueText    → TMP_Text con el contenido (derecha)
///   ContinueHint    → TMP_Text con el hint de tecla (inferior derecha)
///
/// IMPORTANTE: el juego está pausado (timeScale=0) durante el diálogo.
/// El cooldown de input usa Time.unscaledDeltaTime para funcionar igualmente.
/// WasPressedThisFrame del new Input System sí funciona con timeScale=0.
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    // ---- CLASES DE DATOS ----
    #region Clases de datos

    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Sprite del personaje que habla (imagen semi-realista, no chibi)")]
        public Sprite CharacterSprite;

        [Tooltip("Texto de la línea de diálogo")]
        [TextArea(2, 5)]
        public string Text;
    }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("UI - Referencias")]
    [Tooltip("Panel completo de la caja de diálogo")]
    [SerializeField] private GameObject DialogueBox;

    [Tooltip("Image del personaje que habla (esquina inferior izquierda)")]
    [SerializeField] private Image CharacterImage;

    [Tooltip("Texto principal del diálogo")]
    [SerializeField] private TMP_Text DialogueText;

    [Tooltip("Texto que indica la tecla para continuar (inferior derecha)")]
    [SerializeField] private TMP_Text ContinueHint;

    [Header("Contenido")]
    [Tooltip("Secuencia de líneas del diálogo")]
    [SerializeField] private List<DialogueLine> DialogueLines = new List<DialogueLine>();

    [Header("Hint")]
    [Tooltip("Texto del hint de tecla")]
    [SerializeField] private string HintText = "F  /  B (mando)  →  continuar";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    private int _currentLineIndex = 0;
    private bool _isActive = false;
    private Action _onDialogueEnd;

    private InputAction _interactAction;

    /// <summary>
    /// Cooldown para evitar que el mismo frame que abre el diálogo también lo avance.
    /// Usa unscaledDeltaTime porque timeScale = 0 durante el diálogo.
    /// </summary>
    private float _inputCooldown = 0f;
    private const float INPUT_COOLDOWN_DURATION = 0.25f;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Awake()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
        if (_interactAction == null)
            Debug.LogWarning("[DialogueSystem] Acción 'Interact' no encontrada.");

        if (DialogueBox != null)
            DialogueBox.SetActive(false);
    }

    private void Update()
    {
        if (!_isActive) return;

        // Reducir cooldown con tiempo sin escalar (funciona con timeScale = 0)
        if (_inputCooldown > 0f)
        {
            _inputCooldown -= Time.unscaledDeltaTime;
            return;
        }

        // WasPressedThisFrame funciona con timeScale = 0 en el new Input System
        if (_interactAction != null && _interactAction.WasPressedThisFrame())
        {
            AdvanceDialogue();
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Inicia la secuencia de diálogo desde la primera línea.
    /// El juego ya debe estar pausado cuando se llama.
    /// </summary>
    public void StartDialogue(Action onEnd)
    {
        if (DialogueLines == null || DialogueLines.Count == 0)
        {
            Debug.LogWarning("[DialogueSystem] No hay líneas configuradas.");
            onEnd?.Invoke();
            return;
        }

        _onDialogueEnd = onEnd;
        _currentLineIndex = 0;
        _isActive = true;

        if (_interactAction != null)
            _interactAction.Enable();

        if (DialogueBox != null) DialogueBox.SetActive(true);
        if (ContinueHint != null) ContinueHint.text = HintText;

        // Cooldown inicial: evita avanzar el mismo frame que se abre
        _inputCooldown = INPUT_COOLDOWN_DURATION;
        ShowCurrentLine();
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void ShowCurrentLine()
    {
        if (_currentLineIndex >= DialogueLines.Count) return;

        DialogueLine line = DialogueLines[_currentLineIndex];

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
        _inputCooldown = INPUT_COOLDOWN_DURATION;

        if (_currentLineIndex >= DialogueLines.Count)
            EndDialogue();
        else
            ShowCurrentLine();
    }

    private void EndDialogue()
    {
        _isActive = false;
        if (DialogueBox != null) DialogueBox.SetActive(false);
        _onDialogueEnd?.Invoke();
        _onDialogueEnd = null;
    }

    #endregion

} // class DialogueSystem