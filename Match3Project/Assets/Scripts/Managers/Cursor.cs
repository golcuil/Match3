using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Cursor : Singleton<Cursor>
{
    private MatchableGrid grid;

    private SpriteRenderer spriteRenderer;

    private Matchable[] selectedMatchables;

    [SerializeField]
    private Vector2Int   verticalStretch    = new Vector2Int(1, 2),
                         horizontalStretch  = new Vector2Int(2, 1);

    [SerializeField]
    private Vector3 halfUp      = Vector3.up / 2,
                    halfDown    = Vector3.down / 2,
                    halfLeft    = Vector3.left / 2,
                    halfRight   = Vector3.right / 2;

    protected override void Init()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.enabled = false;

        selectedMatchables = new Matchable[2];
    }

    private void Start()
    {
        grid = (MatchableGrid)MatchableGrid.Instance;
    }

    public void SelectFirst(Matchable toSelect)
    {
        selectedMatchables[0] = toSelect;

        if (!enabled || selectedMatchables[0] == null)
            return;

        transform.position = toSelect.transform.position;

        spriteRenderer.size = Vector2.one;
        spriteRenderer.enabled = true;

    }

    public void SelectSecond(Matchable toSelect)
    {
        selectedMatchables[1] = toSelect;

        if (!enabled || selectedMatchables[0] == null || selectedMatchables[1] == null ||
            !selectedMatchables[0].Idle || !selectedMatchables[1].Idle ||
            selectedMatchables[0] == selectedMatchables[1])
            return;

        if (SelectedAreAdjacent())
            StartCoroutine(grid.TrySwap(selectedMatchables));


        SelectFirst(null);
    }

    private bool SelectedAreAdjacent()
    {
        if (selectedMatchables[0].position.x == selectedMatchables[1].position.x)
        {
            if (selectedMatchables[0].position.y == selectedMatchables[1].position.y + 1)
            {
                spriteRenderer.size = verticalStretch;
                transform.position += halfDown;
                return true;
            }
            else if (selectedMatchables[0].position.y == selectedMatchables[1].position.y - 1)
            {
                spriteRenderer.size = verticalStretch;
                transform.position += halfUp;
                return true;
            }
        }
        else if (selectedMatchables[0].position.y == selectedMatchables[1].position.y)
        {
            if (selectedMatchables[0].position.x == selectedMatchables[1].position.x + 1)
            {
                spriteRenderer.size = horizontalStretch;
                transform.position += halfLeft;
                return true;
            }
            else if (selectedMatchables[0].position.x == selectedMatchables[1].position.x - 1)
            {
                spriteRenderer.size = horizontalStretch;
                transform.position += halfRight;
                return true;
            }
        }

        return false;
    }
}
