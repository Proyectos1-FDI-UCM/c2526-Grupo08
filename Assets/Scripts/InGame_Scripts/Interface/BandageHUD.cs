//---------------------------------------------------------
// Muestra en el HUD la cantidad de vendas que tiene el jugador,
// leyendo el contador desde el Inventory cada frame.
// Laura Garay Zubiaguirre
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using TMPro;

/// <summary>
/// Sincroniza el texto del HUD con el número de vendas disponibles
/// en el inventario del jugador (Inventory.GetBandageCount()).
/// </summary>
public class HUDManager : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Referencias")]
    [Tooltip("Inventory del jugador del que se leerá el número de vendas.")]
    [SerializeField] private Inventory playerInventory;

    [Tooltip("Texto de la UI donde se muestra la cantidad de vendas (formato: \"x N\").")]
    [SerializeField] private TextMeshProUGUI bandageText;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cada frame, lee el número de vendas del inventario del jugador
    /// y actualiza el texto del HUD con el formato "x N".
    /// </summary>
    private void Update()
    {
        if (playerInventory != null && bandageText != null)
        {
            int count = playerInventory.GetBandageCount();
            bandageText.text = "x " + count.ToString();
        }
    }

    #endregion

} // class HUDManager (BandageHUD.cs)
  // Laura Garay Zubiaguirre