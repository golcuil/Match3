using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;

    [SerializeField] private Vector2Int dimensions = Vector2Int.one;

    [SerializeField] private TextMeshProUGUI gridOutput;

    // Start is called before the first frame update
    void Start()
    {
        grid = (MatchableGrid)MatchableGrid.Instance;
        pool = (MatchablePool)MatchablePool.Instance;

        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        // Loading screen can be add here

        // Pool the matchables
        pool.PoolObjects(dimensions.x * dimensions.y * 2);

        // Create the grid
        grid.InitializeGrid(dimensions);

        yield return null;

        StartCoroutine(grid.PopulateGrid(false,true));
        // Then remove loading screen here
    }
}
