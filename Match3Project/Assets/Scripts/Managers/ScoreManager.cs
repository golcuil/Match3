using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;
    private AudioMixer audioMixer;

    [SerializeField]private Transform collectionPoint;

    [SerializeField]
    private TextMeshProUGUI scoreText,
                            comboText;

    [SerializeField] Image comboSlider;

    private int score,
                comboMultiplier;
    public int Score { get => score; }

    private float timeSinceLastScore;

    [SerializeField] private float maxComboTime,
                                   currentComboTime;

    private bool timerIsActive;

    private void Start()
    {
        grid = (MatchableGrid)MatchableGrid.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
        audioMixer = (AudioMixer)AudioMixer.Instance;
        scoreText.text = score.ToString();

        comboText.enabled = false;
        comboSlider.gameObject.SetActive(false);
    }

    //When the player hits retry, reset the score and combo
    public void Reset()
    {
        score = 0;
        scoreText.text = score.ToString();
        timeSinceLastScore = maxComboTime;
    }

    public void AddScore(int amount)
    { 
        score += amount * IncreaseCombo();
        scoreText.text = score.ToString();

        timeSinceLastScore = 0;

        if(!timerIsActive)
        {
            StartCoroutine(ComboTimer());
        }

        //Play score sound
        audioMixer.PlaySound(SoundEffects.score);
    }

    //Combo timer coroutine, counts up to max combo time before resetting the combo multiplier.
    private IEnumerator ComboTimer()
    {
        timerIsActive = true;
        comboText.enabled = true;
        comboSlider.gameObject.SetActive(true);

        do
        {
            timeSinceLastScore += Time.deltaTime;
            comboSlider.fillAmount = 1 - timeSinceLastScore / currentComboTime;
            yield return null;

        } while (timeSinceLastScore < currentComboTime);

        comboMultiplier = 0;
        comboText.enabled = false;
        comboSlider.gameObject.SetActive(false);
        timerIsActive = false;
    }

    private int IncreaseCombo()
    {
        comboText.text = "Combo x" + comboMultiplier++;

        currentComboTime = maxComboTime - Mathf.Log(comboMultiplier) / 2;

        return comboMultiplier;
    }

    public IEnumerator ResolveMatch(Match toResolve, MatchType powerupUsed = MatchType.invalid)
    {
        Matchable powerupFormed = null;
        Matchable matchable;

        Transform target = collectionPoint;


        //If no powerup was used to trigger this match and a larger match is made, create a power up;
        if (powerupUsed == MatchType.invalid && toResolve.Count > 3)
        {
            powerupFormed = pool.UpgradeMatchable(toResolve.ToBeUpgraded, toResolve.Type);
            toResolve.RemoveMatchable(powerupFormed);
            target = powerupFormed.transform;
            powerupFormed.SortingOrder = 3;

            //Play upgrade sound
            audioMixer.PlaySound(SoundEffects.upgrade);
        }
        else
        {
            //Play resolve sound
            audioMixer.PlaySound(SoundEffects.resolve);
        }


        for (int i = 0; i != toResolve.Count; i++)
        {
            matchable = toResolve.Matchables[i];


            //Only allow gems used as powerups to resolve gems
            if (powerupUsed == MatchType.match4 && matchable.IsGem)
                continue;

            //Remove the matchables from the gird
            grid.RemoveItemAt(matchable.position);

            //Move them off to the side of the screen
            if (i == toResolve.Count - 1)
                yield return StartCoroutine(matchable.Resolve(target));
            else
                StartCoroutine(matchable.Resolve(target));
        }

        //Update the player's score
        AddScore(toResolve.Count * toResolve.Count);

        //If there was a powerup, reset the sorting order
        if (powerupFormed != null)
            powerupFormed.SortingOrder = 1;

        yield return null;
    }
}
