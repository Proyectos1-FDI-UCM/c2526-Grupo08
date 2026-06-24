//---------------------------------------------------------
// Ataque de "Cuchillas" del jefe: muestra un aviso en la posición del
// jugador, espera un instante y lanza 3 proyectiles (CristalesBoss) en
// abanico hacia esa posición.
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Segundo ataque especial del jefe (Raven): ExecuteBladeAttack es llamado
/// por BossPhaseController como uno de los ataques aleatorios. Muestra un
/// aviso visual en la posición actual del jugador y, tras AttackTelegraphDelay
/// segundos, dispara 3 proyectiles CristalesBoss desde _shootOrigin: uno
/// directo a esa posición y los otros dos desviados ±BladeSpreadAngle
/// grados, formando un abanico.
/// AplicarBuffFaseFinal reduce el tiempo de recarga al activarse la Fase 3.
/// </summary>
public class SecondAttackBoss : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Header("Prefabs")]
    [Tooltip("Prefab del proyectil central del abanico (CristalesBoss).")]
    [SerializeField] private CristalesBoss _prefabKnife1;

    [Tooltip("Prefab del proyectil desviado +BladeSpreadAngle grados (CristalesBoss).")]
    [SerializeField] private CristalesBoss _prefabKnife2;

    [Tooltip("Prefab del proyectil desviado -BladeSpreadAngle grados (CristalesBoss).")]
    [SerializeField] private CristalesBoss _prefabKnife3;

    [Tooltip("Prefab del aviso visual que aparece en la posición del jugador antes del disparo.")]
    [SerializeField] private GameObject _WarningPrefab;

    [Header("Configuración")]
    [Tooltip("Radio de detección dibujado en el editor (Gizmo de referencia).")]
    [SerializeField] private float _DetectionRange = 10f;

    [Tooltip("Tiempo de recarga entre ráfagas de cuchillas (reservado: el ritmo real lo controla BossPhaseController, ver AplicarBuffFaseFinal).")]
    [SerializeField] private float _tiempoRecarga = 3f;

    [Tooltip("Punto desde el que se instancian los proyectiles de cuchillas.")]
    [SerializeField] private Transform _shootOrigin;

    [Header("Abanico de disparo")]
    [Tooltip("Ángulo en grados de desviación de los proyectiles laterales respecto al central.")]
    [SerializeField] private float BladeSpreadAngle = 45f;

    [Tooltip("Tiempo en segundos entre mostrar el aviso y disparar los proyectiles.")]
    [SerializeField] private float AttackTelegraphDelay = 1.0f;

    [Header("Referencia al jugador")]
    [Tooltip("GameObject del jugador, usado como objetivo del ataque.")]
    [SerializeField] private GameObject _player;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Transform del jugador, cacheado en Start a partir de _player.</summary>
    private Transform _jugador;

    /// <summary>Instancia actual del aviso visual, destruida al disparar.</summary>
    private GameObject _avisoActual;

    /// <summary>Posición del jugador en el momento de iniciar el ataque, hacia la que se dispara.</summary>
    private Vector3 _posicionObjetivo;

    /// <summary>True mientras el aviso está mostrado y el disparo está pendiente (evita solapar ataques).</summary>
    private bool _preparandoAtaque;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Cachea el Transform del jugador a partir de _player.
    /// </summary>
    void Start()
    {
        if (_player != null) _jugador = _player.transform;
    }

    /// <summary>
    /// Dibuja en el editor el radio de detección de referencia.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _DetectionRange);
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Inicia el ataque de cuchillas: si no hay otro ataque en curso,
    /// muestra el aviso en la posición actual del jugador y programa
    /// el disparo (Disparar) tras AttackTelegraphDelay segundos.
    /// </summary>
    public void ExecuteBladeAttack()
    {
        if (_jugador == null || _preparandoAtaque) return;

        _preparandoAtaque = true;
        _posicionObjetivo = _jugador.position;

        // Crea el triángulo rojo de aviso
        if (_WarningPrefab != null)
        {
            _avisoActual = Instantiate(_WarningPrefab, _posicionObjetivo, Quaternion.identity);
        }

        // Esperamos AttackTelegraphDelay segundos antes de disparar
        Invoke(nameof(Disparar), AttackTelegraphDelay);
    }

    /// <summary>
    /// Aplica el buff de la Fase 3 (Enrage): reduce el tiempo de recarga
    /// entre ráfagas dividiéndolo por multiplicador.
    /// </summary>
    /// <param name="multiplicador">Factor de potenciación de la Fase 3.</param>
    public void AplicarBuffFaseFinal(float multiplicador)
    {
        _tiempoRecarga /= multiplicador;
        Debug.Log("[SecondAttackBoss] Buff de cadencia aplicado.");
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    /// <summary>
    /// Destruye el aviso visual e instancia los 3 proyectiles de cuchillas
    /// desde _shootOrigin: uno hacia _posicionObjetivo y los otros dos
    /// desviados ±BladeSpreadAngle grados.
    /// </summary>
    private void Disparar()
    {
        if (_avisoActual != null)
        {
            Destroy(_avisoActual);
            _avisoActual = null;
        }

        Vector2 direccion1 = (_posicionObjetivo - _shootOrigin.position).normalized;
        Vector2 dir2 = Quaternion.AngleAxis(BladeSpreadAngle, Vector3.forward) * direccion1;
        Vector2 dir3 = Quaternion.AngleAxis(-BladeSpreadAngle, Vector3.forward) * direccion1;

        CristalesBoss c1 = Instantiate(_prefabKnife1, _shootOrigin.position, Quaternion.identity);
        CristalesBoss c2 = Instantiate(_prefabKnife2, _shootOrigin.position, Quaternion.AngleAxis(BladeSpreadAngle, direccion1.normalized));
        CristalesBoss c3 = Instantiate(_prefabKnife3, _shootOrigin.position, Quaternion.AngleAxis(-BladeSpreadAngle, direccion1.normalized));

        c1.Lanzar(direccion1.normalized);
        c2.Lanzar(dir2);
        c3.Lanzar(dir3);

        _preparandoAtaque = false;
    }

    #endregion

} // class SecondAttackBoss
  // Marián Navarro Santoyo