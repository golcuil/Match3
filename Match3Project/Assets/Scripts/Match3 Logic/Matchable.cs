using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sprite))]
public class Matchable : Movable
{
    private MatchablePool pool;
    private Cursor cursor;

    private int type;
    public int Type { get => type; }

    private SpriteRenderer spriteRenderer;

    //Where is the matchable in the grid
    public Vector2Int position;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cursor = Cursor.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
    }

    public void SetType(int type, Sprite sprite, Color color)
    {
        this.type = type;
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
    }

    public IEnumerator Resolve(Transform collectionPoint)
    {
        //Draw above others in the grid
        spriteRenderer.sortingOrder = 2;

        //Move off the grid to a collection point
        yield return StartCoroutine(MoveToPosition(collectionPoint.position));

        //Reset
        spriteRenderer.sortingOrder = 1;
        yield return null;

        //Return back to the pool
        pool.ReturnObjectToPool(this);
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
