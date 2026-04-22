//---------------------------------------------------------
// Objeto recolectable: llave especial del enemigo especial.
// A diferencia de Objects.cs, este script registra la llave especial
// en Inventory (hasSpecialKey) en lugar de la llave genérica (hasKey).
// Alexia Pérez Santana
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cuando el jugador entra en el trigger y pulsa F (Interact),
/// registra la llave especial en el inventario y se destruye.
/// Muestra un prompt encima del objeto mientras el jugador está en rango.
///
/// Requiere que Inventory tenga el método CollectSpecialKey().
/// </summary>
public class SpecialKeyPickup : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("UI")]
    [Tooltip("Objeto de UI (world-space o screen-space) que indica que se puede recoger")]
    [SerializeField] private GameObject PickupPrompt;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>True mientras el jugador está dentro del trigger.</summary>
    private bool _playerInRange = false;

    /// <summary>Referencia al inventario del jugador en rango.</summary>
    private Inventory _playerInventory;

    private InputAction _interactAction;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
        if (_interactAction == null)
        { 
            Debug.LogWarning("[SpecialKeyPickup] Acción 'Interact' no encontrada."); 
        }
        else
        {
            _interactAction.Enable();
        }

        if (PickupPrompt != null) { PickupPrompt.SetActive(false); }
    }

    private void Update()
    {
        if (!_playerInRange || _playerInventory == null) { return; }

        bool interactPressed = _interactAction.WasPressedThisFrame();

        if (interactPressed)
        { 
            PickUp(); 
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Inventory inv = other.GetComponent<Inventory>();
        if (inv == null) { return; }

        _playerInRange = true;
        _playerInventory = inv;
        if (PickupPrompt != null) { PickupPrompt.SetActive(true); }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Inventory>() == null) { return; }

        _playerInRange = false;
        _playerInventory = null;
        if (PickupPrompt != null) { PickupPrompt.SetActive(false); }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Registra la llave especial en el inventario y destruye el objeto.
    /// </summary>
    private void PickUp()
    {
        _playerInventory.CollectSpecialKey();
        Destroy(gameObject);
    }

    #endregion

} // class SpecialKeyPickup