using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(AudioSource))]
public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [SerializeField] private float _damageAmount = 10f;
    [SerializeField] private float _attackInterval = 1.5f;
    [SerializeField] private float _soundLeadTime = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioClip _attackSound;

    private EnemyPatrol _enemyPatrol;
    private AudioSource _audioSource;
    private Coroutine _attackCoroutine;
    private bool _isAttacking = false;
    private bool _playerInRange = false; // Detecta si físicamente se están tocando

    private void Start()
    {
        _enemyPatrol = GetComponent<EnemyPatrol>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        bool chasing = _enemyPatrol.IsChasing;

        if (chasing && !_isAttacking)
        {
            _attackCoroutine = StartCoroutine(AttackLoop());
            _isAttacking = true;
        }
        else if (!chasing && _isAttacking)
        {
            StopCoroutine(_attackCoroutine);
            _isAttacking = false;
        }
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            // Solo atacamos si está cerca (colisionando) AAAAA
            if (_playerInRange)
            {
                if (_attackSound != null) _audioSource.PlayOneShot(_attackSound);

                yield return new WaitForSeconds(_soundLeadTime);

                // Aplicar el daño
                ApplyDamage();

                yield return new WaitForSeconds(_attackInterval - _soundLeadTime);
            }
            else
            {
                // Si está persiguiendo pero aún no lo toca, esperamos un poco antes de volver a chequear
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void ApplyDamage()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Buscamos el componente Health (el script de Celia)
            Health health = player.GetComponent<Health>();

            if (health != null)
            {
                health.Damage((int)_damageAmount);
                Debug.Log($"[Ataque] Daño de {(int)_damageAmount} enviado al script Health.");
            }
        }
    }

    // Detectar contacto físico (TENGO QUE CAMBIARLO)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) _playerInRange = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) _playerInRange = false;
    }
}