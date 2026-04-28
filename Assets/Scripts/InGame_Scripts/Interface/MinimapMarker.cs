//---------------------------------------------------------
// Muestra un marcador (punto) con la posición del jugador
// sobre la imagen del mapa en la pantalla de pausa/mapa.
// Alexia Pérez Santana
// — No Way Down
// — Proyectos 1 2025-26
//---------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Convierte la posición del jugador en coordenadas de mundo
/// a coordenadas normalizadas sobre la imagen del mapa y mueve
/// un RectTransform (punto rojo) en tiempo real.
///
/// SETUP EN UNITY:
///   1. En el Canvas del mapa (el panelMap del LevelManager),
///      añade una Image con el sprite de tu mapa → "MapImage"
///   2. Crea un hijo de MapImage: Image pequeña redonda (16x16)
///      con color rojo/cyan → "PlayerDot"
///   3. Crea un GameObject vacío en escena → añade este script
///   4. Asigna en el Inspector:
///        · PlayerTransform  → el Transform del jugador (Player)
///        · MapImage         → la Image del mapa
///        · PlayerDot        → el RectTransform del punto
///        · WorldMin         → esquina inferior-izquierda del nivel en coordenadas mundo
///        · WorldMax         → esquina superior-derecha del nivel en coordenadas mundo
///
/// WorldMin y WorldMax definen el rectángulo del nivel que
/// corresponde a los bordes de la imagen del mapa.
/// Puedes encontrar los valores colocando un GameObject vacío
/// en cada esquina del nivel y leyendo su posición.
/// </summary>
public class MinimapMarker : MonoBehaviour
{
    // ---- INSPECTOR ----
    #region Inspector

    [Header("Referencias")]
    [Tooltip("Transform del jugador (Player).")]
    [SerializeField] private Transform PlayerTransform;

    [Tooltip("Image del mapa en el Canvas (panelMap).")]
    [SerializeField] private RectTransform MapImage;

    [Tooltip("RectTransform del punto que indica la posicion del jugador.")]
    [SerializeField] private RectTransform PlayerDot;

    [Header("Limites del nivel en coordenadas mundo")]
    [Tooltip("Coordenada mundo de la esquina inferior-izquierda del nivel.")]
    [SerializeField] private Vector2 WorldMin = new Vector2(-20f, -20f);

    [Tooltip("Coordenada mundo de la esquina superior-derecha del nivel.")]
    [SerializeField] private Vector2 WorldMax = new Vector2(20f, 20f);

    #endregion

    // ---- MONOBEHAVIOUR ----
    #region MonoBehaviour

    private void LateUpdate()
    {
        if (PlayerTransform == null || MapImage == null || PlayerDot == null) { return; }

        // Normalizar la posicion del jugador entre 0 y 1 dentro del area del nivel
        Vector2 worldPos = PlayerTransform.position;
        float nx = Mathf.InverseLerp(WorldMin.x, WorldMax.x, worldPos.x);
        float ny = Mathf.InverseLerp(WorldMin.y, WorldMax.y, worldPos.y);

        // Clamp para que el punto nunca salga de la imagen
        nx = Mathf.Clamp01(nx);
        ny = Mathf.Clamp01(ny);

        // Convertir a coordenadas locales del RectTransform del mapa
        // MapImage.rect da el tamaño real de la imagen en pixels de UI
        Rect rect = MapImage.rect;
        float localX = (nx - 0.5f) * rect.width;
        float localY = (ny - 0.5f) * rect.height;

        PlayerDot.localPosition = new Vector3(localX, localY, 0f);
    }

    #endregion

    // ---- GIZMO ----
    #region Gizmo

    private void OnDrawGizmosSelected()
    {
        // Dibuja el rectangulo del nivel en la Scene View para facilitar la configuracion
        Gizmos.color = new Color(0.47f, 0.89f, 0.84f, 0.4f);
        Vector3 center = new Vector3((WorldMin.x + WorldMax.x) * 0.5f,
                                     (WorldMin.y + WorldMax.y) * 0.5f, 0f);
        Vector3 size = new Vector3(WorldMax.x - WorldMin.x,
                                     WorldMax.y - WorldMin.y, 0.1f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(0.47f, 0.89f, 0.84f, 0.9f);
        Gizmos.DrawWireCube(center, size);
    }

    #endregion

} // class MinimapMarker