using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private TextMeshProUGUI timerText;

    private float timeRemaining;
    private string timeAsString;

    private bool counting;

    private void Start()
    {
        gameManager = GameManager.Instance;
    }
    public void SetTimer(float t)
    {
        StopAllCoroutines();
        timeRemaining = t;
        UpdateText();
    }

    private void UpdateText()
    {
        timeAsString = (int)timeRemaining / 60 + " : ";
        timeAsString += timeRemaining % 60 < 10 ? "0" : "";
        timerText.text = timeAsString + (int)timeRemaining % 60;
    }

    public IEnumerator Countdown()
    {
        counting = true;

        do
        {
            timeRemaining -= Time.deltaTime;
            UpdateText();
            yield return null;
        } while (timeRemaining > 0);

        counting = false;
        gameManager.GameOver();
    }
}
