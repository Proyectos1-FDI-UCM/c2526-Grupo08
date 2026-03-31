//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Responsable de la creación de este archivo
// Nombre del juego
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
    [Tooltip("Prefab del círculo rojo que avisa del ataque.")]
    [SerializeField] private GameObject WarningPrefab;

    [Tooltip("Prefab del cristal que realiza el daño.")]
    [SerializeField] private GameObject CrystalPrefab;

    [Tooltip("Tiempo de anticipación en segundos.")]
    [SerializeField] private float TelegraphDuration = 1.0f;

    [Tooltip("Daño que infligirá el cristal al aparecer.")]
    [SerializeField] private int CrystalDamage = 30;

    [Header("Lógica de Spawning (Generación)")]
    [Tooltip("¿Está la habilidad activa ahora mismo?")]
    [SerializeField] private bool IsActive = false;

    [Tooltip("Tiempo entre cada ráfaga de cristales.")]
    [SerializeField] private float TimeBetweenCrystals = 0.5f;

    // -- CAMBIO AQUÍ: Ahora SpawnRange define el tamaño TOTAL del rectángulo (Ancho, Alto) --
    [Tooltip("Tamaño TOTAL (Ancho en X, Alto en Y) del área rectangular de spawn, centrada en el Boss.")]
    [SerializeField] private Vector2 SpawnRange = new Vector2(10f, 10f);

    [Header("Visualización del Gizmo")]
    [Tooltip("Color del área de ataque en el editor.")]
    [SerializeField] private Color GizmoColor = new Color(1f, 0f, 0f, 0.25f); // Rojo semi-transparente por defecto
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
    private float _spawnTimer;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        // Asegurarnos de que hay color en el gizmo por defecto si se olvida ponerlo
        if (GizmoColor.a == 0) GizmoColor = new Color(1, 0, 0, 0.25f);
    }

    void Update()
    {
        // 1. Lógica de generación aleatoria si la habilidad está activa
        if (IsActive)
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= TimeBetweenCrystals)
            {
                _spawnTimer = 0;
                SpawnRandomCrystal();
            }
        }

        // 2. Lógica de actualización de ataques pendientes (Cuenta atrás)
        // Usamos bucle for inverso para poder eliminar elementos de forma segura.
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

    // -- CAMBIO AQUÍ: El Gizmo ahora es un rectángulo plano y semi-transparente --
    // Dibuja el área de ataque en el editor para que sea fácil de ajustar.
    // Solo aparece cuando el objeto 'Boss (1)' está seleccionado en la Hierarchy.
    private void OnDrawGizmosSelected()
    {
        // Establecer el color, usando transparencia para que no tape al personaje
        Gizmos.color = GizmoColor;

        // --- CÁLCULO DEL RECTÁNGULO DE ÁREA ---

        // El centro del rectángulo es la posición del Boss.
        Vector3 positionCenter = transform.position;

        // El tamaño es directamente SpawnRange (Ancho en X, Alto en Y).
        // Le damos un grosor Z mínimo (0.1f) para que sea visible en 2D ortogonal y en 3D Top-Down.
        Vector3 sizeRect = new Vector3(SpawnRange.x, SpawnRange.y, 0.1f);

        // Dibuja un CUBO SÓLIDO (plano en Z) para visualizar el área.
        Gizmos.DrawCube(positionCenter, sizeRect);

        // Opcional: Dibujar un borde para que se vea mejor incluso en ángulos raros
        Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 1.0f); // Color sólido para el borde
        Gizmos.DrawWireCube(positionCenter, sizeRect);
    }

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

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void SpawnRandomCrystal()
    {
        // Genera una posición aleatoria dentro del rectángulo centrado en el Boss.
        // Dado que SpawnRange es el tamaño total, el rango va de -(mitad) a +(mitad).
        float randomX = Random.Range(-SpawnRange.x / 2f, SpawnRange.x / 2f);
        float randomY = Random.Range(-SpawnRange.y / 2f, SpawnRange.y / 2f);

        // Sumamos la compensación a la posición central del Boss.
        Vector3 randomPos = transform.position + new Vector3(randomX, randomY, 0f);

        LaunchCrystalAttack(randomPos);
    }

    private void LaunchCrystalAttack(Vector3 spawnPosition)
    {
        if (WarningPrefab == null) return; // Evitar errores si no hay prefab asignado

        ActiveAttack newAttack = new ActiveAttack
        {
            Position = spawnPosition,
            Timer = 0f,
            WarningInstance = Instantiate(WarningPrefab, spawnPosition, Quaternion.identity)
        };

        _pendingAttacks.Add(newAttack);
    }

    private void ExecuteAttack(ActiveAttack attack)
    {
        if (attack.WarningInstance != null)
        {
            Destroy(attack.WarningInstance);
        }

        if (CrystalPrefab == null) return; // Evitar errores si no hay prefab asignado
        GameObject crystalObj = Instantiate(CrystalPrefab, attack.Position, Quaternion.identity);

        // Intentamos obtener el script Crystal para pasarle el daño
        if (crystalObj.TryGetComponent<Crystal>(out Crystal crystalScript))
        {
            crystalScript.damage = CrystalDamage;
        }
    }

    #endregion
}