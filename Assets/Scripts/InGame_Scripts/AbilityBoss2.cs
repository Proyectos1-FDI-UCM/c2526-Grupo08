//-------------------------------------------------------------------------
// Archivo: AbilityBoss2.cs
// Descripción: Script para que el boss invoque súbditos (minions)
//              durante la pelea en los puntos de spawn.
// Responsable: Laura Garay
// Proyecto: No way down Proyectos 1 - Curso 2025-26
//-------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

// Este es el script para la segunda habilidad: invocar bichos a los lados
[AddComponentMenu("Scripts/Boss/AbilityBoss2")]
public class AbilityBoss2 : MonoBehaviour
{
    // ---- COSAS QUE TENGO QUE ARRASTRAR AL UNITY ----
    #region Atributos del Inspector (serialized fields)

    [Header("Ajustes de Invocación")]
    [SerializeField] private GameObject minionPrefab; // El bicho que va a salir

    [Header("Puntos de salida")]
    [SerializeField] private Transform spawnPointL; // Punto de la izquierda
    [SerializeField] private Transform spawnPointR; // Punto de la derecha

    #endregion

    // ---- VARIABLES INTERNAS (POR SI HACEN FALTA LUEGO) ----
    #region Atributos Privados

    // De momento vacío, pero lo dejo por si hay que guardar una lista de minions

    #endregion

    // ---- FUNCIONES DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    #endregion

    // ---- FUNCIONES QUE SE LLAMAN DESDE FUERA ----
    #region Métodos públicos

    // Esta función hace el trabajo sucio de crear los minions
    public void SummonMinions()
    {
        // Primero miro si he arrastrado todo al inspector para que no de error
        if (minionPrefab != null && spawnPointL != null && spawnPointR != null)
        {
            // Creamos uno en la izquierda y otro en la derecha
            Instantiate(minionPrefab, spawnPointL.position, Quaternion.identity);
            Instantiate(minionPrefab, spawnPointR.position, Quaternion.identity);

            // Un mensaje en consola para saber que ha funcionado
            Debug.Log("¡Esbirros invocados en los puntos laterales!");
        }
        else
        {
            // Si falta algo, aviso por consola para que no nos volvamos locos buscando el fallo
            Debug.LogWarning("Faltan referencias en AbilityBoss2 (Prefab o SpawnPoints)");
        }
    }

    // Esta es la función principal que activará el ataque
    public void ExecuteSummoning()
    {
        // Raven es el nombre del boss, supongo
        Debug.Log("Raven está invocando ayuda...");

        // Llamamos a la lógica de arriba para que salgan los bichos
        SummonMinions();
    }

    #endregion

    // ---- FUNCIONES EXTRA ----
    #region Métodos Privados

    // Aquí irían las cosas de apoyo si el ataque fuera más complejo

    #endregion
}