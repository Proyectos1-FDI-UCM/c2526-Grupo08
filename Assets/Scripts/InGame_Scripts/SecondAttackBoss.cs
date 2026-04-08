//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Marian Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class SecondAttackBoss : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    [Header("Prefabs")]
    [SerializeField] private CristlesBoss _prefabCuchilla1;
    [SerializeField] private CristlesBoss _prefabCuchilla2;
    [SerializeField] private CristlesBoss _prefabCuchilla3;
    [SerializeField] private GameObject _avisoVisualPrefab;

    [Header("Configuración")]
    [SerializeField] private float _rangoDeteccion = 10f;
    [SerializeField] private float _tiempoRecarga = 3f;
    [SerializeField] private Transform _puntoDisparo;


    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    private Transform _jugador;
    private GameObject _avisoActual;
    private Vector3 _posicionObjetivo;

    private float _timerRecarga;
    private float _timerAviso;
    private bool _preparandoAtaque;



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
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _jugador = p.transform;
        _timerRecarga = _tiempoRecarga;
    }


    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (_jugador == null) return;

        
        if (!_preparandoAtaque)
        {
            _timerRecarga += Time.deltaTime;
        }

        float distancia = Vector2.Distance(transform.position, _jugador.position);

      
        if (distancia <= _rangoDeteccion && _timerRecarga >= _tiempoRecarga && !_preparandoAtaque)
        {
            _preparandoAtaque = true;
            _timerAviso = 0;
            _posicionObjetivo = _jugador.position; 

            _avisoActual = Instantiate(_avisoVisualPrefab, _posicionObjetivo, Quaternion.identity);
        }

        if (_preparandoAtaque && _avisoActual != null)
        {
            _timerAviso += Time.deltaTime;

            if (_timerAviso >= 2f)
            {
                Disparar();
            }
        }
        
        else if (_preparandoAtaque && _avisoActual == null)
        {
            _preparandoAtaque = false;
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

    private void Disparar()
    {
        if (_avisoActual != null)
        {
            Destroy(_avisoActual);
            _avisoActual = null;
        }

        Vector2 direccion1 = (_posicionObjetivo - _puntoDisparo.position);
        Vector2 direccion2 = (_posicionObjetivo - _puntoDisparo.position);
        Vector2 direccion3 = (_posicionObjetivo - _puntoDisparo.position);

        for (int i = 0; i < 3; i++)
        {
            CristlesBoss c1 = Instantiate(_prefabCuchilla1, _puntoDisparo.position, Quaternion.identity);
            CristlesBoss c2 = Instantiate(_prefabCuchilla2, _puntoDisparo.position + new Vector3(2, 0, 0), Quaternion.Euler(0, 0, 10));
            CristlesBoss c3 = Instantiate(_prefabCuchilla3, _puntoDisparo.position - new Vector3(2, 0, 0), Quaternion.Euler(0, 0, -10));
            c1.Lanzar(direccion1);
            c2.Lanzar(direccion2);
            c3.Lanzar(direccion3);
        }

        _preparandoAtaque = false;
        _timerRecarga = 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _rangoDeteccion);
    }

    #endregion

}
// class SecondAttackBoss 
// namespace
