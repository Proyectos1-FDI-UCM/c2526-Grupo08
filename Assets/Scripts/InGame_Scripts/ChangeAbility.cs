//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Responsable de la creación de este archivo
// Nombre del juego
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class ChangeAbility : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Abilities")]
    [SerializeField] private MonoBehaviour _multiAbility;
    [SerializeField] private MonoBehaviour _explosiveAbility;
    [SerializeField] private MonoBehaviour _chargedattackAbility;
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    private InputAction _changeAbilityAction;

    private int _currentIndex = 0;
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    #endregion
    
    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour
    
    // Por defecto están los típicos (Update y Start) pero:
    // - Hay que añadir todos los que sean necesarios
    // - Hay que borrar los que no se usen 
    
    /// <summary>
    /// Start is called on the frame when a script is enabled just before 
    /// any of the Update methods are called the first time.
    /// </summary>
    private void Start()
    {
        _changeAbilityAction = InputSystem.actions.FindAction("ChangeAbility");

        if (_changeAbilityAction == null)
        {
            Debug.LogError("Acción no encontrada.");
            enabled = false;
            return;
        }

        _changeAbilityAction.Enable();
        UpdateAbilities();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        if (_changeAbilityAction.WasPressedThisFrame()) 
        {
            SwitchAbility();
        }
    }
    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Documentar cada método que aparece aquí con ///<summary>
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)
    // Ejemplo: GetPlayerController

    #endregion
    
    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void SwitchAbility()
    {
        _currentIndex = (_currentIndex + 1) % 3;
        UpdateAbilities();
    }

    private void UpdateAbilities()
    {
        if (_chargedattackAbility != null)
        {
            _chargedattackAbility.enabled = (_currentIndex == 0);
        }

        if (_multiAbility != null)
        {
            _multiAbility.enabled = (_currentIndex == 1);
        }

        if (_explosiveAbility != null)
        {
            _explosiveAbility.enabled = (_currentIndex == 2);
        }
    }
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    #endregion   

} // class ChangeAbility 
// namespace
