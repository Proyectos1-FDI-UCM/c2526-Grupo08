//-------------------------------------------------------------------------
// Script para que el jefe invoque esbirros (minions) por sus dos flancos
// durante la pelea, de forma periódica mientras la habilidad está activa.
// Laura Garay Zubiaguirre
// No Way Down
// Proyectos 1 - Curso 2025-26
//-------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Segunda habilidad especial del jefe: invoca un esbirro en spawnPointL
/// y otro en spawnPointR. Si la oleada anterior sigue viva, no invoca una
/// nueva. Una vez activada (ActivarInvocacion o StartCounting), repite la
/// invocación automáticamente cada summonInterval segundos mientras la
/// oleada anterior ya ha desaparecido.
/// </summary>
[AddComponentMenu("Scripts/Boss/AbilityBoss2")]
public class AbilityBoss2 : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Ajustes de Invocación")]
    [Tooltip("Prefab del esbirro que aparecerá en cada invocación.")]
    [SerializeField] private GameObject minionPrefab;

    [Tooltip("Segundos entre cada invocación automática.")]
    [SerializeField] private float summonInterval = 15f;

    [Tooltip("Tiempo en segundos antes de la primera invocación al activar la habilidad con ActivarInvocacion.")]
    [SerializeField] private float initialSummonDelay = 0.5f;

    [Header("Puntos de Spawn")]
    [Tooltip("Punto de aparición del esbirro del flanco izquierdo.")]
    [SerializeField] private Transform spawnPointL;

    [Tooltip("Punto de aparición del esbirro del flanco derecho.")]
    [SerializeField] private Transform spawnPointR;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados

    /// <summary>Esbirro invocado en el flanco izquierdo en la última oleada.</summary>
    private GameObject _leftMinion;

    /// <summary>Esbirro invocado en el flanco derecho en la última oleada.</summary>
    private GameObject _rightMinion;

    /// <summary>Cuenta atrás hasta la siguiente invocación automática.</summary>
    private float _timer;

    /// <summary>Indica si la cuenta atrás de invocación está activa.</summary>
    private bool _isPlayerDetected = false;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Inicializa el temporizador con el intervalo de invocación configurado.
    /// </summary>
    private void Start()
    {
        _timer = summonInterval;
    }

    /// <summary>
    /// Mientras la cuenta atrás está activa, descuenta el tiempo y, al
    /// llegar a 0, invoca una nueva oleada y reinicia el temporizador.
    /// </summary>
    void Update()
    {
        if (_isPlayerDetected)
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                ExecuteSummoning();
                _timer = summonInterval;
            }
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Activa la cuenta atrás de invocación y la fuerza a que la primera
    /// oleada salga casi de inmediato (initialSummonDelay segundos).
    /// Llamado por BossPhaseController al entrar en la fase correspondiente.
    /// </summary>
    public void ActivarInvocacion()
    {
        _isPlayerDetected = true;
        _timer = initialSummonDelay;
        Debug.Log("AbilityBoss2: ¡Recibida orden de activación de minions!");
    }

    /// <summary>
    /// Invoca una nueva oleada de esbirros (uno por cada flanco) si la
    /// oleada anterior ya ha desaparecido y hay referencias asignadas
    /// en el Inspector.
    /// </summary>
    public void SummonMinions()
    {
        bool leftAlive = _leftMinion != null;
        bool rightAlive = _rightMinion != null;

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

    /// <summary>
    /// Activa la cuenta atrás de invocación sin forzar una invocación
    /// inmediata (a diferencia de ActivarInvocacion).
    /// </summary>
    public void StartCounting()
    {
        _isPlayerDetected = true;
        Debug.Log("AbilityBoss2: Cuenta atrás activada. Esbirros cada " + summonInterval + " segundos.");
    }

    /// <summary>
    /// Punto de entrada principal del ataque: invoca una nueva oleada de esbirros.
    /// </summary>
    public void ExecuteSummoning()
    {
        SummonMinions();
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados
    // Esta clase no tiene métodos privados.
    #endregion

} // class AbilityBoss2
  // Laura Garay Zubiaguirre