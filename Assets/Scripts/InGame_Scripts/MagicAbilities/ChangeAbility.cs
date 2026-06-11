//---------------------------------------------------------
// Gestiona el cambio de habilidad activa del jugador.
// Carlos Mesa Torres
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cambia entre las habilidades disponibles del jugador (cargada, multidireccional, explosiva).
/// Solo puede seleccionarse una habilidad desbloqueada en el Inventory.
/// </summary>
public class ChangeAbility : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Habilidades (scripts)")]
    [Tooltip("Script de la habilidad multidireccional.")]
    [SerializeField] private MonoBehaviour _multiAbility;

    [Tooltip("Script de la habilidad explosiva.")]
    [SerializeField] private MonoBehaviour _explosiveAbility;

    [Tooltip("Script de la habilidad cargada (siempre disponible).")]
    [SerializeField] private MonoBehaviour _chargedattackAbility;

    [Header("Iconos de UI (uno por habilidad)")]
    [Tooltip("Array de GameObjects de icono HUD. Índice 0=cargada, 1=multi, 2=explosiva.\n" +
             "Deben estar asignados los 3 slots.")]
    [SerializeField] private GameObject[] _abilityImage;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Número total de habilidades disponibles (cargada, multi, explosiva).</summary>
    private const int AbilityCount = 3;

    /// <summary>Índice de la habilidad cargada (siempre disponible).</summary>
    private const int ChargedIndex = 0;

    /// <summary>Índice de la habilidad multidireccional.</summary>
    private const int MultiIndex = 1;

    /// <summary>Índice de la habilidad explosiva.</summary>
    private const int ExplosiveIndex = 2;

    private InputAction _changeAbilityAction;
    private int _currentIndex = 0;
    private Inventory _inventory;

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        _changeAbilityAction = InputSystem.actions.FindAction("ChangeAbility");

        if (_changeAbilityAction == null)
        {
            Debug.LogError("[ChangeAbility] Acción 'ChangeAbility' no encontrada en el InputSystem.");
            enabled = false;
            return;
        }

        // CORRECCIÓN: validar array antes de indexarlo
        if (_abilityImage == null || _abilityImage.Length == 0)
        {
            Debug.LogError("[ChangeAbility] El array _abilityImage está vacío o no asignado. " +
                           "Asigna los 3 iconos de habilidad en el Inspector.");
            enabled = false;
            return;
        }

        // Desactivar todos los iconos y activar solo el inicial
        for (int i = 0; i < _abilityImage.Length; i++)
        {
            if (_abilityImage[i] != null)
                _abilityImage[i].SetActive(false);
        }

        // Activar el índice inicial solo si es válido
        if (_currentIndex < _abilityImage.Length && _abilityImage[_currentIndex] != null)
            _abilityImage[_currentIndex].SetActive(true);

        _inventory = GetComponent<Inventory>();

        _changeAbilityAction.Enable();
        UpdateAbilities();
    }

    private void Update()
    {
        if (_changeAbilityAction.WasPressedThisFrame())
            SwitchAbility();
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Avanza al siguiente índice de habilidad de forma cíclica y se detiene
    /// en la primera que esté desbloqueada. Como la cargada (índice 0) siempre
    /// está desbloqueada, el bucle siempre termina como máximo tras AbilityCount
    /// iteraciones.
    /// </summary>
    private void SwitchAbility()
    {
        for (int i = 0; i < AbilityCount; i++)
        {
            _currentIndex = (_currentIndex + 1) % AbilityCount;

            if (IsAbilityUnlocked(_currentIndex))
            {
                UpdateAbilities();
                return;
            }
        }
    }

    private bool IsAbilityUnlocked(int index)
    {
        switch (index)
        {
            case ChargedIndex: return true; // cargada siempre disponible
            case MultiIndex: return _inventory != null && _inventory.HasMultiAbility;
            case ExplosiveIndex: return _inventory != null && _inventory.HasExplosiveAbility;
        }
        return false;
    }

    private void UpdateAbilities()
    {
        if (_chargedattackAbility != null)
            _chargedattackAbility.enabled = (_currentIndex == 0);

        if (_multiAbility != null)
            _multiAbility.enabled = (_currentIndex == 1 && IsAbilityUnlocked(1));

        if (_explosiveAbility != null)
            _explosiveAbility.enabled = (_currentIndex == 2 && IsAbilityUnlocked(2));

        UpdateAbilityUI();
    }

    private void UpdateAbilityUI()
    {
        for (int i = 0; i < _abilityImage.Length; i++)
        {
            if (_abilityImage[i] != null)
                _abilityImage[i].SetActive(i == _currentIndex);
        }
    }

    #endregion

} // class ChangeAbility
  // Carlos Mesa Torres