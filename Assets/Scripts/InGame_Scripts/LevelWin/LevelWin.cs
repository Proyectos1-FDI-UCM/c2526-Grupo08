//---------------------------------------------------------
// Detecta cuando el jugador activa el ascensor con los objetos necesarios
// y delega el cambio de escena al LevelManager.
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Componente del GameObject del ascensor.
/// Cuando el jugador entra en el trigger con la cantidad necesaria del objeto
/// requerido (fusibles o tarjetas), llama a LevelManager.CompleteLevel()
/// que guarda el checkpoint y carga la siguiente escena.
/// Si le faltan objetos, muestra un mensaje de ayuda por consola.
/// </summary>
public class LevelWin : MonoBehaviour
{
    // ---- TIPOS ----
    #region Tipos

    /// <summary>Tipo de objeto que requiere el ascensor para activarse.</summary>
    public enum RequirementType { Fusibles, Cards }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Scene Settings")]
    [Tooltip("Nombre de la escena que se cargará al activar el ascensor.")]
    [SerializeField] private string nextSceneName = "Level_2";

    [Header("Win Conditions")]
    [Tooltip("Tipo de objeto que necesita el jugador en ESTE nivel para activar el ascensor.")]
    [SerializeField] private RequirementType requiredItem;

    [Tooltip("Cantidad del objeto requerido para que el ascensor funcione.")]
    [SerializeField] private int requiredAmount = 3;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Si el jugador entra en el trigger del ascensor con los objetos necesarios,
    /// completa el nivel; si no, muestra cuántos le faltan.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        Inventory inventory = other.GetComponent<Inventory>();
        if (inventory == null) { return; }

        int currentCount = 0;

        if (requiredItem == RequirementType.Fusibles)
            currentCount = inventory.GetFusibleCount();
        else if (requiredItem == RequirementType.Cards)
            currentCount = inventory.GetCardCount();

        if (currentCount >= requiredAmount)
        {
            Debug.Log($"[LevelWin] Ascensor activado con {requiredItem}.");

            if (LevelManager.HasInstance())
                LevelManager.Instance.CompleteLevel(nextSceneName);
            else
                SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log($"[LevelWin] Necesitas {requiredAmount} {requiredItem}. Tienes {currentCount}.");
        }
    }

    #endregion

} // class LevelWin
  // Marián Navarro Santoyo