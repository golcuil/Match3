using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;
    private AudioMixer audioMixer;
    private Cursor cursor;
    private ScoreManager scoreManager;

    [SerializeField] private Fader loadingScreen,
                                    darkener;

    [SerializeField] private Vector2Int dimensions = Vector2Int.one;

    [SerializeField] private TextMeshProUGUI gridOutput;

    [SerializeField] private TextMeshProUGUI finalScoreText;

    [SerializeField] Movable resultsPage;

    [SerializeField] private bool levelIsTimed;
    [SerializeField] private LevelTimer timer;
    [SerializeField] private float timeLimit;

    // Start is called before the first frame update
    void Start()
    {
        grid = (MatchableGrid)MatchableGrid.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
        audioMixer = (AudioMixer)AudioMixer.Instance;
        cursor = (Cursor)Cursor.Instance;
        scoreManager = (ScoreManager)ScoreManager.Instance;

        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        //Disable user input
        cursor.enabled = false;

        //Unhide Loading screen
        loadingScreen.Hide(false);

        //If level is timed, set the timer
        if (levelIsTimed)
            timer.SetTimer(timeLimit);

        //Pool the matchables
        pool.PoolObjects(dimensions.x * dimensions.y * 2);

        //Create the grid
        grid.InitializeGrid(dimensions);

        //Fade out loading screen
        StartCoroutine(loadingScreen.Fade(0f));

        //Start background music
        audioMixer.PlayMusic();

        //Populate the grid
        yield return StartCoroutine(grid.PopulateGrid(false,true));

        //Check for gridlock and offer the player a hint if they need it
        grid.CheckPossibleMoves();

        //Enable user input
        cursor.enabled = true;

        //If level is timed, start the timer
        if (levelIsTimed)
            StartCoroutine(timer.Countdown());
    }

    public void NoMoreMoves()
    {
        //If level is timed, reward the player for running out of moves
        if (levelIsTimed)
            grid.MatchEverything();

        //In survival mode, punish the player for running out of moves
        else
            GameOver();

    }

    public void GameOver()
    {
        //Get and update the final score for the results page
        finalScoreText.text = scoreManager.Score.ToString();

        //Disable the cursor
        cursor.enabled = false;

        //Unhide the darkener and fade in
        darkener.Hide(false);
        StartCoroutine(darkener.Fade(0.75f));

        //Move the results page onto the screen
        StartCoroutine(resultsPage.MoveToPosition(new Vector2(Screen.width / 2, Screen.height / 2)));
    }

    private IEnumerator Quit()
    {
        yield return StartCoroutine(loadingScreen.Fade(1));
        SceneManager.LoadScene("Main Menu");
    }

    public void QuitButtonPressed()
    {
        StartCoroutine(Quit());  
    }

    private IEnumerator Retry()
    {
        //Fade out the darkener, and move the results page off screen
        StartCoroutine(resultsPage.MoveToPosition(new Vector2(Screen.width / 2, Screen.height / 2) + Vector2.down * 1000));
        yield return StartCoroutine(darkener.Fade(0));
        darkener.Hide(true);

        //Reset the cursor, game grid and score
        if (levelIsTimed)
            timer.SetTimer(timeLimit);

        cursor.Reset();
        scoreManager.Reset();

        yield return StartCoroutine(grid.Reset());

        //let the player start again
        cursor.enabled = true;

        if (levelIsTimed)
            StartCoroutine(timer.Countdown());
    }

    public void RetryButtonPressed()
    {
        StartCoroutine(Retry());
    }
}
