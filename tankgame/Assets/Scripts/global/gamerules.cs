using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas
using UnityEngine.UI;

public class gamerules : MonoBehaviour
{
    public int maxEnemies = 10;
    public float distanceBetween = 9f;
    public Transform spawnArea;
    public Transform initialInvestigationPointObject;
    public GameObject enemyPrefab;
    public GameObject player;
    private TankController playerTank;
    GameObject playerInstance;
    public float timeLeft = 600f;

    // 👉 Referencia al panel de Game Over
    public GameObject gameOverPanel;
    private List<GameObject> enemies;

    private bool gameEnded = false;

    void Start()
    {
        playerTank = player.GetComponent<TankController>();
        playerTank.OnPlayerDeath += OnPlayerKilled;
        SpawnEnemies();
    }
    void Update()
    {
        if (gameEnded) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            GameOver();
        }
        if (AllEnemiesDead())
        {
            winGame();
        }
    }

    void GameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameEnded = true;
        Time.timeScale = 0f; // Pausa el juego
        gameOverPanel.SetActive(true);
        Debug.Log("Tiempo agotado. Has perdido.");
    }
    void winGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameEnded = true;
        Time.timeScale = 0f; // Pausa el juego
        //gameOverPanel.SetActive(true);
        Debug.Log("Has ganado!");
    }

    // 👉 Se llama desde el botón Retry
    public void RetryGame()
    {
        Time.timeScale = 1f; // Asegura que el juego se despausa
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // 👉 Se llama desde el botón Main Menu
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Nombre de tu escena del menú
    }

    public void SpawnEnemies()
    {
        if (enemyPrefab == null || spawnArea == null)
        {
            Debug.LogError("Falta asignar el prefab o el punto de spawn en el inspector.");
            return;
        }

        // 🔹 Distancia entre enemigos
        Vector3 basePos = spawnArea.position;

        for (int i = 0; i < maxEnemies; i++)
        {
            // 🔹 Calculamos una posición con separación
            Vector3 offset = new Vector3(i * distanceBetween, 0, 0);
            Vector3 spawnPos = basePos + offset;

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, spawnArea.rotation);
            enemies.Add(newEnemy);
            // 🔹 Asignamos referencias al script del enemigo
            AgentTankAI enemy = newEnemy.GetComponent<AgentTankAI>();

            if (enemy != null)
            {
                enemy.enemy = player.transform;                     // 👈 asigna Player como objetivo
                enemy.initialInvestigationPoint = initialInvestigationPointObject; // 👈 punto al que irá primero
            }
        }

        
    }
    void OnPlayerKilled()
    {
        GameOver();
    }
    bool AllEnemiesDead()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
                return false;
        }
        return true;
    }

}
