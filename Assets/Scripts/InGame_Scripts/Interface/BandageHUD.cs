//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Responsable de la creación de este archivo
// Nombre del juego
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Referencias")]
    public Inventory playerInventory; // Arrastra al jugador aquí
    public TextMeshProUGUI bandageText; // Arrastra el texto de la UI aquí

    void Update()
    {
        if (playerInventory != null && bandageText != null)
        {
            // Accedemos al método público que ya creaste
            int count = playerInventory.GetBandageCount();

            // Actualizamos el texto (ejemplo: "x 3")
            bandageText.text = "x " + count.ToString();
        }
    }

} // class BandageHUG 
// Laura Garay
