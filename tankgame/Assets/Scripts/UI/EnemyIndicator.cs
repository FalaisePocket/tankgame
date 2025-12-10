using UnityEngine;
using UnityEngine.UI;

public class EnemyIndicator : MonoBehaviour
{
    public Transform enemy;                 // Enemigo a seguir
    public RectTransform indicator;         // RectTransform del icono UI
    public Camera cam;                      // Cámara del jugador
    public float heightOffset = 1.5f;       // Qué tan arriba se muestra el indicador
    public float borderOffset = 70f;        // Margen para indicadores fuera de pantalla

    void Update()
    {
        // Si el enemigo se destruyó → también destruye este indicador
        if (enemy == null)
        {
            Destroy(gameObject);
            return;
        }
        if (cam == null)
            return;

        // Posición del enemigo con altura extra
        Vector3 worldPos = enemy.position + Vector3.up * heightOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // ¿Está fuera de la pantalla?
        bool isOffScreen =
            screenPos.z < 0 ||
            screenPos.x < 0 || screenPos.x > Screen.width ||
            screenPos.y < 0 || screenPos.y > Screen.height;

        if (!isOffScreen)
        {
            // ENEMIGO DENTRO DE PANTALLA
            indicator.position = screenPos;
            indicator.localRotation = Quaternion.identity;
        }
        else
        {
            // ENEMIGO FUERA DE PANTALLA
            Vector3 center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            Vector3 dir = (screenPos - center).normalized;

            // Posición en el borde
            Vector3 edgePos = center + dir * (Mathf.Min(Screen.width, Screen.height) / 2f - borderOffset);

            indicator.position = edgePos;

            // Rotación hacia el enemigo
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            indicator.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
}
