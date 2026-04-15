//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Marián Navarro
// No way down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class BossPhaseController : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints




    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    private Health _health;
    private BoosBehaviour _movimiento;
    private BossFisrtShoot _dash;
    private SecondAttackBoss _cuchillas;
    private AbilityBoss1 _cristales;
    private BossSummonAbility _esbirros;

    private bool _fase2Activada = false;
    private bool _fase3Activada = false;

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

    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>

    private void Update()
    {
        int vidaActual = _health.GetCurrentHealth();

        // LÓGICA DE FASE 2 (499 - 200 HP)
        if (vidaActual < 500 && vidaActual >= 200 && !_fase2Activada)
        {
            ActivarHabilidadesFase2();
        }

        // LÓGICA DE FASE 3 (199 - 0 HP)
        if (vidaActual < 200 && !_fase3Activada)
        {
            ActivarEnrageFase3();
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
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    private void Awake()
    {
        _health = GetComponent<Health>();
        _movimiento = GetComponent<BoosBehaviour>();
        _dash = GetComponent<BossFisrtShoot>();
        _cuchillas = GetComponent<SecondAttackBoss>();
        _cristales = GetComponent<AbilityBoss1>();
        _esbirros = GetComponent<BossSummonAbility>();
    }

    private void ActivarHabilidadesFase2()
    {
        _fase2Activada = true;

        // Habilitamos los scripts de habilidades que estaban apagados
        if (_cristales != null) _cristales.SetAbilityActive(true);
        if (_esbirros != null) _esbirros.enabled = true;

        Debug.Log("Fase 2 activada");
    }

    private void ActivarEnrageFase3()
    {
        _fase3Activada = true;
        float multiplicador = 1.5f;

        // 1. Buff de movimiento (en BoosBehaviour)
        if (_movimiento != null) _movimiento.AplicarBuffVelocidad(multiplicador);

        // 2. Buff de ataques (usando los nuevos setters)
        if (_dash != null) _dash.AplicarBuffFaseFinal(multiplicador);
        if (_cuchillas != null) _cuchillas.AplicarBuffFaseFinal(multiplicador);

        Debug.Log("Fase 3 activada");

        #endregion

    }
}// class BossPhaseController 
// namespace
