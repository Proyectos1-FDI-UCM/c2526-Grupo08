//-------------------------------------------------------------------------
// Archivo: AbilityBoss1.cs
// Descripción: Sistema de gestión para la primera habilidad especial del jefe.
//              Controla el spawn de cristales con pre-aviso visual.
// Responsable: Laura Garay
// Proyecto: No way down Proyectos 1 - Curso 2025-26
//-------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

// Este es el script para la primera habilidad del boss (los cristales del suelo)
[AddComponentMenu("Scripts/Boss/AbilityBoss1")]
public class AbilityBoss1 : MonoBehaviour
{
    // ---- COSAS PARA ARRASTRAR AL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Cosas que necesito")]
    [SerializeField] private GameObject WarningPrefab; // El circulito de aviso
    [SerializeField] private GameObject CrystalPrefab; // El cristal que hace daño

    [Header("Ajustes del ataque")]
    [SerializeField] private float TelegraphDuration = 1.0f; // Cuanto tarda en salir el cristal
    [SerializeField] private int CrystalDamage = 30; // El daño que quita

    [Header("Donde aparecen")]
    [SerializeField] private bool IsActive = true; // Para que no ataque si no toca
    [SerializeField] private Vector2 SpawnRange = new Vector2(10f, 10f); // El tamaño de la zona

    #endregion

    // ---- VARIABLES INTERNAS ----
    #region Atributos Privados

    // Clase para guardar los datos de cada ataque que se está preparando
    class ActiveAttack
    {
        public GameObject WarningInstance; // El clon del aviso
        public Vector3 Position;           // Donde va a salir
        public float Timer;                // El tiempo que lleva puesto
    }

    // Una lista para controlar todos los cristales que van a salir
    private List<ActiveAttack> _pendingAttacks = new List<ActiveAttack>();

    #endregion

    // ---- FUNCIONES DE UNITY ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        // De momento no necesito poner nada aquí
    }

    void Update()
    {
        // Miramos todos los ataques que están esperando en la lista
        // Lo hago al revés para que no pete al borrar elementos
        for (int i = _pendingAttacks.Count - 1; i >= 0; i--)
        {
            ActiveAttack attack = _pendingAttacks[i];

            // Vamos sumando el tiempo que pasa
            attack.Timer += Time.deltaTime;

            // Si ya ha pasado el tiempo de aviso, soltamos el cristal
            if (attack.Timer >= TelegraphDuration)
            {
                ExecuteAttack(attack);
                _pendingAttacks.RemoveAt(i); // Lo quitamos de la lista porque ya ha salido
            }
        }
    }

    #endregion

    // ---- FUNCIONES QUE LLAMO DESDE OTROS SCRIPTS ----
    #region Métodos públicos

    // Para activar o desactivar el ataque según la fase del boss
    public void SetAbilityActive(bool state)
    {
        IsActive = state;
    }

    // Esta es la función que hay que llamar para que empiece a tirar cristales
    public void ExecuteGroundCrystals()
    {
        // Si el boss está activo, tiramos uno aleatorio
        if (IsActive)
        {
            SpawnRandomCrystal();
        }
    }

    #endregion

    // ---- MI LÓGICA PROPIA ----
    #region Métodos Privados

    // Para ver el cuadradito rojo en el editor y saber por donde salen los cristales
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawCube(transform.position, new Vector3(SpawnRange.x, SpawnRange.y, 0.1f));
    }

    // Función para elegir un sitio al azar y poner el aviso
    private void SpawnRandomCrystal()
    {
        // Calculo un sitio random dentro del rango que hemos puesto
        float randomX = Random.Range(-SpawnRange.x / 2f, SpawnRange.x / 2f);
        float randomY = Random.Range(-SpawnRange.y / 2f, SpawnRange.y / 2f);
        Vector3 randomPos = transform.position + new Vector3(randomX, randomY, 0f);

        // Si tenemos el prefab del aviso, lo creamos y lo metemos en la lista
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

    // Aquí es donde el aviso desaparece y sale el cristal de verdad
    private void ExecuteAttack(ActiveAttack attack)
    {
        // Borramos el aviso de la escena
        if (attack.WarningInstance != null)
        {
            Destroy(attack.WarningInstance);
        }

        // Creamos el cristal en el sitio que habíamos guardado
        if (CrystalPrefab != null)
        {
            GameObject crystal = Instantiate(CrystalPrefab, attack.Position, Quaternion.identity);

            // TODO: Falta pasarle el CrystalDamage al script del propio cristal

            // Intentamos obtener el script Crystal para pasarle el daño
            if (crystal.TryGetComponent<Crystal>(out Crystal crystalScript))
            {
                crystalScript.damage = CrystalDamage;
            }
        }
    }

    #endregion
}