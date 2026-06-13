//---------------------------------------------------------
// Bala del ataque explosivo: al colisionar con cualquier objeto,
// aplica daño en área a todo lo que esté dentro de su radio de
// explosión y en la capa indicada, y luego se destruye.
// Carlos Mesa Torres
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Controla la explosión de la bala del ataque "Explosivo".
/// Al entrar en contacto con cualquier collider, busca todos los
/// objetos dentro de ExplosionRadius que pertenezcan a DamageLayer
/// y, si tienen componente Health, les aplica Damage puntos de daño.
/// Después se destruye a sí misma.
/// </summary>
public class ExplosiveBullet : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Explosion settings")]
    [Tooltip("Radio del área de explosión en unidades de mundo.")]
    [SerializeField] private float ExplosionRadius = 4f;

    [Tooltip("Puntos de daño que aplica la explosión a cada objetivo válido.")]
    [SerializeField] private int Damage = 250;

    [Tooltip("Capas de física que se comprueban al buscar objetivos dentro del radio de explosión.")]
    [SerializeField] private LayerMask DamageLayer;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Al entrar en contacto con cualquier collider, dispara la explosión.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        Explode();
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Esta clase no expone métodos públicos.
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Detecta todos los colliders dentro de ExplosionRadius que pertenezcan
    /// a DamageLayer, aplica Damage a los que tengan componente Health,
    /// y destruye la bala.
    /// </summary>
    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ExplosionRadius, DamageLayer);

        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(Damage);
            }
        }

        Destroy(gameObject);
    }

    #endregion

} // class ExplosiveBullet
  // Carlos Mesa Torres