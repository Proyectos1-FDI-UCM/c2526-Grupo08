//---------------------------------------------------------
// Detecta cuando el jugador activa el ascensor con los fusibles necesarios
// y delega el cambio de escena al LevelManager.
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Componente del GameObject del ascensor.
/// Cuando el jugador entra en el trigger con los fusibles necesarios,
/// llama a LevelManager.CompleteLevel() que guarda el checkpoint
/// y carga la siguiente escena.
/// </summary>
public class LevelWin : MonoBehaviour
{
    public enum RequirementType { Fusibles, Cards }
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector


    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "Level_2";

    [Header("Win Conditions")]
    [Tooltip("Selecciona qué objeto necesita el jugador en ESTE nivel")]
    [SerializeField] private RequirementType requiredItem;

    [Tooltip("Cantidad necesaria para que el ascensor funcione")]
    [SerializeField] private int requiredAmount = 3;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Buscamos el inventario en el objeto que ha entrado (el jugador)
        Inventory inventory = other.GetComponent<Inventory>();
        if (inventory == null) return;

        int currentCount = 0;

        // 2. Dependiendo de lo que elegiste en el Inspector, consultamos una cosa u otra
        if (requiredItem == RequirementType.Fusibles)
        {
            currentCount = inventory.GetFusibleCount();
        }
        else if (requiredItem == RequirementType.Cards)
        {
            currentCount = inventory.GetCardCount();
        }

        // 3. Verificamos si el jugador cumple el requisito
        if (currentCount >= requiredAmount)
        {
            Debug.Log($"[LevelWin] Door opened with {requiredItem}!");

            if (LevelManager.HasInstance())
                LevelManager.Instance.CompleteLevel(nextSceneName);
            else
                SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Mensaje de ayuda si le faltan objetos
            Debug.Log($"[LevelWin] You need {requiredAmount} {requiredItem}. You have {currentCount}.");
        }
    }
}


    #endregion

 // class LevelWin