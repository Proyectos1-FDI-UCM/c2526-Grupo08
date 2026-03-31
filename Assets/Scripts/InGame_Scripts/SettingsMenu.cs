//---------------------------------------------------------
// Gestiona el panel de ajustes (Settings / Opciones) del menú principal.
// Cada parámetro tiene dos botones (+/-) y un TMP_Text que muestra el valor
// actual en tiempo real al pulsar cualquiera de los dos botones.
// Los valores se guardan en GameManager para que persistan entre escenas.
// Alexia Pérez Santana

// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla el panel de ajustes del menú.
/// Cada parámetro (camera shake, follow delay, etc.) se gestiona con
/// un bloque serializado que contiene:
///   - Botón "menos"  → reduce el valor
///   - Botón "más"    → aumenta el valor
///   - TMP_Text       → muestra el valor actual
///
/// Al abrir el panel (OnEnable o llamando a RefrescarUI) todos los textos
/// se sincronizan con los valores actuales de GameManager.
///
/// Para añadir un nuevo parámetro en el futuro:
///   1. Añadir el campo a GameManager (getter + setter).
///   2. Duplicar un bloque [SettingsEntry] en el Inspector.
///   3. Añadir los métodos públicos correspondientes aquí.
///
/// SETUP EN INSPECTOR:
///   · Asignar los botones y textos de cada parámetro.
///   · Conectar los botones OnClick a los métodos de este script.
///   · El objeto con este script debe activarse al pulsar "Opciones"
///     en MenuManager.ActiveOptions().
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    // ---- CLASES DE DATOS ----
    #region Clases de datos

    /// <summary>
    /// Agrupa los elementos de UI de un parámetro ajustable.
    /// Un solo bloque por parámetro en el Inspector.
    /// </summary>
    [System.Serializable]
    public class SettingsEntry
    {
        [Tooltip("Botón para reducir el valor")]
        public Button ButtonMinus;

        [Tooltip("Botón para aumentar el valor")]
        public Button ButtonPlus;

        [Tooltip("TMP_Text que muestra el valor actual")]
        public TMP_Text ValueLabel;
    }

    #endregion

    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Camera Shake")]
    [Tooltip("Intensidad del temblor de cámara. Rango: 0.0 – 1.0, paso: 0.1")]
    [SerializeField] private SettingsEntry ShakeEntry;

    [Tooltip("Valor mínimo permitido para la intensidad de shake")]
    [SerializeField] private float ShakeMin = 0f;

    [Tooltip("Valor máximo permitido para la intensidad de shake")]
    [SerializeField] private float ShakeMax = 1f;

    [Tooltip("Incremento por pulsación para la intensidad de shake")]
    [SerializeField] private float ShakeStep = 0.1f;

    [Header("Follow Delay (retraso de cámara)")]
    [Tooltip("Retraso del seguimiento de cámara. Rango: 0.1 – 1.5, paso: 0.1")]
    [SerializeField] private SettingsEntry FollowDelayEntry;

    [Tooltip("Valor mínimo permitido para el follow delay")]
    [SerializeField] private float FollowDelayMin = 0.1f;

    [Tooltip("Valor máximo permitido para el follow delay")]
    [SerializeField] private float FollowDelayMax = 1.5f;

    [Tooltip("Incremento por pulsación para el follow delay")]
    [SerializeField] private float FollowDelayStep = 0.1f;

    [Header("Formato de número")]
    [Tooltip("Formato de ToString para mostrar el valor (F1 = 1 decimal, F0 = entero)")]
    [SerializeField] private string NumberFormat = "F1";

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        RefrescarUI();
    }

    private void OnEnable()
    {
        // Cada vez que se abre el panel, sincronizar los textos con los valores actuales
        RefrescarUI();
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS — SHAKE ----
    #region Métodos públicos — Camera Shake

    /// <summary>
    /// Reduce la intensidad del shake un paso. Llamar desde el botón "–".
    /// </summary>
    public void ShakeMenos()
    {
        if (!GameManager.HasInstance()) return;

        float nuevo = Mathf.Round((GameManager.Instance.CameraShakeIntensity - ShakeStep) * 100f) / 100f;
        GameManager.Instance.CameraShakeIntensity = Mathf.Clamp(nuevo, ShakeMin, ShakeMax);
        ActualizarLabel(ShakeEntry, GameManager.Instance.CameraShakeIntensity);
    }

    /// <summary>
    /// Aumenta la intensidad del shake un paso. Llamar desde el botón "+".
    /// </summary>
    public void ShakeMas()
    {
        if (!GameManager.HasInstance()) return;

        float nuevo = Mathf.Round((GameManager.Instance.CameraShakeIntensity + ShakeStep) * 100f) / 100f;
        GameManager.Instance.CameraShakeIntensity = Mathf.Clamp(nuevo, ShakeMin, ShakeMax);
        ActualizarLabel(ShakeEntry, GameManager.Instance.CameraShakeIntensity);
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS — FOLLOW DELAY ----
    #region Métodos públicos — Follow Delay

    /// <summary>
    /// Reduce el follow delay un paso. Llamar desde el botón "–".
    /// </summary>
    public void FollowDelayMenos()
    {
        if (!GameManager.HasInstance()) return;

        float nuevo = Mathf.Round((GameManager.Instance.CameraFollowDelay - FollowDelayStep) * 100f) / 100f;
        GameManager.Instance.CameraFollowDelay = Mathf.Clamp(nuevo, FollowDelayMin, FollowDelayMax);
        ActualizarLabel(FollowDelayEntry, GameManager.Instance.CameraFollowDelay);
    }

    /// <summary>
    /// Aumenta el follow delay un paso. Llamar desde el botón "+".
    /// </summary>
    public void FollowDelayMas()
    {
        if (!GameManager.HasInstance()) return;

        float nuevo = Mathf.Round((GameManager.Instance.CameraFollowDelay + FollowDelayStep) * 100f) / 100f;
        GameManager.Instance.CameraFollowDelay = Mathf.Clamp(nuevo, FollowDelayMin, FollowDelayMax);
        ActualizarLabel(FollowDelayEntry, GameManager.Instance.CameraFollowDelay);
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Actualiza todos los labels con los valores actuales de GameManager.
    /// Se llama en OnEnable para sincronizar al abrir el panel.
    /// </summary>
    private void RefrescarUI()
    {
        if (!GameManager.HasInstance()) return;

        ActualizarLabel(ShakeEntry, GameManager.Instance.CameraShakeIntensity);
        ActualizarLabel(FollowDelayEntry, GameManager.Instance.CameraFollowDelay);
    }

    /// <summary>
    /// Actualiza el TMP_Text de un SettingsEntry con el valor formateado.
    /// </summary>
    private void ActualizarLabel(SettingsEntry entry, float value)
    {
        if (entry == null || entry.ValueLabel == null) return;
        entry.ValueLabel.text = value.ToString(NumberFormat);
    }

    #endregion

} // class SettingsMenu