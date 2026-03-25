//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Responsable de la creación de este archivo
// Nombre del juego
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class MenuManager : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject Options;
    [SerializeField] private GameObject Credits;
    [SerializeField] private float _followDelay = 0.5f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
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
    void Start()
    {
        if(MainMenu != null) MainMenu.SetActive(true); 
        if(Options != null) Options.SetActive(false);
        if(Credits != null) Options.SetActive(false);
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

    public void ChangeScene(int index)
    {
        System.GC.Collect();
        SceneManager.LoadScene(index);
        System.GC.Collect();
    }

    public void RestarDelay()
    {
        if(_followDelay > 0)_followDelay -= 0.1f;
        Mathf.Round(_followDelay);
    }
    public void SumarDelay()
    {
        if(_followDelay <= 1.5f) _followDelay += 0.1f;
        Mathf.Round(_followDelay);
    }

    public float GetFollowDelay() => _followDelay;

    public void ActiveMainMenu()
    {
        if (MainMenu != null) MainMenu.SetActive(true);
        if (Options != null) Options.SetActive(false);
        if (Credits != null) Credits.SetActive(false);
    }

    public void ActiveOptions()
    {
        if (MainMenu != null) MainMenu.SetActive(false);
        if (Options != null) Options.SetActive(true);
        if (Credits != null) Credits.SetActive(false);
    }

    public void ActiveCredits()
    {
        if (MainMenu != null) MainMenu.SetActive(false);
        if (Options != null) Options.SetActive(false);
        if (Credits != null) Credits.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit(); //Función para salir del juego
        Debug.Log("Saliendo del juego");
    }



    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    #endregion

} // class MenuManager 
// namespace
