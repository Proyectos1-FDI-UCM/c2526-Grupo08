//---------------------------------------------------------
// Activador que entrega al jugador una llave genérica al entrar en su
// trigger, y se autodestruye. Usado, por ejemplo, junto a una puerta para
// simular que se abre y deja una llave accesible.
// Marián Navarro
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Al entrar el jugador en el trigger de este objeto, le añade una llave
/// genérica a su Inventory (CollectKey) y destruye este GameObject.
/// </summary>
public class OpentheDoor : MonoBehaviour
{
    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Si lo que entra en el trigger tiene componente Inventory (el jugador),
    /// le añade una llave genérica y destruye este GameObject.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Inventory inventory = collision.GetComponent<Inventory>();

        if (inventory != null)
        {
            inventory.CollectKey();
            Destroy(gameObject);
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

} // class OpentheDoor
  // Marián Navarro