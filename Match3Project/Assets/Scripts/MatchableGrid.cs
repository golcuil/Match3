using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchableGrid : GridSystem<Matchable>
{
    private MatchablePool pool;

    [SerializeField]private Vector3 offscreenOffset;

    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
    }

    public IEnumerator PopulateGrid(bool allowMatches = false)
    {
        Matchable newMatchable;
        Vector3 onscreenPosition;

        for(int y = 0; y < Dimensions.y; y++)
            for(int x = 0; x < Dimensions.x; x++)
            {
                //Get matcahble from the pool
                newMatchable = pool.GetRandomMatchable();

                //Position the matchable on screen
                //newMatchable.transform.position = transform.position + new Vector3(x, y);
                onscreenPosition = transform.position + new Vector3(x, y);
                newMatchable.transform.position = onscreenPosition + offscreenOffset;

                //Activate object
                newMatchable.gameObject.SetActive(true);

                //Tell this matcahble where it is on the gird
                newMatchable.position = new Vector2Int(x, y);

                //Place the matchable on the grid
                PutItemAt(newMatchable, x, y);

                int type = newMatchable.Type;

                while(!allowMatches && IsPartOfAMatch(newMatchable))
                {
                    //Change the matchable's type until it isn't a match anymore
                    if (pool.NextType(newMatchable) == type)
                    {
                        Debug.LogWarning("Failed to find a matchable type that didn't match at (" + x + ", " + y + ")");
                        Debug.Break();
                        break;
                    }
                        
                }
                //Move the matchable to its on screen position
                StartCoroutine(newMatchable.MoveToPosition(onscreenPosition));

                yield return new WaitForSeconds(0.01f);
            }
        //yield return null; all coming from top
    }

    /*
     * TODO: Complete this function later
     */
    private bool IsPartOfAMatch(Matchable matchable)
    {
        return false;
    }
}
