//---------------------------------------------------------
// Cuchillo/cristal lanzado por SecondAttackBoss: se desplaza en línea
// recta en la dirección indicada y aplica daño al jugador al impactar.
// Marián Navarro
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Proyectil lanzado por SecondAttackBoss (ataque de "cuchillas").
/// Lanzar() le da una velocidad y rotación según la dirección indicada.
/// Al colisionar con el jugador, le aplica _damage puntos de daño.
/// </summary>
public class CristalesBoss : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Tooltip("Velocidad de desplazamiento del proyectil.")]
    [SerializeField] private float _speed = 12f;

    [Tooltip("Puntos de daño que aplica al jugador al impactar.")]
    [SerializeField] private int _damage = 35;

    [Tooltip("Tiempo de vida en segundos (reservado: actualmente el proyectil no se autodestruye por tiempo, ver Start()).")]
    [SerializeField] private int _tiempoVida;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Rigidbody2D del proyectil, cacheado en Awake.</summary>
    private Rigidbody2D _rb;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea el Rigidbody2D antes que Start.
    /// </summary>
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Punto de inicialización del proyectil. Actualmente vacío:
    /// la autodestrucción por _tiempoVida está reservada para el futuro
    /// (ver tooltip de _tiempoVida).
    /// </summary>
    void Start()
    {
    }

    /// <summary>
    /// Si lo que entra en contacto con el proyectil es el jugador,
    /// le aplica _damage puntos de daño.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerMovement>() != null)
        {
            Health otherHealth = other.GetComponent<Health>();
            if (otherHealth != null)
            {
                otherHealth.Damage(_damage);
            }
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Lanza el proyectil en la dirección indicada: le aplica velocidad
    /// _speed en esa dirección y rota el sprite para que apunte hacia ella.
    /// </summary>
    /// <param name="direccion">Dirección de lanzamiento (se normaliza internamente).</param>
    public void Lanzar(Vector2 direccion)
    {
        _rb.linearVelocity = direccion.normalized * _speed;

        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados
    // Esta clase no tiene métodos privados adicionales.
    #endregion

} // class CristalesBoss
  // Marián Navarro