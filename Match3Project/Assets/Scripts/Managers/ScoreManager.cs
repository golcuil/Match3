using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreManager : Singleton<ScoreManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;

    [SerializeField]private Transform collectionPoint;

    private TextMeshProUGUI scoreText;

    private int score = 0;
    public int Score { get => score; }

    protected override void Init()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        grid = (MatchableGrid)MatchableGrid.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
        scoreText.text = "Score: " + score;
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
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
