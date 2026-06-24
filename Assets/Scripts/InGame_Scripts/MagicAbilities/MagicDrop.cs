//---------------------------------------------------------
// Objeto recolectable que restaura puntos de magia al jugador.
// Celia García Riaza
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Al entrar el jugador en el trigger, aumenta su magia en MagicPoint
/// puntos llamando a Magic.IncreaseMagicAmount() y se destruye.
/// Soltado por los enemigos normales al morir (Health.Die).
/// </summary>
public class MagicDrop : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Tooltip("Puntos de magia que se restauran al recoger este drop.")]
    [SerializeField] private int MagicPoint = 8;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Si lo que entra en el trigger tiene componente Magic (el jugador),
    /// le restaura MagicPoint puntos de magia y destruye este GameObject.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        Magic magic = other.GetComponent<Magic>();
        if (magic != null)
        {
            magic.IncreaseMagicAmount(MagicPoint);
            Destroy(gameObject);
        }
    }

    #endregion

} // class MagicDrop
  // Celia García Riaza