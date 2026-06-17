//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Marián Navarro
// Nombre del juego
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class ShootSound : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    [Header("Audio Settings")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _sfxDisparo;
    [SerializeField, Range(0, 1)] private float _volumen = 0.7f;

    [Header("Bullet Prefab")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;

    [Header("Shoot Settings")]
    [Tooltip("Tiempo en segundos que debe pasar entre cada disparo.")]
    [SerializeField] private float _fireRate = 0.2f;

    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)

    /// <summary>
    /// Almacena el momento de tiempo exacto en el que podremos volver a disparar.
    /// </summary>
    private float _nextFireTime = 0f;

    /// <summary>Acción de Input System para disparar.</summary>
    private InputAction _attackAction;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    private void Start()
    {
        _attackAction = InputSystem.actions.FindAction("Attack");
        if (_attackAction == null)
        {
            Debug.LogError("[PlayerShoot] Acción 'Attack' no encontrada.");
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        // Usamos GetMouseButton(0) para detectar si se MANTIENE pulsado el botón.
        // Además, comprobamos si el tiempo actual del juego es mayor o igual al tiempo del próximo disparo.
        if (_attackAction.IsInProgress() && Time.time >= _nextFireTime)
        {
            // Calculamos cuándo será el próximo disparo permitido
            _nextFireTime = Time.time + _fireRate;

            Shoot();
        }
    }
    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados

    private void Shoot()
    {
        if (_audioSource != null && _sfxDisparo != null)
        {
            // PlayOneShot es perfecto aquí porque permite que los sonidos
            // de disparos muy rápidos se superpongan ligeramente de forma natural.
            _audioSource.PlayOneShot(_sfxDisparo, _volumen);
        }

        if (_bulletPrefab != null && _firePoint != null)
        {
            // Instancia la bala y la lanza
            GameObject bullet = Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);

            // Asumiendo que tu script Bullet tiene el método Init
            if (bullet.TryGetComponent<Bullet>(out Bullet bulletScript))
            {
                bulletScript.Init(_firePoint.right, 20);
            }
        }
    }

    #endregion

} // class ShootSound 
// namespace
