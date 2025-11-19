using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    
    [Header("Stats")]
    public int health = 100;
    public int ammoLeft = 30;
    [Header("Modules")]
    public Object cannon;
    public Object turret;
    public Object chasis;
    public Object leftTrack;
    public Object rightTrack;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void getDamage(int damage)
    {
        health -= damage;

    }


}
