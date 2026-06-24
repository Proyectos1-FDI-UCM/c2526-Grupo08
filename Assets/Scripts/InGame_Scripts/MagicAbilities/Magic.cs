//---------------------------------------------------------
// Gestiona los puntos de magia del jugador: máximo, consumo,
// regeneración y el flash de color al ganar magia.
// Celia García Riaza
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Componente de magia del jugador. Mantiene la magia actual entre 0 y
/// MaxMagic, actualiza la barra de UI asociada (MagicBar) y muestra un
/// breve flash de color cuando se gana magia.
/// </summary>
public class Magic : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)

    [Tooltip("Magia máxima. (GDD: jugador 60)")]
    [SerializeField] private int MaxMagic = 60;

    [Tooltip("Barra de UI que muestra la magia en pantalla. Asignar desde el Inspector.")]
    [SerializeField] private UIBar MagicBar;

    [Tooltip("Duración en segundos del flash de color al ganar magia.")]
    [SerializeField] private float ColorDuration = 0.3f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>Magia actual del jugador, entre 0 y MaxMagic.</summary>
    private int _currentMagic;

    /// <summary>Tiempo restante del flash de color al ganar magia.</summary>
    private float _colorTimer;

    /// <summary>SpriteRenderer del jugador, usado para el flash de color.</summary>
    private SpriteRenderer _spriteRenderer;

    /// <summary>Color original del sprite, al que se vuelve tras el flash.</summary>
    private Color _ogColor;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Inicializa la magia actual al máximo, configura la barra de UI
    /// y guarda el color original del sprite.
    /// </summary>
    void Start()
    {
        _currentMagic = MaxMagic;

        if (MagicBar != null)
        {
            MagicBar.SetMaxValue(MaxMagic);
            MagicBar.SetValue(_currentMagic);
        }

     
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _ogColor = _spriteRenderer.color;
    }

    /// <summary>
    /// Controla el temporizador del flash de color al ganar magia.
    /// </summary>
    private void Update()
    {
        if (_colorTimer > 0)
        {
            _colorTimer -= Time.deltaTime;

            if (_colorTimer <= 0)
            {
                _spriteRenderer.color = _ogColor;
            }
        }
    }

    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos

    /// <summary>
    /// Aumenta la magia actual sin superar el máximo, actualiza la barra
    /// de UI y muestra el flash de color cian.
    /// </summary>
    public void IncreaseMagicAmount(int magicPoints)
    {
        _currentMagic = Mathf.Min(_currentMagic + magicPoints, MaxMagic);
        if (MagicBar != null) MagicBar.SetValue(_currentMagic);
        _spriteRenderer.color = Color.cyan;
        _colorTimer = ColorDuration; // Inicio del cronómetro
    }

    /// <summary>
    /// Intenta gastar la cantidad de magia indicada. Si no hay suficiente,
    /// no descuenta nada y devuelve false.
    /// </summary>
    /// <param name="amount">Cantidad de magia a gastar.</param>
    /// <returns>True si había magia suficiente y se ha descontado; false en caso contrario.</returns>
    public bool TrySpendMagic(int amount)
    {
        if (_currentMagic < amount)
        {
            return false;
        }
        _currentMagic -= amount;
        if (MagicBar != null)
        {
            MagicBar.SetValue(_currentMagic);
        }

        return true;
    }

    /// <summary>Indica si el jugador tiene al menos la cantidad de magia indicada.</summary>
    /// <param name="amount">Cantidad de magia a comprobar.</param>
    /// <returns>True si la magia actual es mayor o igual que amount.</returns>
    public bool HasEnoughMagic(int amount)
    {
        return _currentMagic >= amount;
    }

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados
    // Esta clase no tiene métodos privados.
    #endregion

} // class Magic
  // Celia García Riaza