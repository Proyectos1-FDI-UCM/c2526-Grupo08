//---------------------------------------------------------
// Detecta cuando el jugador activa el ascensor con los fusibles necesarios
// y delega el cambio de escena al LevelManager.
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Componente del GameObject del ascensor.
/// Cuando el jugador entra en el trigger con los fusibles necesarios,
/// llama a LevelManager.CompleteLevel() que guarda el checkpoint
/// y carga la siguiente escena.
/// </summary>
public class LevelWin : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Tooltip("Nombre exacto de la escena a cargar al completar el nivel.")]
    [SerializeField] private string nextSceneName = "Level_2";

    [Tooltip("Número de fusibles necesarios para activar el ascensor. (GDD: 3)")]
    [SerializeField] private int requiredFusibles = 3;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobar que quien entra es el jugador
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;

        // Comprobar que lleva los fusibles necesarios
        Inventory inventory = other.GetComponent<Inventory>();
        if (inventory == null) return;

        if (inventory.GetFusibleCount() >= requiredFusibles)
        {
            Debug.Log("[LevelWin] Nivel completado. Cargando siguiente escena...");

            if (LevelManager.HasInstance())
                LevelManager.Instance.CompleteLevel(nextSceneName);
            else
            {
                // Fallback si no hay LevelManager (ejecución directa en editor)
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            Debug.Log($"[LevelWin] Faltan fusibles. Tienes {inventory.GetFusibleCount()}/{requiredFusibles}.");
        }
    }

    #endregion

} // class LevelWin