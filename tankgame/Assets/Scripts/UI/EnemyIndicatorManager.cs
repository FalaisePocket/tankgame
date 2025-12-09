using UnityEngine;
using System.Collections.Generic;

public class EnemyIndicatorManager : MonoBehaviour
{
    public static EnemyIndicatorManager instance;

    public GameObject indicatorPrefab;   // Prefab del icono UI
    public Camera cam;                   // Cámara del jugador

    private Dictionary<Transform, GameObject> indicators = new();

    void Awake()
    {
        instance = this;
    }

    public void RegisterEnemy(Transform enemy)
    {
        // Ya registrado → salir
        if (indicators.ContainsKey(enemy))
            return;

        // Crear indicador
        GameObject newIndicator = Instantiate(indicatorPrefab, transform);
        indicators.Add(enemy, newIndicator);

        EnemyIndicator script = newIndicator.GetComponent<EnemyIndicator>();
        script.enemy = enemy;
        script.indicator = newIndicator.GetComponent<RectTransform>();
        script.cam = cam;
    }

    public void UnregisterEnemy(Transform enemy)
    {
        if (indicators.ContainsKey(enemy))
        {
            Destroy(indicators[enemy]);  // destruir el indicador
            indicators.Remove(enemy);
        }
    }
}
