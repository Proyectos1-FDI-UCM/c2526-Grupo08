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
///   3. El jugador pulsa Interact (F / X mando) para avanzar
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

    /// <summary>True si hay un DialogueSystem activo en la escena.</summary>
    public static bool HasInstance() => _instance != null;

    /// <summary>
    /// Inicializa el singleton, cachea la acción Interact del Input System
    /// y oculta la caja de diálogo desde Awake para que esté listo
    /// antes de que cualquier NarratorDialogue lo busque en Start.
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("[DialogueSystem] Ya existe una instancia. Destruyendo duplicado.");
            Destroy(gameObject);
            return;
        }
        _instance = this;

        _interactAction = InputSystem.actions?.FindAction("Interact");
        if (_interactAction == null)
            Debug.LogWarning("[DialogueSystem] Acción 'Interact' no encontrada en el InputSystem.");

        if (DialogueBox != null)
            DialogueBox.SetActive(false);
    }

    /// <summary>Limpia la referencia estática si esta instancia era la activa.</summary>
    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    #endregion

    // ---- CLASE DE DATOS ----
    #region Clase de datos

    /// <summary>
    /// Datos de una línea de diálogo: quién habla, su sprite y el texto.
    /// Se serializa directamente en el Inspector dentro de la lista DialogueLines.
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Nombre del personaje que aparece en el cuadro de diálogo. Vacío = narración sin nombre.")]
        [SerializeField] private string _speakerName;

        [Tooltip("Sprite del personaje que aparece junto al texto. Vacío = sin imagen.")]
        [SerializeField] private Sprite _characterSprite;

        [Tooltip("Texto de la línea de diálogo.")]
        [TextArea(2, 5)]
        [SerializeField] private string _text;

        /// <summary>Nombre del personaje que habla en esta línea.</summary>
        public string SpeakerName => _speakerName;

        /// <summary>Sprite del personaje para esta línea. Puede ser null.</summary>
        public Sprite CharacterSprite => _characterSprite;

        /// <summary>Texto de la línea.</summary>
        public string Text => _text;

        /// <summary>Constructor sin parámetros requerido por Unity para serialización.</summary>
        public DialogueLine() { }

        /// <summary>Crea una línea de diálogo por código (sin pasar por el Inspector).</summary>
        public DialogueLine(string speakerName, string text, Sprite characterSprite = null)
        {
            _speakerName = speakerName;
            _characterSprite = characterSprite;
            _text = text;
        }
    }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("UI — Referencias")]
    [Tooltip("Panel contenedor de la caja de diálogo. Se activa al iniciar y se desactiva al terminar.")]
    [SerializeField] private GameObject DialogueBox;

    [Tooltip("Image del personaje que habla. Se oculta si la línea no tiene sprite asignado.")]
    [SerializeField] private Image CharacterImage;

    [Tooltip("TMP_Text donde se muestra el nombre del personaje que habla.")]
    [SerializeField] private TMP_Text SpeakerNameText;

    [Tooltip("TMP_Text donde se muestra el texto de la línea actual.")]
    [SerializeField] private TMP_Text DialogueText;

    [Tooltip("TMP_Text donde se muestra el hint de la tecla para continuar.")]
    [SerializeField] private TMP_Text ContinueHint;

    [Header("Líneas por defecto (Inspector)")]
    [Tooltip("Líneas usadas si nadie llama SetLines() antes de StartDialogue().\n" +
             "SpecialEnemyInteraction y BossManager usan este campo.")]
    [SerializeField] private List<DialogueLine> DialogueLines = new List<DialogueLine>();

    [Header("Hint")]
    [Tooltip("Texto que se muestra como indicación para continuar el diálogo.")]
    [SerializeField] private string HintText = "F  /  X (mando)  ->  continuar";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Líneas activas en el diálogo actual (inyectadas por SetLines o tomadas del Inspector).</summary>
    private List<DialogueLine> _activeLines;

    /// <summary>Índice de la línea actual dentro de _activeLines.</summary>
    private int _currentLineIndex = 0;

    /// <summary>True mientras hay un diálogo activo en pantalla.</summary>
    private bool _isActive = false;

    /// <summary>Callback que se ejecuta al terminar el diálogo. Asignado en StartDialogue().</summary>
    private Action _onDialogueEnd;

    /// <summary>Acción de Input System para avanzar el diálogo (Interact / F / X mando).</summary>
    private InputAction _interactAction;

    /// <summary>
    /// Tiempo restante del cooldown de input al abrir el diálogo.
    /// Evita que el mismo frame que abre el diálogo también lo avance.
    /// </summary>
    private float _inputCooldown = 0f;

    /// <summary>Duración del cooldown de input al iniciar un diálogo, en segundos.</summary>
    private const float INPUT_COOLDOWN = 0.25f;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region MonoBehaviour

    /// <summary>
    /// Cada frame (usando unscaledDeltaTime para funcionar con timeScale=0),
    /// descuenta el cooldown de input y detecta la pulsación de Interact
    /// para avanzar al siguiente línea del diálogo activo.
    /// </summary>
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

    /// <summary>
    /// Actualiza los elementos de UI con el contenido de la línea actual:
    /// nombre del hablante, sprite del personaje y texto.
    /// Oculta los elementos que no aplican a esa línea.
    /// </summary>
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

    /// <summary>
    /// Avanza al siguiente índice de línea. Si ya no quedan líneas,
    /// termina el diálogo; si quedan, muestra la siguiente.
    /// Reinicia el cooldown de input para evitar saltos dobles.
    /// </summary>
    private void AdvanceDialogue()
    {
        _currentLineIndex++;
        _inputCooldown = INPUT_COOLDOWN;

        if (_currentLineIndex >= _activeLines.Count)
            EndDialogue();
        else
            ShowCurrentLine();
    }

    /// <summary>
    /// Finaliza el diálogo: desactiva la caja, limpia el estado interno
    /// y ejecuta el callback _onDialogueEnd si estaba asignado.
    /// </summary>
    private void EndDialogue()
    {
        _isActive = false;
        _activeLines = null;

        if (DialogueBox != null) DialogueBox.SetActive(false);

        Action cb = _onDialogueEnd;
        _onDialogueEnd = null;
        cb?.Invoke();
    }

    #endregion

} // class DialogueSystem
  // Alexia Pérez Santana