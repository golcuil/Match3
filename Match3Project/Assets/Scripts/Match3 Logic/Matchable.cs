using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sprite))]
public class Matchable : Movable
{
    private MatchableGrid grid;
    private MatchablePool pool;
    private Cursor cursor;

    private int type;
    public int Type { get => type; }

    private MatchType powerup = MatchType.invalid;

    public bool IsGem
    {
        get
        {
            return powerup == MatchType.match5;
        }
    }

    private SpriteRenderer spriteRenderer;

    //Where is the matchable in the grid
    public Vector2Int position;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cursor = Cursor.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
    }

    public void SetType(int type, Sprite sprite, Color color)
    {
        this.type = type;
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
    }

    public IEnumerator Resolve(Transform collectionPoint)
    {
        // If matchable is a powerup, resolve it as such
        if(powerup != MatchType.invalid)
        {
            // Resolve a match4 powerup
            if(powerup == MatchType.match4)
            {
                //score everything adjacent to this
                grid.MatchAllAdjacent(this);
            }

            // Resolve a match5 powerup

            // Resolve a cross powerup
            if(powerup == MatchType.cross)
            {
                grid.MatchRowAndColumn(this);
            }

            powerup = MatchType.invalid;
        }

        //If this was called as the result of a powerup being upgraded, then don't move this off the grid
        if (collectionPoint == null)
            yield break;
        //Draw above others in the grid
        spriteRenderer.sortingOrder = 2;

        //Move off the grid to a collection point
        yield return StartCoroutine(MoveToTransform(collectionPoint));

        //Reset
        spriteRenderer.sortingOrder = 1;
        yield return null;

        //Return back to the pool
        pool.ReturnObjectToPool(this);
    }

    //Change the sprite of this matchable to be a powerup while retaining color and type
    public Matchable Upgrade(MatchType powerupType, Sprite powerUpSprite)
    {
        //if this is already a powerup, resolve it before updating
        if(powerup != MatchType.invalid)
        {
            idle = false;
            StartCoroutine(Resolve(null));
            idle = true;
        }
        if(powerupType == MatchType.match5)
        {
            type = -1;
            spriteRenderer.color = Color.white;
        }


        powerup = powerupType;
        spriteRenderer.sprite = powerUpSprite;

        return this;
    }

    //Set the sorting order of the sprite renderer so it will be drawn above or below others.
    public int SortingOrder
    {
        set
        {
            spriteRenderer.sortingOrder = value;
        }
    }

    private void OnMouseDown()
    {
        cursor.SelectFirst(this);
    }

    private void OnMouseUp()
    {
        cursor.SelectFirst(null);
    }

    private void OnMouseEnter()
    {
        cursor.SelectSecond(this);
    }

    public override string ToString()
    {
        return gameObject.name;
    }
}
