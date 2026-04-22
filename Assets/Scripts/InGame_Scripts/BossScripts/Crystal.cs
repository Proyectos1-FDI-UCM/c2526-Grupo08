//---------------------------------------------------------
// Gestiona el tiempo que permanece el cristal en el juego y luego se destruye. Tambien detecta si ha chocado con el jugador
// Laura Garay Zubiaguirre
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;
// Añadir aquí el resto de directivas using


/// <summary>
/// Antes de cada class, descripción de qué es y para qué sirve,
/// usando todas las líneas que sean necesarias.
/// </summary>

public class Crystal : MonoBehaviour
{
    [Header("Configuración")]
    public int damage = 30;

    [Tooltip("Tiempo en segundos antes de que el cristal desaparezca solo.")]
    public float lifeTime = 2.0f;

    void Start()
    {
        // Se destruye el cristal
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Primero, comprobamos si lo que hemos tocado es el Jugador usando su Tag.
        // IMPORTANTE: Asegúrate de que tu objeto Jugador tiene el Tag "Player".
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.Damage(damage); //llamamos a health para quitar X daño al jugador

                // pa ver que funciona
                Debug.Log($"<color=red>¡Cristal golpeó al Jugador!</color> Daño aplicado: {damage}");
            }
        }
    }
}