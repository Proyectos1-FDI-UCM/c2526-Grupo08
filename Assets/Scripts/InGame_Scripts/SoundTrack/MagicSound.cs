//---------------------------------------------------------
// Reproduce un sonido al recoger un punto de magia.
// Marián Navarro Santoyo
// No Way Down
// Proyectos 1 - Curso 2025-26
//---------------------------------------------------------

using UnityEngine;

/// <summary>
/// Se adjunta al prefab del punto de magia que sueltan los enemigos al morir.
/// Cuando el jugador entra en contacto con él (trigger), reproduce el clip
/// de recogida y se destruye. El sonido se reproduce con PlayClipAtPoint
/// para que no se corte al destruirse el GameObject.
/// </summary>
public class MagicSound : MonoBehaviour
{
    // ---- ATRIBUTOS DEL INSPECTOR ----
    #region Atributos del Inspector

    [Header("Configuración de Sonido")]
    [Tooltip("AudioClip que se reproduce al recoger el punto de magia.")]
    [SerializeField] private AudioClip pickupClip;

    [Tooltip("Volumen del sonido de recogida (0 = silencio, 1 = máximo).")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    #endregion

    // ---- MÉTODOS DE MONOBEHAVIOUR ----
    #region Métodos de MonoBehaviour

    /// <summary>
    /// Si lo que entra en el trigger tiene componente PlayerMovement o Magic
    /// (es el jugador), reproduce el sonido de recogida.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() != null || other.GetComponent<Magic>() != null)
        {
            if (pickupClip != null)
                AudioSource.PlayClipAtPoint(pickupClip, transform.position, volume);
        }
    }

    #endregion

} // class MagicSound
  // Marián Navarro Santoyo