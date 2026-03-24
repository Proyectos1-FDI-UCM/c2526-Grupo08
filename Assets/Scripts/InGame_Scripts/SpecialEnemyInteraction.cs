//---------------------------------------------------------
// Gestiona la interacción del jugador con el enemigo especial derrotado.
// Muestra el prompt de acercamiento (F / B en mando), el panel de opciones
// y lanza el diálogo o el remate según la elección.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Se activa cuando SpecialEnemyDeath termina la animación de caída.
/// Detecta si el jugador está cerca (por distancia), muestra el prompt "Pulsa F / B"
/// y al pulsar la tecla abre un panel con dos opciones: Amenazar o Rematar.
/// - Amenazar: pausa el juego y activa DialogueSystem.
/// - Rematar: da +15 de magia, cura al jugador y destruye el enemigo.
///
/// IMPORTANTE sobre F en el panel de opciones:
/// Mientras el panel está abierto este script ignora F para que no haya
/// conflicto con el DialogueSystem. El jugador navega con los botones de UI
/// (ratón o mando con navegación de UI).
/// Cuando el diálogo arranca, _dialogueActive = true y este script
/// queda completamente inactivo hasta que el diálogo termina.
/// </summary>
public class SpecialEnemyInteraction : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Detección de proximidad")]
    [Tooltip("Radio en el que el jugador puede interactuar con el enemigo derrotado")]
    [SerializeField] private float InteractionRadius = 1.5f;

    [Header("UI - Prompt")]
    [Tooltip("Objeto de UI que muestra 'Pulsa F / B para interactuar'")]
    [SerializeField] private GameObject PromptUI;

    [Header("UI - Panel de opciones")]
    [Tooltip("Panel con los dos botones de decisión (Amenazar / Rematar)")]
    [SerializeField] private GameObject OptionsPanel;

    [Tooltip("Botón de la opción Amenazar")]
    [SerializeField] private Button AmenazarButton;

    [Tooltip("Botón de la opción Rematar")]
    [SerializeField] private Button RematarButton;

    [Header("Recompensas al rematar (drops en el suelo)")]
    [Tooltip("Prefab del drop de magia (MagicDrop). Se instancian tantos como MagicDropCount.")]
    [SerializeField] private GameObject MagicDropPrefab;

    [Tooltip("Cuántos drops de magia aparecen al rematar. Por defecto 2 para sumar ~15 pts.")]
    [SerializeField] private int MagicDropCount = 2;

    [Tooltip("Prefab de la venda (Objects con tipo bandage). Aparecen 2 al rematar.")]
    [SerializeField] private GameObject BandagePrefab;

    [Tooltip("Cuántas vendas aparecen al rematar.")]
    [SerializeField] private int BandageCount = 2;

    [Tooltip("Separación entre drops instanciados para que no se solapen.")]
    [SerializeField] private float DropSpread = 0.4f;

    [Header("Sistema de diálogo")]
    [Tooltip("Referencia al DialogueSystem de la escena")]
    [SerializeField] private DialogueSystem DialogueSystemRef;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    private bool _interactionEnabled = false;
    private bool _playerInRange = false;
    private bool _optionsOpen = false;

    /// <summary>
    /// True mientras el DialogueSystem está reproduciendo el diálogo.
    /// Impide que este script procese ninguna entrada y que F interfiera.
    /// </summary>
    private bool _dialogueActive = false;

    private Transform _playerTransform;
    private InputAction _interactAction;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerTransform = player.transform;
        else
            Debug.LogWarning("[SpecialEnemyInteraction] No se encontró jugador con tag 'Player'.");

        _interactAction = InputSystem.actions.FindAction("Interact");
        if (_interactAction == null)
            Debug.LogWarning("[SpecialEnemyInteraction] Acción 'Interact' no encontrada en el InputSystem.");
    }

    private void Start()
    {
        if (PromptUI != null) PromptUI.SetActive(false);
        if (OptionsPanel != null) OptionsPanel.SetActive(false);

        if (AmenazarButton != null) AmenazarButton.onClick.AddListener(OnAmenazar);
        if (RematarButton != null) RematarButton.onClick.AddListener(OnRematar);

        // Desactivar hasta que el enemigo caiga
        enabled = false;
    }

    private void Update()
    {
        if (!_interactionEnabled || _playerTransform == null) return;

        // Mientras el diálogo está activo, este script no hace nada en absoluto
        if (_dialogueActive) return;

        float dist = Vector2.Distance(transform.position, _playerTransform.position);
        _playerInRange = dist <= InteractionRadius;

        if (!_optionsOpen)
        {
            // Mostrar u ocultar el prompt según proximidad
            if (PromptUI != null)
                PromptUI.SetActive(_playerInRange);

            // F / B abre el panel de opciones
            if (_playerInRange && _interactAction != null && _interactAction.WasPressedThisFrame())
            {
                AbrirOpciones();
            }
        }
        // Con el panel abierto F no hace nada aquí:
        // el jugador elige con los botones (ratón o navegación de mando por UI)
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Llamado por SpecialEnemyDeath cuando la animación de caída termina.
    /// </summary>
    public void EnableInteraction()
    {
        _interactionEnabled = true;
        enabled = true;

        if (_interactAction != null)
            _interactAction.Enable();
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>Abre el panel de dos opciones y oculta el prompt.</summary>
    private void AbrirOpciones()
    {
        _optionsOpen = true;
        if (PromptUI != null) PromptUI.SetActive(false);
        if (OptionsPanel != null) OptionsPanel.SetActive(true);
    }

    /// <summary>
    /// Opción 1: Amenazar. Cierra el panel, pausa el juego y activa el diálogo.
    /// </summary>
    private void OnAmenazar()
    {
        if (OptionsPanel != null) OptionsPanel.SetActive(false);
        _optionsOpen = false;

        // Marcar diálogo activo ANTES de pausar para que Update no interfiera
        _dialogueActive = true;

        Time.timeScale = 0f;

        if (DialogueSystemRef != null)
        {
            DialogueSystemRef.StartDialogue(OnDialogueEnd);
        }
        else
        {
            Debug.LogWarning("[SpecialEnemyInteraction] No hay DialogueSystem asignado.");
            Time.timeScale = 1f;
            Destroy(gameObject);
        }
    }

    /// <summary>Callback del DialogueSystem al terminar todas las líneas.</summary>
    private void OnDialogueEnd()
    {
        _dialogueActive = false;
        Time.timeScale = 1f;
        Destroy(gameObject);
    }

    /// <summary>
    /// Opción 2: Rematar. Instancia drops de magia y vendas en el suelo para
    /// que el jugador los recoja, y destruye al enemigo.
    /// Los drops se esparcen ligeramente para que no se solapen.
    /// </summary>
    private void OnRematar()
    {
        if (OptionsPanel != null) OptionsPanel.SetActive(false);
        _optionsOpen = false;

        Vector2 basePos = transform.position;

        // Instanciar drops de magia en el suelo
        if (MagicDropPrefab != null)
        {
            for (int i = 0; i < MagicDropCount; i++)
            {
                // Desplazar cada drop en círculo para que no se solapen
                Vector2 offset = new Vector2(
                    Mathf.Cos(i * Mathf.PI * 2f / MagicDropCount) * DropSpread,
                    Mathf.Sin(i * Mathf.PI * 2f / MagicDropCount) * DropSpread
                );
                Instantiate(MagicDropPrefab, basePos + offset, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("[SpecialEnemyInteraction] MagicDropPrefab no asignado.");
        }

        // Instanciar vendas en el suelo
        if (BandagePrefab != null)
        {
            for (int i = 0; i < BandageCount; i++)
            {
                // Desplazar en dirección opuesta a los de magia
                Vector2 offset = new Vector2(
                    Mathf.Cos(i * Mathf.PI * 2f / BandageCount + Mathf.PI) * DropSpread,
                    Mathf.Sin(i * Mathf.PI * 2f / BandageCount + Mathf.PI) * DropSpread
                );
                Instantiate(BandagePrefab, basePos + offset, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("[SpecialEnemyInteraction] BandagePrefab no asignado.");
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, InteractionRadius);
    }

    #endregion

} // class SpecialEnemyInteraction