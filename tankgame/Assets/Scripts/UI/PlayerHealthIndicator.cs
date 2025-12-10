using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthIndicator : MonoBehaviour
{
    public Image fillBar;
    public TankController playerTank;
    private float maxHealth;
    private float currentHealth;

    void Start()
    {
        maxHealth = playerTank.maxHealth;
        currentHealth = playerTank.currentHealth;
        fillBar.fillAmount = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        currentHealth = playerTank.currentHealth;
        float fillAmount = currentHealth / maxHealth;
        fillBar.fillAmount = fillAmount;
    }
}
