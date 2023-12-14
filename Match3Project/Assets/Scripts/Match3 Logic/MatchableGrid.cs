using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class MatchableGrid : GridSystem<Matchable>
{
    private MatchablePool pool;
    private ScoreManager score;
    private HintIndicator hint;
    private AudioMixer audioMixer;

    [SerializeField]private Vector3 offscreenOffset;

    private List<Matchable> possibleMoves;

    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        score = (ScoreManager)ScoreManager.Instance;
        hint = (HintIndicator)HintIndicator.Instance;
        audioMixer = (AudioMixer)AudioMixer.Instance;
    }

    //When the player hits retry, wipe the board an repopulate
    public IEnumerator Reset()
    {
        for (int y = 0; y < Dimensions.y; y++)
            for (int x = 0; x < Dimensions.x; x++)
                if (!IsEmpty(x, y))
                    pool.ReturnObjectToPool(RemoveItemAt(x, y));

        yield return StartCoroutine(PopulateGrid(false, true));
    }

    public IEnumerator PopulateGrid(bool allowMatches = false, bool initialPopulation = false)
    {
        List<Matchable> newMatchables = new List<Matchable>();
        Matchable newMatchable;
        Vector3 onscreenPosition;

        for(int y = 0; y < Dimensions.y; y++)
            for(int x = 0; x < Dimensions.x; x++)
            {
                if(IsEmpty(x,y))
                {
                    //Get matcahble from the pool
                    newMatchable = pool.GetRandomMatchable();

                    //Position the matchable off screen
                    newMatchable.transform.position = transform.position + new Vector3(x, y) + offscreenOffset;

                    //Activate object
                    newMatchable.gameObject.SetActive(true);

                    //Tell this matcahble where it is on the gird
                    newMatchable.position = new Vector2Int(x, y);

                    //Place the matchable on the grid
                    PutItemAt(newMatchable, x, y);

                    //Add new matchable to the list
                    newMatchables.Add(newMatchable);

                    //What was the initial type of the new matchable ?
                    int initialType = newMatchable.Type;

                    while (!allowMatches && IsPartOfAMatch(newMatchable))
                    {
                        //Change the matchable's type until it isn't a match anymore
                        if (pool.NextType(newMatchable) == initialType)
                        {
                            Debug.LogWarning("Failed to find a matchable type that didn't match at (" + x + ", " + y + ")");
                            Debug.Break();
                            break;
                        }

                    }
                }              
            }

        //Move each matchable to its on screen position, yielding until the last has finished
        for(int i = 0; i<newMatchables.Count; i++)
        {
            //Calculate the future on screen position of the matchable
            onscreenPosition = transform.position + new Vector3(newMatchables[i].position.x, newMatchables[i].position.y);

            //Play a landing sound after a delay when the matchable lands in position
            audioMixer.PlayDelayedSound(SoundEffects.land, 1f / newMatchables[i].Speed);

            //Move the matchable to its on screen position
            if (i == newMatchables.Count-1)
                yield return StartCoroutine(newMatchables[i].MoveToPosition(onscreenPosition));

            else
                StartCoroutine(newMatchables[i].MoveToPosition(onscreenPosition));

            //Pause for 1/10 of a second after each for cool effect
            if(initialPopulation)
                yield return new WaitForSeconds(0.01f);

        }
    }

    //Check if the matchable being populated is part of a match
    private bool IsPartOfAMatch(Matchable toMatch)
    {
        int horizontalMatches = 0,
            verticalMatches = 0;

        //Look left and right
        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.left);
        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.right);

        if (horizontalMatches > 1)
            return true;


        //Look up and down
        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.up);
        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.down);

        if (verticalMatches > 1)
            return true;

        return false;
    }

    //Count the number of matches on the grid starting from the matchable to match moving in the direction indicated
    private int CountMatchesInDirection(Matchable toMatch, Vector2Int direction)
    {
        int matches = 0;

        //First look to the left
        Vector2Int position = toMatch.position + direction;

        while (CheckBounds(position) && !IsEmpty(position) && GetItemAt(position).Type == toMatch.Type)
        {
            matches++;
            position += direction;
        }
        return matches;
    }

    public IEnumerator TrySwap(Matchable[] toBeSwapped)
    {
        //Make a local copy of what we're swapping so Cursor doesn't overwrite
        Matchable[] copies = new Matchable[2];
        copies[0] = toBeSwapped[0];
        copies[1] = toBeSwapped[1];

        //hide the hint indicator
        hint.CancelHint();

        //Yield until matchables animate swapping
        yield return StartCoroutine(Swap(copies));

        //Special cases for gems
        //If both are gems, then match everything on the grid
        if (copies[0].IsGem && copies[1].IsGem)
        {
            MatchEverything();
            yield break;
        }

        //if 1 is a gem, then match all mathing the color of the other
        if (copies[0].IsGem)
        {
            MatchEverythingByType(copies[0], copies[1].Type);
            yield break;
        }
        else if (copies[1].IsGem)
        {
            MatchEverythingByType(copies[1], copies[0].Type);
            yield break;
        }

        //Check for a valid match
        Match[] matches = new Match[2];
        matches[0] = GetMatch(copies[0]);
        matches[1] = GetMatch(copies[1]);

        if (matches[0] != null)
        {
            //Resolve Match
            StartCoroutine(score.ResolveMatch(matches[0]));
        }
        if (matches[1] != null)
        {
            //Resolve Match
            StartCoroutine(score.ResolveMatch(matches[1]));
        }

        //If there is no match, swap them back
        if (matches[0] == null && matches[1] == null)
        {
            yield return StartCoroutine(Swap(copies));

            if(ScanForMatches())
                StartCoroutine(FillAndScanGrid());
        }
        
        else
            StartCoroutine(FillAndScanGrid());
    }

    private IEnumerator FillAndScanGrid()
    {
        //Collapse and repopulate the grid
        CollapseGrid();
        yield return StartCoroutine(PopulateGrid(true));

        //Scan the grid for chain reactions
        if (ScanForMatches())
            //Collapse, repopulate and scan again
            StartCoroutine(FillAndScanGrid());
        else
        {
            CheckPossibleMoves();
        }

    }

    public void CheckPossibleMoves()
    {
        if(ScanForMoves() == 0)
        {
            //no moves!
            GameManager.Instance.NoMoreMoves();
        }
        else
        {
            //offer a hint
            hint.StartAutoHint(possibleMoves[Random.Range(0,possibleMoves.Count)].transform);
        }
    }

    private Match GetMatch(Matchable toMatch)
    {
        Match match = new Match(toMatch);

        Match horizontalMatch,
              verticalMatch;

        //First get horizontal matches to the left and right
        horizontalMatch = GetMatchesInDirection(match, toMatch, Vector2Int.left);
        horizontalMatch.Merge(GetMatchesInDirection(match, toMatch, Vector2Int.right));

        horizontalMatch.orientation = Orientation.horizontal;

        if(horizontalMatch.Count > 1)
        {
            match.Merge(horizontalMatch);

            //Scan for vertical branches
            GetBranches(match, horizontalMatch, Orientation.vertical);
        }

        //Then get vertical matches up and down
        verticalMatch = GetMatchesInDirection(match, toMatch, Vector2Int.up);
        verticalMatch.Merge(GetMatchesInDirection(match, toMatch, Vector2Int.down));

        verticalMatch.orientation = Orientation.vertical;

        if (verticalMatch.Count > 1)
        {
            match.Merge(verticalMatch);

            //Scan for horizontal branches
            GetBranches(match, verticalMatch, Orientation.horizontal);
        }

        if (match.Count == 1)
            return null;

        return match;
    }

    public void GetBranches(Match tree, Match branchToSearch, Orientation perpendicular)
    {
        Match branch;

        foreach(Matchable matchable in branchToSearch.Matchables)
        {
            branch = GetMatchesInDirection(tree, matchable, perpendicular == Orientation.horizontal ? Vector2Int.left : Vector2Int.down);
            branch.Merge(GetMatchesInDirection(tree, matchable, perpendicular == Orientation.horizontal ? Vector2Int.right : Vector2Int.up));

            branch.orientation = perpendicular;

            if(branch.Count > 1)
            {
                tree.Merge(branch);
                GetBranches(tree, branch, perpendicular == Orientation.horizontal ? Orientation.vertical : Orientation.horizontal);
            }
        }
    }

    //Add each matching matchable in the direction to a match and return it
    private Match GetMatchesInDirection(Match tree, Matchable toMatch, Vector2Int direction)
    {
        Match match = new Match();

        //First look to the left
        Vector2Int position = toMatch.position + direction;

        Matchable next;

        while (CheckBounds(position) && !IsEmpty(position))
        {
            next = GetItemAt(position);

            if (next.Type == toMatch.Type && next.Idle)
            {
                if (!tree.Contains(next))
                    match.AddMatchable(next);
                else
                    match.AddUnlisted();
                position += direction;
            }
            else
                break;
        }
        return match;
    }

    public bool SwapWasValid()
    {
        return true;
    }

    private IEnumerator Swap(Matchable[] toBeSwapped)
    {

        //Swap them in the grid data structure
        SwapItemsAt(toBeSwapped[0].position, toBeSwapped[1].position);

        //Tell the matchables their new position
        Vector2Int temp = toBeSwapped[0].position;
        toBeSwapped[0].position = toBeSwapped[1].position;
        toBeSwapped[1].position = temp;

        //Get the world positions of both
        Vector3[] worldPosition = new Vector3[2];
        worldPosition[0] = toBeSwapped[0].transform.position;
        worldPosition[1] = toBeSwapped[1].transform.position;

        //Play swap sound
        audioMixer.PlaySound(SoundEffects.swap);

        //Move them to their new positions on screen
                     StartCoroutine(toBeSwapped[0].MoveToPosition(worldPosition[1]));
        yield return StartCoroutine(toBeSwapped[1].MoveToPosition(worldPosition[0]));
    }

   
    public void CollapseGrid()
    {
        /*
    * Go through each column left to right,
    * Search from bottom up to find an empty space,
    * then look above the empty space, and up through the rest of the column,
    * until you find a non empty space.
    * Move the matchable at the non empty space into the empty space,
    * then continue looking for empty spaces
    */

        for (int x = 0; x < Dimensions.x; x++)
        {
            for(int y = 0; y < Dimensions.y-1; y++)
            {
                if(IsEmpty(x,y))
                    for (int yEmpty = 0; yEmpty < Dimensions.y; yEmpty++)
                    {
                        if(IsEmpty(x, yEmpty))
                            for(int yNotEmpty = yEmpty + 1; yNotEmpty < Dimensions.y; yNotEmpty++)
                            {
                                if(!IsEmpty(x,yNotEmpty) && GetItemAt(x,yNotEmpty).Idle)
                                {
                                    MoveMatchableToPosition(GetItemAt(x, yNotEmpty), x, yEmpty);
                                    break;
                                }
                            }
                    }
            }
        }
    }

    private void MoveMatchableToPosition(Matchable toMove, int x, int y)
    {
        //Move the matchable to its new position in the grid
        MoveItemTo(toMove.position, new Vector2Int(x, y));

        //Update the matchable's internal grid position
        toMove.position = new Vector2Int(x, y);

        //Start animation to move it screen
        StartCoroutine(toMove.MoveToPosition(transform.position + new Vector3(x, y)));

        audioMixer.PlayDelayedSound(SoundEffects.land, 1f / toMove.Speed);
    }

    //Scan the grid for any matches and resolve them
    private bool ScanForMatches()
    {
        bool madeAMatch = false;
        Matchable toMatch;
        Match match;

        //Iterate through the grid, looking for non-empty and idle matchables
        for(int y = 0; y < Dimensions.y; y++)
        {
            for(int x = 0; x < Dimensions.x; x++)
                if(!IsEmpty(x,y))
                {
                    toMatch = GetItemAt(x, y);

                    if (!toMatch.Idle)
                        continue;

                    //Try to match and resolve
                    match = GetMatch(toMatch);

                    if (match != null)
                    {
                        madeAMatch = true;
                        StartCoroutine(score.ResolveMatch(match));
                    }
                    
                }

        }

        return madeAMatch;
    }

    //Make a match of all matchables adjacent to this powerup on the grid and resolve it
    public void MatchAllAdjacent(Matchable powerup)
    {
        Match allAdjacent = new Match();

        for (int y = powerup.position.y - 1 ; y < powerup.position.y + 2; y++)
            for (int x = powerup.position.x - 1 ; x < powerup.position.x + 2; x++)
                if(CheckBounds(x,y) && !IsEmpty(x,y) && GetItemAt(x, y).Idle)
                    allAdjacent.AddMatchable(GetItemAt(x, y));

        StartCoroutine(score.ResolveMatch(allAdjacent, MatchType.match4));

        //Play powerup sound
        audioMixer.PlaySound(SoundEffects.powerup);

    }

    //Make a match of everything in the row and column that contains the powerup and resolve it
    public void MatchRowAndColumn(Matchable powerup)
    {
        Match rowAndColumn = new Match();

        for (int y = 0; y < Dimensions.y + 2; y++)
            if (CheckBounds(powerup.position.x, y) && !IsEmpty(powerup.position.x, y) && GetItemAt(powerup.position.x, y).Idle)
                rowAndColumn.AddMatchable(GetItemAt(powerup.position.x, y));

        for (int x = 0; x < Dimensions.x + 2; x++)
             if (CheckBounds(x, powerup.position.y) && !IsEmpty(x, powerup.position.y) && GetItemAt(x, powerup.position.y).Idle)
                 rowAndColumn.AddMatchable(GetItemAt(x, powerup.position.y));

        StartCoroutine(score.ResolveMatch(rowAndColumn, MatchType.cross));

        //Play powerup sound
        audioMixer.PlaySound(SoundEffects.powerup);

    }

    //Match everything on the grid wit a specific type and resolve it
    public void MatchEverythingByType(Matchable gem, int type)
    {
        Match everythingByType = new Match(gem);

        for (int y = 0; y < Dimensions.y; y++)
            for (int x = 0; x < Dimensions.x; x++)
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle && GetItemAt(x,y).Type == type)
                    everythingByType.AddMatchable(GetItemAt(x, y));

        StartCoroutine(score.ResolveMatch(everythingByType, MatchType.match5));
        StartCoroutine(FillAndScanGrid());

        //Play powerup sound
        audioMixer.PlaySound(SoundEffects.powerup);
    }
    //Match everything on the grid and resolve it
    public void MatchEverything()
    {
        Match everything = new Match();

        for (int y = 0; y < Dimensions.y; y++)
            for (int x = 0; x < Dimensions.x; x++)
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle)
                    everything.AddMatchable(GetItemAt(x, y));
         
        StartCoroutine(score.ResolveMatch(everything, MatchType.match5));
        StartCoroutine(FillAndScanGrid());
    }

    //Scan for all possible moves
    private int ScanForMoves()
    {
        possibleMoves = new List<Matchable>();

        //Scan through the entire grid
        for (int y = 0; y < Dimensions.y; y++)
            for (int x = 0; x < Dimensions.x; x++)
                if (CheckBounds(x, y) && !IsEmpty(x, y) && CanMove(GetItemAt(x, y)))
                    possibleMoves.Add(GetItemAt(x, y));

        return possibleMoves.Count;
    }

    //Check if this matchable can move tp form a valid match
    private bool CanMove(Matchable toCheck)
    {
        if (CanMove(toCheck,Vector2Int.up) ||
            CanMove(toCheck, Vector2Int.right) ||
            CanMove(toCheck, Vector2Int.down) ||
            CanMove(toCheck, Vector2Int.left))
            return true;

        if (toCheck.IsGem)
            return true;

        return false;
    }

    //Can this matchable move in 1 direction?
    private bool CanMove(Matchable toCheck, Vector2Int direction)
    {
        //Look 2 and 3 positions away straight ahead
        Vector2Int position1 = toCheck.position + direction * 2,
                   position2 = toCheck.position + direction * 3;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        //What is the clockwise direction?
        /*     
         *  Clockwise(cw) x = y, y = -x
         *  Counter Clockwise(ccw) x = -y, y = x
         */
        Vector2Int cw = new Vector2Int(direction.y, -direction.x),
                    ccw = new Vector2Int(-direction.y, direction.x);

        //Look diagonally clockwise
        position1 = toCheck.position + direction + cw;
        position2 = toCheck.position + direction + cw * 2;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        //Look diagonally both ways
        position2 = toCheck.position + direction + ccw;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        //Look diagonally counterclockwise
        position1 = toCheck.position + direction + ccw * 2;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;


        return false;
    }

    private bool IsAPotentialMatch(Matchable toCompare, Vector2Int position1, Vector2Int position2)
    {
        if (CheckBounds(position1) && CheckBounds(position2) &&
            !IsEmpty(position1) && !IsEmpty(position2) &&
            GetItemAt(position1).Idle && GetItemAt(position2).Idle &&
            GetItemAt(position1).Type == toCompare.Type && GetItemAt(position2).Type == toCompare.Type)
            return true;

        return false;
    }

    //Show a hint to the player
    public void ShowHint()
    {
        hint.IndicateHint(possibleMoves[Random.Range(0, possibleMoves.Count)].transform);
    }

}
