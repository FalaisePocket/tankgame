using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas
using UnityEngine.UI;

public class gamerules : MonoBehaviour
{
    public int maxEnemies = 10;
    public Vector3 spawnAreaMin = new Vector3(0f, 0f, 0f); 
    public GameObject enemyPrefab;
    public GameObject playerPrefab;
    public float timeLeft = 600f;

    // 👉 Referencia al panel de Game Over
    public GameObject gameOverPanel;

    private bool gameEnded = false;

    void Update()
    {
        if (gameEnded) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            GameOver();
        }
    }

    void GameOver()
    {
        gameEnded = true;
        Time.timeScale = 0f; // Pausa el juego
        gameOverPanel.SetActive(true);
        Debug.Log("Tiempo agotado. Has perdido.");
    }

    // 👉 Se llama desde el botón Retry
    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 👉 Se llama desde el botón Main Menu
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Nombre de tu escena del menú
    }
}
