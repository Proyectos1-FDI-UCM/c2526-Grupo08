//---------------------------------------------------------
// "Truco" que hace que la vida del jugador sea infinita
// Laura Garay Zubiaguirre
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Clase que modifica el límite de vida máxima del componente Health
/// cuando se detecta la pulsación de la tecla Alt.
/// </summary>
public class Hack : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [SerializeField] private Health TargetHealth;
    [SerializeField] private int NewMaxHealthValue = 100000;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    private bool _isHealthComponentReady;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    void Start()
    {
        if (TargetHealth == null)
        {
            TargetHealth = GetComponent<Health>();
        }

        _isHealthComponentReady = TargetHealth != null;
    }

    void Update()
    {
        // Usamos GetKeyDown para que solo ocurra una vez al pulsar la tecla
        if (_isHealthComponentReady && Input.GetKeyDown(KeyCode.LeftAlt))
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