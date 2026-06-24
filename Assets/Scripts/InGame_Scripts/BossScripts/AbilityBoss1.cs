//---------------------------------------------------------
// Sistema de gestión para la primera habilidad especial del jefe.
// Controla el spawn periódico de cristales con pre-aviso visual
// (telegraph) dentro de un área rectangular alrededor del jefe.
// Laura Garay Zubiaguirre
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Primera habilidad especial del jefe: mientras está activa, cada
/// SpawnRate segundos elige una posición aleatoria dentro de SpawnRange
/// (centrada en CenterOfAttackArea) e instancia un aviso visual
/// (WarningPrefab). Tras TelegraphDuration segundos, sustituye el aviso
/// por un cristal (CrystalPrefab) que puede dañar al jugador.
/// El BossPhaseController activa/desactiva esta habilidad con SetAbilityActive.
/// </summary>
[AddComponentMenu("Scripts/Boss/AbilityBoss1")]
public class AbilityBoss1 : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Prefabs")]
    [Tooltip("Prefab del aviso visual que aparece antes de que salga el cristal.")]
    [SerializeField] private GameObject WarningPrefab;

    [Tooltip("Prefab del cristal que aparece tras el aviso y puede dañar al jugador.")]
    [SerializeField] private GameObject CrystalPrefab;

    [Header("Configuración del Tiempo")]
    [Tooltip("Cada cuántos segundos aparece un cristal nuevo automáticamente.")]
    [SerializeField] private float SpawnRate = 1.5f;

    [Tooltip("Cuánto tarda el cristal en salir tras el aviso, en segundos.")]
    [SerializeField] private float TelegraphDuration = 1.0f;

    [Header("Área de Juego")]
    [Tooltip("Indica si la habilidad está activa. La activa/desactiva BossPhaseController mediante SetAbilityActive.")]
    [SerializeField] private bool IsActive = false;

    [Tooltip("Tamaño (ancho, alto) del área rectangular donde pueden aparecer los cristales.")]
    [SerializeField] private Vector2 SpawnRange = new Vector2(10f, 10f);

    [Tooltip("Centro del área de spawn. Si es null, se usa la posición del jefe al iniciar.")]
    [SerializeField] private Transform CenterOfAttackArea;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Centro real del área de spawn, calculado una vez en Start().</summary>
    private Vector3 _fixedAreaCenter;

    /// <summary>Acumulador de tiempo para el spawn automático de cristales (SpawnRate).</summary>
    private float _spawnTimer;

    /// <summary>Lista de ataques con aviso visual pendientes de convertirse en cristal.</summary>
    private List<ActiveAttack> _pendingAttacks = new List<ActiveAttack>();

    /// <summary>
    /// Representa un ataque pendiente: la instancia del aviso visual, su
    /// posición de spawn y el tiempo transcurrido desde que se mostró.
    /// </summary>
    struct ActiveAttack
    {
        public GameObject WarningInstance;
        public Vector3 Position;
        public float Timer;
    }

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Fuerza la habilidad a desactivada al arrancar y calcula el centro
    /// fijo del área de spawn (CenterOfAttackArea o la posición del jefe).
    /// </summary>
    private void Start()
    {
        IsActive = false;
        _spawnTimer = 0f;

        if (CenterOfAttackArea != null)
            _fixedAreaCenter = CenterOfAttackArea.position;
        else
            _fixedAreaCenter = transform.position;
    }

    /// <summary>
    /// Mientras la habilidad está activa, genera avisos de cristal cada
    /// SpawnRate segundos y convierte en cristal cada aviso cuyo
    /// TelegraphDuration haya terminado.
    /// </summary>
    private void Update()
    {
        if (!IsActive) return;

        // Generador automático de avisos
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= SpawnRate)
        {
            SpawnRandomCrystal();
            _spawnTimer = 0f;
        }

        // Revisamos la lista de ataques pendientes para ver si el aviso ha terminado
        for (int i = _pendingAttacks.Count - 1; i >= 0; i--)
        {
            _pendingAttacks[i] = new ActiveAttack
            {
                WarningInstance = _pendingAttacks[i].WarningInstance,
                Position = _pendingAttacks[i].Position,
                Timer = _pendingAttacks[i].Timer + Time.deltaTime
            };

            if (_pendingAttacks[i].Timer >= TelegraphDuration)
            {
                ExecuteAttack(_pendingAttacks[i]);
                _pendingAttacks.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Dibuja en el editor el área rectangular donde pueden aparecer los cristales.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 pos = Application.isPlaying ? _fixedAreaCenter : (CenterOfAttackArea != null ? CenterOfAttackArea.position : transform.position);
        Gizmos.color = new Color(0.5f, 0, 1f, 0.3f); // Morado transparente
        Gizmos.DrawCube(pos, new Vector3(SpawnRange.x, SpawnRange.y, 0.1f));
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Activa o desactiva esta habilidad. Llamado por BossPhaseController
    /// al entrar o salir de la fase correspondiente. Al activarse, reinicia
    /// el temporizador de spawn.
    /// </summary>
    /// <param name="state">True para activar la habilidad, false para desactivarla.</param>
    public void SetAbilityActive(bool state)
    {
        IsActive = state;
        _spawnTimer = 0f;
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Elige una posición aleatoria dentro de SpawnRange (centrada en
    /// _fixedAreaCenter) e instancia el aviso visual en esa posición,
    /// añadiéndolo a la lista de ataques pendientes.
    /// </summary>
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

    /// <summary>
    /// Destruye el aviso visual de un ataque pendiente e instancia el
    /// cristal real en su posición.
    /// </summary>
    private void ExecuteAttack(ActiveAttack attack)
    {
        if (attack.WarningInstance != null) Destroy(attack.WarningInstance);

        if (CrystalPrefab != null)
        {
            Instantiate(CrystalPrefab, attack.Position, Quaternion.identity);
        }
    }

    #endregion

} // class AbilityBoss1
  // Laura Garay Zubiaguirre