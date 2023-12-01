using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sprite))]
public class Matchable : Movable
{
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
    }

    public void SetType(int type, Sprite sprite, Color color)
    {
        this.type = type;
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
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
