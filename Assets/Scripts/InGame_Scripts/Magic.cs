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

        _currentMagic = MaxMagic;

        if (MagicBar != null)
        {
            MagicBar.SetMaxValue(MaxMagic);
            MagicBar.SetValue(_currentMagic);
        }
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

    public bool TrySpendMagic(int amount)
    {
        if (_currentMagic < amount)
        {
            return false;
        }
        _currentMagic -= amount;
        if (MagicBar != null)
        {
            MagicBar.SetValue(_currentMagic);
        }
        return true;
    }

    public bool HasEnoughMagic(int amount)
    {
        return _currentMagic >= amount;
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
