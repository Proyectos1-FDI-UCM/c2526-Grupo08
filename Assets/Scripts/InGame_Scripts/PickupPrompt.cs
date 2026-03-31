//---------------------------------------------------------
// Muestra un prompt encima de cualquier objeto recogible cuando el jugador
// está en rango. Se desactiva al alejarse y desaparece para siempre
// cuando el objeto padre es destruido (recogido).
// Componente independiente: no modifica Objects.cs ni ningún otro script.
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Se añade como componente al mismo GameObject que Objects.cs (o cualquier
/// recogible). Detecta si el jugador entra/sale del trigger del padre y
/// muestra/oculta un TMP_Text con el texto de interacción.
///
/// El texto se genera automáticamente según el dispositivo activo:
///   - Teclado/ratón  → "F — recoger"
///   - Mando          → "B — recoger"
///
/// SETUP EN INSPECTOR:
///   · PromptText  → TMP_Text hijo de este GameObject (o asignado manualmente).
///   · OffsetY     → cuánto sube el prompt por encima del objeto (en unidades de mundo).
///   · KeyboardText / GamepadText → textos personalizables si se quiere cambiar el mensaje.
///
/// El componente se autodeshabilita si el padre no tiene Collider2D trigger,
/// mostrando un warning en consola para ayudar al setup.
/// </summary>
public class PickupPrompt : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("UI")]
    [Tooltip("TMP_Text que muestra el prompt. Si está vacío se busca en los hijos automáticamente.")]
    [SerializeField] private TMP_Text PromptText;

    [Tooltip("Desplazamiento vertical del prompt respecto al objeto (en unidades de mundo)")]
    [SerializeField] private float OffsetY = 0.6f;

    [Header("Textos")]
    [Tooltip("Texto mostrado cuando se usa teclado")]
    [SerializeField] private string KeyboardText = "F — recoger";

    [Tooltip("Texto mostrado cuando se usa mando")]
    [SerializeField] private string GamepadText = "B — recoger";

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>True mientras el jugador está dentro del trigger.</summary>
    private bool _playerInRange = false;

    /// <summary>True una vez el objeto ha sido recogido. Evita reactivar el prompt.</summary>
    private bool _pickedUp = false;

    /// <summary>Transform del texto para actualizar su posición cada frame.</summary>
    private Transform _promptTransform;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Awake()
    {
        // Buscar TMP_Text automáticamente si no se asignó en Inspector
        if (PromptText == null)
            PromptText = GetComponentInChildren<TMP_Text>(true);

        if (PromptText == null)
        {
            Debug.LogWarning($"[PickupPrompt] No se encontró TMP_Text en '{gameObject.name}'. " +
                             "Crea un hijo con TMP_Text o asígnalo en el Inspector.");
            enabled = false;
            return;
        }

        _promptTransform = PromptText.transform;

        // Verificar que el padre tiene un trigger (necesario para OnTriggerEnter2D)
        bool hasTrigger = false;
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            if (col.isTrigger) { hasTrigger = true; break; }
        }
        if (!hasTrigger)
            Debug.LogWarning($"[PickupPrompt] '{gameObject.name}' no tiene Collider2D trigger. " +
                             "El prompt no se activará. Añade un trigger al objeto.");

        OcultarPrompt();
    }

    private void Update()
    {
        if (_pickedUp || !_playerInRange) return;

        // Posicionar el prompt encima del objeto cada frame (sigue al objeto si se mueve)
        _promptTransform.position = transform.position + new Vector3(0f, OffsetY, 0f);

        // Actualizar texto según dispositivo activo en tiempo real
        ActualizarTextoDispositivo();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_pickedUp) return;
        if (other.GetComponent<Inventory>() == null) return;

        _playerInRange = true;
        MostrarPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Inventory>() == null) return;

        _playerInRange = false;
        OcultarPrompt();
    }

    private void OnDestroy()
    {
        // El objeto ha sido destruido (recogido): el prompt desaparece con él.
        // No hay nada que limpiar porque el texto es hijo del mismo GameObject.
        _pickedUp = true;
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void MostrarPrompt()
    {
        if (PromptText == null) return;
        ActualizarTextoDispositivo();
        PromptText.gameObject.SetActive(true);
    }

    private void OcultarPrompt()
    {
        if (PromptText == null) return;
        PromptText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Comprueba si el último dispositivo que envió input es un gamepad
    /// y actualiza el texto del prompt en consecuencia.
    /// Funciona con el new Input System sin necesidad de referencias adicionales.
    /// </summary>
    private void ActualizarTextoDispositivo()
    {
        if (PromptText == null) return;

        bool esGamepad = Gamepad.current != null &&
                         InputSystem.GetDevice<Gamepad>() == Gamepad.current;

        PromptText.text = esGamepad ? GamepadText : KeyboardText;
    }

    #endregion

} // class PickupPrompt