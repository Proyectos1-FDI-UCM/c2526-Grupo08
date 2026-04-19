//---------------------------------------------------------
// Controla el Slider de la barra de vida en el HUD.
// Celia García Riaza
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wrapper del Slider de Unity para la barra de vida.
/// Health llama a SetMaxValue al iniciar para configurar el rango
/// y a SetValue cada vez que la vida cambia.
/// </summary>
public class UIBar : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Tooltip("Slider de UI que representa la barra de vida. " +
             "Si no se asigna, se busca automáticamente en este GameObject.")]
    [SerializeField] private Slider _slider;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Awake()
    {
        if (_slider == null)
        { 
            _slider = GetComponent<Slider>();
        }

        if (_slider == null)
        { 
            Debug.LogError("[HealthBar] No se encontró Slider. Asígnalo en el Inspector."); 
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Configura el rango máximo del Slider.
    /// Llamar una vez desde Health.Start() para que el Slider vaya de 0 a MaxHealth.
    /// </summary>
    public void SetMaxValue(int maxValue)
    {
        if (_slider == null) { return; }
        _slider.minValue = 0;
        _slider.maxValue = maxValue;
        _slider.value = maxValue;
    }

    /// <summary>Actualiza el valor del Slider para reflejar la vida actual.</summary>
    public void SetValue(int value)
    {
        if (_slider == null) { return; }
        _slider.value = value;
    }

    #endregion

} // class UIBar