using UnityEngine;

public class gamerules : MonoBehaviour
{

    public int maxEnemies = 10;
    public Vector3 spawnAreaMin = new Vector3(0f, 0f, 0f); 
    public GameObject enemyPrefab;
    public GameObject playerPrefab;
    public float timeLeft = 3600f; // Tiempo en segundos
    private GameObject player;
    private GameObject[] enemies;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //spawnEnemies();
    }

    // Update is called once per frame
    void Update()
    {
        
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            Debug.Log("Tiempo agotado. Has perdido.");
            // Aquí puedes agregar lógica para finalizar el juego o reiniciar el nivel
            timeLeft = 0f; // Evitar que el tiempo sea negativo
        }
    }
    private void spawnEnemies()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMin.x + 50f),
                spawnAreaMin.y,
                Random.Range(spawnAreaMin.z, spawnAreaMin.z + 50f)
            );

            GameObject enemy = Instantiate(Resources.Load("Enemy"), spawnPosition, Quaternion.identity) as GameObject;
        }
    }
}
