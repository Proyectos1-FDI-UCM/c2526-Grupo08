//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Responsable de la creación de este archivo
// Nombre del juego
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class Health : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints


    [SerializeField] private int MaxHealth = 200;
    [SerializeField] HealthBar HealthBar; //Cuando tenga daño tengo que llamarla para que se modifique
    [SerializeField] GameObject EnemyGameObject;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    private InputAction _healingAction;
    //private InteractuarObjetos _pickUpBandage;
    //private Vendas bandage;

    private int _currentHealth;

    private bool _picked;

    private bool _isImmune = false;

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
        _currentHealth = MaxHealth;
        
        _healingAction = InputSystem.actions.FindAction("Healing");
        if (_healingAction == null)
        {
            Debug.Log("Accion no encontrada, no funciona curarse");
            Destroy(this);
        }

        HealthBar.SetValue(MaxHealth);
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

    public void Damage(int damageAmount)
    {
        if (_isImmune)
        {
            return;
        }

        _currentHealth -= damageAmount;
        if (HealthBar != null) HealthBar.SetValue(_currentHealth);
        DestroyEnemy();
        PlayerDeath();
    }

    public void Healing(int bandageHealing)
    {
        if (_currentHealth < MaxHealth)
        {
            _currentHealth += bandageHealing;
        }
        else if (_currentHealth >= MaxHealth)
        {
            _currentHealth = MaxHealth;
        }
        if (HealthBar != null) HealthBar.SetValue(_currentHealth);
    }

    #endregion

    public void SetImmune(bool immune)
    {
        _isImmune = immune;
    }

    public bool IsImmune()
    {
        return _isImmune;
    }

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    /// <summary>
    /// Distintos comportamientos de muerte, diferenciando al enemigo de al jugador
    /// </summary>
    private void DestroyEnemy()
    {
        EnemyPatrol enemy = GetComponent<EnemyPatrol>();
        if (enemy != null && _currentHealth <= 0)
        {
            Destroy(EnemyGameObject);
            //TODO: liberar magia cuando se muera el enemigo
        }
    }

    private void PlayerDeath()
    {
        PlayerMovement player = GetComponent<PlayerMovement>();
        if (player != null)
        {
            GameManager.Instance.UpdateGUI(_currentHealth);
        }
    }

    #endregion

}
// class PlayerHealth 
// namespace
