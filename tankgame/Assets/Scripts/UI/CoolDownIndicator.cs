using UnityEngine;
using UnityEngine.UI;

public class CoolDownIndicator : MonoBehaviour
{
    public Image fillBar;     // La barra con Image Type = Filled
    private float cooldownTime = 0f;
    public TankController tankController;

    private bool isCoolingDown = false;
    private float cooldownTimer = 0f;
    private Image imageComponent;
    void Start()
    {
        
        cooldownTime = tankController.shootCooldown;
        fillBar.fillAmount = 0f;
        imageComponent = GetComponent<Image>();
        imageComponent.enabled = false;
    }
    void Update()
    {   
        

        if (isCoolingDown)
        {
            cooldownTimer = tankController.shootTimer;

            // Normalizamos de 1 a 0
            float fillAmount = cooldownTimer / cooldownTime;
            fillBar.fillAmount = fillAmount;

            if (cooldownTimer <= 0 || cooldownTimer > cooldownTime)
            {
                imageComponent.enabled = false;
                isCoolingDown = false;
                fillBar.fillAmount = 0f;
                
            }
            
            return;
        }
        if (!tankController.canShoot && !isCoolingDown)
        {
            StartCooldown();
        }
    }

    public void StartCooldown()
    {
        isCoolingDown = true;
        cooldownTimer = cooldownTime;
        fillBar.fillAmount = 0f;  // Llenamos la barra
        imageComponent.enabled = true;
    }
}
