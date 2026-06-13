//---------------------------------------------------------
// Gestiona el tiempo de vida de un cristal lanzado por el jefe:
// se destruye solo tras lifeTime segundos, y si golpea al jugador
// antes de eso, le aplica daño.
// Laura Garay Zubiaguirre
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Proyectil de cristal del jefe (usado por AbilityBoss1).
/// Al instanciarse, programa su propia destrucción tras lifeTime segundos.
/// Si colisiona con el jugador antes de destruirse, le aplica damage
/// puntos de daño mediante su componente Health.
/// </summary>
public class Crystal : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Configuración")]
    [Tooltip("Puntos de daño que aplica el cristal al jugador si lo golpea.")]
    [SerializeField] private int damage = 30;

    [Tooltip("Tiempo en segundos antes de que el cristal desaparezca solo.")]
    [SerializeField] private float lifeTime = 2.0f;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Programa la destrucción automática del cristal tras lifeTime segundos.
    /// </summary>
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Si lo que entra en contacto con el cristal es el jugador, le aplica
    /// damage puntos de daño mediante su componente Health.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobamos si lo que hemos tocado es el Jugador
        if (other.gameObject.GetComponent<PlayerMovement>() != null)
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.Damage(damage);

                Debug.Log($"<color=red>¡Cristal golpeó al Jugador!</color> Daño aplicado: {damage}");
            }
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Esta clase no expone métodos públicos.
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados
    // Esta clase no tiene métodos privados.
    #endregion

} // class Crystal
  // Laura Garay Zubiaguirre