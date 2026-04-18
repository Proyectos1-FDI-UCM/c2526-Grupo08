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
    [Tooltip("El prefab del esbirro que va a aparecer.")]
    [SerializeField] private GameObject minionPrefab;

    [Tooltip("Segundos entre cada invocación automática.")]
    [SerializeField] private float summonInterval = 15f;

    [Header("Puntos de Spawn")]
    [SerializeField] private Transform spawnPointL;
    [SerializeField] private Transform spawnPointR;

    #endregion

    // ---- VARIABLES INTERNAS (POR SI HACEN FALTA LUEGO) ----
    #region Atributos Privados

    private GameObject _leftMinion;
    private GameObject _rightMinion;

    private float _timer;
    private bool _isPlayerDetected = false;


    #endregion

    // ---- FUNCIONES DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        _timer = summonInterval; // Inicializamos el temporizador con el intervalo que pusimos en el Inspector
    }

    void Update()
    {
        // Solo empezamos la cuenta atrás si el boss ha visto al jugador
        if (_isPlayerDetected)
        {
            _timer -= Time.deltaTime;

            // Cuando el tiempo llega a cero, intentamos invocar
            if (_timer <= 0f)
            {
                ExecuteSummoning();
                _timer = summonInterval; // Reiniciamos el reloj
            }
        }
    }

    #endregion

    // ---- FUNCIONES QUE SE LLAMAN DESDE FUERA ----
    #region Métodos públicos

    // Esta función hace el trabajo sucio de crear los minions
    public void SummonMinions()
    {
        // Miramos si los esbirros de la oleada anterior siguen vivos
        bool leftAlive = _leftMinion != null;
        bool rightAlive = _rightMinion != null;

        // Si alguno sigue vivo, no invocamos más para no saturar
        if (leftAlive || rightAlive)
        {
            Debug.Log("AbilityBoss2: Todavía hay esbirros vivos. Saltando esta oleada.");
            return;
        }

        if (minionPrefab != null && spawnPointL != null && spawnPointR != null)
        {
            _leftMinion = Instantiate(minionPrefab, spawnPointL.position, Quaternion.identity);
            _rightMinion = Instantiate(minionPrefab, spawnPointR.position, Quaternion.identity);

            Debug.Log("AbilityBoss2: Nueva oleada de esbirros invocada.");
        }
        else
        {
            Debug.LogWarning("AbilityBoss2: ¡Faltan referencias en el Inspector!");
        }
    }

    public void StartCounting()
    {
        _isPlayerDetected = true;
        Debug.Log("AbilityBoss2: Cuenta atrás activada. Esbirros cada " + summonInterval + " segundos.");
    }

    // Esta es la función principal que activará el ataque
    public void ExecuteSummoning()
    {
        SummonMinions();
    }

    #endregion

    // ---- FUNCIONES EXTRA ----
    #region Métodos Privados

    // Aquí irían las cosas de apoyo si el ataque fuera más complejo

    #endregion
}