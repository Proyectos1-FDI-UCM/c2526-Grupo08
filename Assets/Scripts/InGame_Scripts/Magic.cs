//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Celia 
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
public class Magic : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    [Tooltip("Magia máxima. (GDD: jugador 60)")]
    [SerializeField] private int MaxMagic = 60;

    [Tooltip("Barra de vida que muestra la vida en pantalla. Asignar desde el Inspector.")]
    [SerializeField] private UIBar MagicBar;

    [Tooltip("Cantidad de magia que gasta cada habilidad mágica. GDD: cargado = 20; multidirección = 30; explosión = 35")]
    [SerializeField] private int ChargedAttackPoints = 20;
    [SerializeField] private int MultiDirAttackPoints = 30; //Cada bala hace 30 de daño
    [SerializeField] private int ExplosiveAttackPoints = 35;


    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    private int _currentMagic;

    private InputAction _magicAbility1;
    private InputAction _magicAbility2;

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
    void Start()
    {
        _magicAbility1 = InputSystem.actions.FindAction("Attack"); //ESTO ESTÁ MAL, ES DE PRUEBA
        //_magicAbility1 = InputSystem.actions.FindAction("Charged");
        _magicAbility2 = InputSystem.actions.FindAction("MultiDir_Explosion");
        if (_magicAbility1 == null)
        {
            Debug.LogError("[Magic] Acción 'Charged' no encontrada.");
            enabled = false;
            return;
        }

        if (_magicAbility2 == null)
        {
            Debug.LogError("[Magic] Acción 'MultiDir_Explosion' no encontrada.");
            enabled = false;
            return;
        }

        _currentMagic = MaxMagic;

        if (MagicBar != null)
        {
            MagicBar.SetMaxValue(MaxMagic);
            MagicBar.SetValue(_currentMagic);
        }

        _magicAbility1.Enable();
        _magicAbility2.Enable();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        
    }
    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Documentar cada método que aparece aquí con ///<summary>
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)
    // Ejemplo: GetPlayerController

    public void IncreaseMagicAmount(int magicPoints)
    {
        _currentMagic = Mathf.Min(_currentMagic + magicPoints, MaxMagic);
        if (MagicBar != null) MagicBar.SetValue(_currentMagic);
        Debug.Log("La magia aumentó");
    }

    public void DecreaseMagic()
    {
        if (_currentMagic >= ChargedAttackPoints && _magicAbility1.IsPressed())
        //Cuando estén las habilidades hechas también depedenderá de qué componente está activado
        //algo así como ChargedAttackAbility.Enabled()
        {
            _currentMagic = Mathf.Max(_currentMagic - ChargedAttackPoints, 0); //Para evitar que baje de 0
            if (MagicBar != null) MagicBar.SetValue(_currentMagic);
            Debug.Log("La magia disminuyó");
        }

        //TODO: hacer un caso para cada habilidad

    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    /// <summary>
    /// Decrementa los puntos de magia cuando se usa una habilidad mágica
    /// </summary>


    #endregion

} // class Magic 
// namespace
