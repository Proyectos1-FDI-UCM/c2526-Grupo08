//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Adriana Fernández Luna
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class Inventory : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    [SerializeField]
    private int BandageHealth = 30;
    [SerializeField] 
    private Health _health;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    private int _bandage = 0;
    private int _key = 0;
    private int _fusible = 0;

    private InputAction HealthAction;
    private Health _playerHealth;
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

    private void Awake()
    {
        _playerHealth = GetComponent<Health>();

        _health = GetComponent<Health>(); 
        if (_health == null)
        {
            Debug.LogError("No se encontró el componente Health en el jugador");
        }

        HealthAction = InputSystem.actions.FindAction("Healing");
        if (HealthAction == null)
        {
            Debug.Log("Accion no encontrada, no funciona el Inventory");
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        HealthAction.Enable();
        HealthAction.performed += OnUseBandage;
    }

    private void OnDisable()
    {
        HealthAction.Disable();
        HealthAction.performed -= OnUseBandage;
    }

    private void OnUseBandage(InputAction.CallbackContext context)
    {
        UseBandage();
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Documentar cada método que aparece aquí con ///<summary>
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)
    // Ejemplo: GetPlayerController

    public void AddItem(Objects.ObjectsType type)
    {
        switch (type)
        {
            case Objects.ObjectsType.bandage:

                _bandage += 1;
                Debug.Log("Bandages: " + _bandage);
                break;
            case Objects.ObjectsType.key:

                _key += 1;
                Debug.Log("Keys: " + _key);
                break;
            case Objects.ObjectsType.fusible:

                _fusible += 1;
                Debug.Log("Fusibles: " + _fusible);
                break;

        }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    private void UseBandage()
    {
        if (_bandage > 0)
        {
            _bandage--;
            _health.Healing(BandageHealth);
            Debug.Log("Se ha usado una venda, quedan: " + _bandage + " vendas");
        }
        else Debug.Log("No tienes vendas");
    }

    #endregion   

} // class Inventory 
// Adriana Fernández Luna
//Laura Garay Zubiaguirre
