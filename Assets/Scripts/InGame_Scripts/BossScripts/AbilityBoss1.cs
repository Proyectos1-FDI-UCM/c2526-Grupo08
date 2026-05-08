//-------------------------------------------------------------------------//-------------------------------------------------------------------------
// Archivo: AbilityBoss1.cs
// Descripción: Sistema de gestión para la primera habilidad especial del jefe.
//              Controla el spawn de cristales con pre-aviso visual.
// Responsable: Laura Garay
// Proyecto: No way down Proyectos 1 - Curso 2025-26
//-------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Scripts/Boss/AbilityBoss1")]
public class AbilityBoss1 : MonoBehaviour
{
    #region Atributos del Inspector
    [Header("Prefabs")]
    [SerializeField] private GameObject WarningPrefab;
    [SerializeField] private GameObject CrystalPrefab;

    [Header("Configuración del Tiempo")]
    [Tooltip("Cada cuántos segundos aparece un cristal nuevo automáticamente")]
    [SerializeField] private float SpawnRate = 1.5f;
    [Tooltip("Cuánto tarda el cristal en salir tras el aviso")]
    [SerializeField] private float TelegraphDuration = 1.0f;

    [Header("Área de Juego")]
    [SerializeField] private bool IsActive = false; // Empieza en falso hasta que el Controller lo active
    [SerializeField] private Vector2 SpawnRange = new Vector2(10f, 10f);
    [SerializeField] private Transform CenterOfAttackArea;

    private Vector3 _fixedAreaCenter;
    private float _spawnTimer; // Reloj interno para el SpawnRate
    #endregion

    private List<ActiveAttack> _pendingAttacks = new List<ActiveAttack>();

    class ActiveAttack
    {
        public GameObject WarningInstance;
        public Vector3 Position;
        public float Timer;
    }

    private void Start()
    {
        // Forzamos que esté desactivado al arrancar el juego
        IsActive = false;
        _spawnTimer = 0f;

        // Lógica del centro que ya tenías
        if (CenterOfAttackArea != null)
            _fixedAreaCenter = CenterOfAttackArea.position;
        else
            _fixedAreaCenter = transform.position;
    }

    private void Update()
    {
        // 1. Si no está activa, no hace nada
        if (!IsActive) return;

        // 2. Generador automático de avisos (Esto ya lo tenías)
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= SpawnRate)
        {
            SpawnRandomCrystal();
            _spawnTimer = 0f;
        }

        // 3. --- ¡ESTO ES LO QUE TE FALTA Y DEBES PEGAR! ---
        // Revisamos la lista de ataques pendientes para ver si el aviso ha terminado
        for (int i = _pendingAttacks.Count - 1; i >= 0; i--)
        {
            ActiveAttack attack = _pendingAttacks[i];
            attack.Timer += Time.deltaTime;

            // Si el tiempo del aviso (TelegraphDuration) ha pasado...
            if (attack.Timer >= TelegraphDuration)
            {
                // ¡LLAMAMOS A LA FUNCIÓN QUE CREA EL CRISTAL!
                ExecuteAttack(attack);

                // Lo quitamos de la lista para que no se repita
                _pendingAttacks.RemoveAt(i);
            }
        }
    }

    public void SetAbilityActive(bool state)
    {
        IsActive = state;
        _spawnTimer = 0f; // Reiniciamos el tiempo al activar la fase
    }

    private void SpawnRandomCrystal()
    {
        float randomX = Random.Range(-SpawnRange.x / 2f, SpawnRange.x / 2f);
        float randomY = Random.Range(-SpawnRange.y / 2f, SpawnRange.y / 2f);
        Vector3 randomPos = _fixedAreaCenter + new Vector3(randomX, randomY, 0f);

        if (WarningPrefab != null)
        {
            _pendingAttacks.Add(new ActiveAttack
            {
                Position = randomPos,
                Timer = 0f,
                WarningInstance = Instantiate(WarningPrefab, randomPos, Quaternion.identity)
            });
        }
    }

    private void ExecuteAttack(ActiveAttack attack)
    {
        if (attack.WarningInstance != null) Destroy(attack.WarningInstance);
        if (CrystalPrefab != null)
        {
            GameObject crystal = Instantiate(CrystalPrefab, attack.Position, Quaternion.identity);
            // Aquí podrías pasarle el daño si el script del cristal lo requiere
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 pos = Application.isPlaying ? _fixedAreaCenter : (CenterOfAttackArea != null ? CenterOfAttackArea.position : transform.position);
        Gizmos.color = new Color(0.5f, 0, 1f, 0.3f); // Morado transparente
        Gizmos.DrawCube(pos, new Vector3(SpawnRange.x, SpawnRange.y, 0.1f));
    }
}