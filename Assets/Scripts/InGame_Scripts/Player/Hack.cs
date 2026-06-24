//---------------------------------------------------------
// "Truco" que hace que la vida del jugador sea infinita
// Laura Garay Zubiaguirre
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Clase que modifica el límite de vida máxima del componente Health
/// cuando se detecta la pulsación de la tecla Alt.
/// </summary>
public class Hack : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Tooltip("Componente Health al que se le aplicará la vida infinita. Si se deja vacío se busca en este mismo GameObject.")]
    [SerializeField] private Health TargetHealth;

    [Tooltip("Nuevo valor de vida máxima que se establece al activar el truco.")]
    [SerializeField] private int NewMaxHealthValue = 100000;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    private bool _isHealthComponentReady;
    private InputAction _hack;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>Obtiene la acción Hack del Input System y cachea Health si no está asignado.</summary>
    void Start()
    {
        _hack = InputSystem.actions.FindAction("Hack");
        if (TargetHealth == null)
        {
            TargetHealth = GetComponent<Health>();
        }

        _isHealthComponentReady = TargetHealth != null;
    }

     /// <summary>Cada frame comprueba si se ha pulsado la acción Hack para activar la vida infinita.</summary>ç
    void Update()
    {
        // Usamos GetKeyDown para que solo ocurra una vez al pulsar la tecla
        if (_isHealthComponentReady && _hack.WasPressedThisFrame())
        {
            IncreaseLimit();
        }
    }
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Llama al nuevo método de Health para expandir el límite de vida.
    /// </summary>
    private void IncreaseLimit()
    {
        TargetHealth.SetMaxHealth(NewMaxHealthValue);
    }

    #endregion   

} // class Hack