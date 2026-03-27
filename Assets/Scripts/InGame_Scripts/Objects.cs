//---------------------------------------------------------
// Script que reciben todos los objetos recolectables del juego.
// Detecta cuando el jugador pulsa F cerca del objeto y lo añade
// al inventario del jugador.
// Adriana Fernández Luna
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Componente de objeto recolectable (venda, llave, fusible).
/// Cuando el jugador entra en el trigger, espera a que pulse F
/// (acción "Interact" del nuevo Input System) para añadir el
/// objeto al inventario y destruirse.
/// El tipo de objeto se configura en el Inspector.
/// </summary>
public class Objects : MonoBehaviour
{
    // ---- TIPOS DE OBJETO ----
    public enum ObjectsType { bandage, key, fusible, card, multiAbility, explosiveAbility }

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Tooltip("Tipo de objeto recolectable.")]
    [SerializeField] private ObjectsType type;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Acción de input para interactuar (tecla F).</summary>
    private InputAction _interactAction;

    /// <summary>True mientras el jugador está dentro del trigger.</summary>
    private bool _playerInRange = false;

    /// <summary>Referencia al inventario del jugador cuando está en rango.</summary>
    private Inventory _playerInventory;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
        if (_interactAction == null)
        {
            Debug.LogWarning("[Objects] Acción 'Interact' no encontrada en el Input System. " +
                             "Usando KeyCode.F como fallback.");
        }
        else
        {
            _interactAction.Enable();
        }
    }

    private void Update()
    {
        if (!_playerInRange || _playerInventory == null) return;

        // Detectar pulsación de F con el nuevo Input System o fallback con KeyCode
        bool interactPressed = _interactAction != null
            ? _interactAction.WasPressedThisFrame()
            : Input.GetKeyDown(KeyCode.F);

        if (interactPressed)
        {
            PickUp();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Inventory inventory = other.GetComponent<Inventory>();
        if (inventory != null)
        {
            _playerInRange = true;
            _playerInventory = inventory;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Inventory>() != null)
        {
            _playerInRange = false;
            _playerInventory = null;
        }
    }

    private void OnDestroy()
    {
        if (_interactAction != null);
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Añade este objeto al inventario del jugador y destruye el GameObject.
    /// </summary>
    private void PickUp()
    {
        _playerInventory.AddItem(type);
        Destroy(gameObject);
    }

    #endregion

} // class Objects
  // Adriana Fernández Luna — Laura Garay Zubiaguirre