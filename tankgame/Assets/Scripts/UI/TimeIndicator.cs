using UnityEngine;
using TMPro;

public class TimeIndicator : MonoBehaviour
{
    public gamerules gameRules;
    private float hours;
    private float minutes;
    private float seconds;
    public TMP_Text textTimer;

    void Start()
    {
        textTimer = GetComponent<TMP_Text>();
    }

    void Update()
    {
        float timeLeft = gameRules.timeLeft;

        hours = Mathf.Floor(timeLeft / 3600);
        minutes = Mathf.Floor((timeLeft % 3600) / 60);
        seconds = Mathf.Floor(timeLeft % 60);

        string timeString;

        // Si quieres mostrar horas:
        // timeString = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);

        // Si solo quieres MM:SS:
        timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

        textTimer.text = timeString;
    }
}
