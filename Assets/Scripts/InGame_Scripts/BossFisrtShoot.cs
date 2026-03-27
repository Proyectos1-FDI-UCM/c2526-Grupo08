//---------------------------------------------------------
// Breve descripción del contenido del archivo
// Marian Navarro Santoyo
// No way down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>
public class BossFisrtShoot : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector (serialized fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // públicos y de inspector se nombren en formato PascalCase
    // (palabras con primera letra mayúscula, incluida la primera letra)
    // Ejemplo: MaxHealthPoints

    [Header("Configuración de Ataque")]
    [SerializeField] private int damageValue = 30;
    [SerializeField] private float detectionRange = 12f;
    //[SerializeField] private float cooldownTime = 3f; por definir eh
   
    [SerializeField] private float minWaitTime = 7f;
    [SerializeField] private float maxWaitTime = 15f;

    [Header("Referencias")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private Transform _playerTransform;
    private float _timer = 0f;
    private float _nextAttackCooldown;
    #endregion

    // ---- ATRIBUTOS PRIVADOS ----
    #region Atributos Privados (private fields)
    // Documentar cada atributo que aparece aquí.
    // El convenio de nombres de Unity recomienda que los atributos
    // privados se nombren en formato _camelCase (comienza con _, 
    // primera palabra en minúsculas y el resto con la 
    // primera letra en mayúsculas)
    // Ejemplo: _maxHealthPoints

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    // Por defecto están los típicos (Update y Start) pero:
    // - Hay que añadir todos los que sean necesarios
    // - Hay que borrar los que no se usen 

    /// <summary>
    /// Start is called on the frame when a script is enabled just before 
    /// any of the Update methods are called the first time.
    /// </summary>
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (_playerTransform == null) return;

        // El cronómetro avanza cada frame
        _timer += Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

        // Si ha pasado el tiempo aleatorio y el jugador está cerca
        if (_timer >= _nextAttackCooldown && distanceToPlayer <= detectionRange)
        {
            ExecuteAttack();
        }

    }
    #endregion

    // ---- MÉTODOS PÚBLICOS ----
    #region Métodos públicos
    // Documentar cada método que aparece aquí con ///<summary>
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)
    // Ejemplo: GetPlayerController

    #endregion

    // ---- MÉTODOS PRIVADOS ----
    #region Métodos Privados
    // Documentar cada método que aparece aquí
    // El convenio de nombres de Unity recomienda que estos métodos
    // se nombren en formato PascalCase (palabras con primera letra
    // mayúscula, incluida la primera letra)

    #endregion

    private void ExecuteAttack()
    {
        transform.position = _playerTransform.position;

        Health playerHealth = _playerTransform.GetComponent<Health>(); //aquí llama a health que sino mi bro no recibe daño.
        if (playerHealth != null)
        {
            playerHealth.Damage(damageValue);
            Debug.Log(" Daño " + damageValue + " aplicado."); //esto lo he puesto para ver en unity si funciona pero se puede quitar.
        }

        if (bulletPrefab != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        }
        _timer = 0f;
        SetRandomCooldown();
    }

    private void SetRandomCooldown()
    {
        _nextAttackCooldown = Random.Range(minWaitTime, maxWaitTime);
    }
        
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRange); //esto para que el jugador sepa que viene la mierda del ataque <3.
    }

} // class BossFisrtShoot 
// namespace
