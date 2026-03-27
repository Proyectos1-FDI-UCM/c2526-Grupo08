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

    [SerializeField] private GameObject _prefabTriangulo;
    [SerializeField] private GameObject _avisoVisualPrefab;
    [SerializeField] private float _rangoDeteccion = 10f;
    [SerializeField] private float _tiempoRecarga = 3f;
    [SerializeField] private Transform _puntoDisparo;

    private Transform _jugador;
    private GameObject _avisoActual;
    private float _timerAtaque;
    private float _timerAviso;
    private Vector3 _posicionRegistrada;
    private bool _alreadyAviso;


    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

   
    private void LanzarCuchillas()
    { 
        // Lanzar 3 proyectiles
        for (int i = 0; i < 3; i++)
        {
            GameObject proyectil = Instantiate(_prefabTriangulo, _puntoDisparo.position, Quaternion.identity);
            Vector2 direccion = (_posicionRegistrada - _puntoDisparo.position).normalized;

            if (proyectil.TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = direccion* 10f;
            }
        }

        // Resetear todo para el siguiente ciclo
        _alreadyAviso = false;
        _timerAtaque = 0;
    }


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
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _jugador = playerObj.transform;
    }



    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (_jugador == null) return;

        float distancia = Vector2.Distance(transform.position, _jugador.position);
        _timerAtaque += Time.deltaTime;

        // 1. Detección y creación del aviso
        if (distancia <= _rangoDeteccion && _timerAtaque >= _tiempoRecarga && !_alreadyAviso)
        {
            _posicionRegistrada = _jugador.position;
            _avisoActual = Instantiate(_avisoVisualPrefab, _posicionRegistrada, Quaternion.identity);
            _alreadyAviso = true;
            _timerAviso = 0; //Esto se reinicia que sino no diapara otra vez
        }


        if (_alreadyAviso)
        {
            _timerAviso += Time.deltaTime;

            if (_timerAviso >= 2f)//He puesto 2 porque 1 me parece muy difícil, si vemos que no, lo cambio
            {
                LanzarTriangulos();
            }
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

    private void LanzarTriangulos()
    {
        if (_avisoActual != null) Destroy(_avisoActual);

        // Lanzar 3 proyectiles
        for (int i = 0; i < 3; i++)
        {
            GameObject proyectil = Instantiate(_prefabTriangulo, _puntoDisparo.position, Quaternion.identity);
            Vector2 direccion = (_posicionRegistrada - _puntoDisparo.position).normalized;

            if (proyectil.TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = direccion * 10f;
            }
        }

        // Resetear todo para el siguiente ciclo
        _alreadyAviso = false;
        _timerAtaque = 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _rangoDeteccion);
    }

    #endregion

} // class SecondAttackBoss 
// namespace
