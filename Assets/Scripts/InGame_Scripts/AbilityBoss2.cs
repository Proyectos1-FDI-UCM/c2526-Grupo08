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
public class AbilityBoss2 : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private Transform spawnPointL;
    [SerializeField] private Transform spawnPointR;
    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados


    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {

    }

    void Update()
    {

    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Activa o desactiva la generación de cristales.
    /// </summary>
    /// 
    public void SummonMinions()
    {
        if (minionPrefab != null && spawnPointL != null && spawnPointR != null)
        {
            Instantiate(minionPrefab, spawnPointL.position, Quaternion.identity);
            Instantiate(minionPrefab, spawnPointR.position, Quaternion.identity);
            Debug.Log("¡Esbirros invocados en los puntos laterales!");
        }
        else
        {
            Debug.LogWarning("Faltan referencias en AbilityBoss2 (Prefab o SpawnPoints)");
        }
    }

    public void ExecuteSummoning()
    {
        Debug.Log("Raven está invocando ayuda...");
        SummonMinions(); // <--- Llamamos a la lógica real de instanciar
    }
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados


    #endregion
}
