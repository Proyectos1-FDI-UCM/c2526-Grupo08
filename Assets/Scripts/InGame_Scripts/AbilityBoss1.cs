//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Responsable de la creación de este archivo
// No way down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la primera habilidad del Boss: invoca cristales en posiciones aleatorias 
/// tras mostrar un indicador visual de advertencia dentro de un área rectangular definida.
/// </summary>
[AddComponentMenu("Scripts/Boss/AbilityBoss1")] // Para encontrarlo fácil en 'Add Component'
public class AbilityBoss1 : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Prefabs y Configuración")]
    [SerializeField] private GameObject WarningPrefab;
    [SerializeField] private GameObject CrystalPrefab;
    [SerializeField] private float TelegraphDuration = 1.0f;
    [SerializeField] private int CrystalDamage = 30;

    [Header("Lógica de Spawning")]
    [SerializeField] private bool IsActive = false; // Solo permite atacar si la fase es correcta
    [SerializeField] private Vector2 SpawnRange = new Vector2(10f, 10f);
    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    private class ActiveAttack
    {
        public GameObject WarningInstance;
        public Vector3 Position;
        public float Timer;
    }

    private List<ActiveAttack> _pendingAttacks = new List<ActiveAttack>();

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {

    }

    void Update()
    {
        for (int i = _pendingAttacks.Count - 1; i >= 0; i--)
        {
            ActiveAttack attack = _pendingAttacks[i];
            attack.Timer += Time.deltaTime;

            if (attack.Timer >= TelegraphDuration)
            {
                ExecuteAttack(attack);
                _pendingAttacks.RemoveAt(i);
            }
        }
    }
   

    // Dibuja el área de ataque en el editor para que sea fácil de ajustar.
    

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Activa o desactiva la generación de cristales.
    /// </summary>
    public void SetAbilityActive(bool state)
    {
        IsActive = state;
    }
    public void ExecuteGroundCrystals()
    {
        // Solo disparamos si la fase actual lo permite
        if (IsActive)
        {
            SpawnRandomCrystal();
        }
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawCube(transform.position, new Vector3(SpawnRange.x, SpawnRange.y, 0.1f));
    }
    private void SpawnRandomCrystal()
    {
        // Calculamos posición aleatoria
        float randomX = Random.Range(-SpawnRange.x / 2f, SpawnRange.x / 2f);
        float randomY = Random.Range(-SpawnRange.y / 2f, SpawnRange.y / 2f);
        Vector3 randomPos = transform.position + new Vector3(randomX, randomY, 0f);

        // Creamos el aviso y lo metemos en la lista de "pendientes"
        if (WarningPrefab != null)
        {
            ActiveAttack newAttack = new ActiveAttack
            {
                Position = randomPos,
                Timer = 0f,
                WarningInstance = Instantiate(WarningPrefab, randomPos, Quaternion.identity)
            };
            _pendingAttacks.Add(newAttack);
        }
    }

    private void ExecuteAttack(ActiveAttack attack)
    {
        if (attack.WarningInstance != null)
        {
            Destroy(attack.WarningInstance);
        }

        if (CrystalPrefab != null)
        {
            Instantiate(CrystalPrefab, attack.Position, Quaternion.identity);
            // Aquí podrías asignar el daño al script del cristal si lo necesitas
        }
    }

    #endregion
}