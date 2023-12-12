using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Orientation
{
    none,
    horizontal,
    vertical,
    both
}

public enum MatchType
{
    invalid,
    match3,
    match4,
    match5,
    cross
}
/*
 * This is a collection of Matchables that have been matched
 */

public class Match
{
    private int unlisted = 0;

    //Is this match horizontal or vertical?
    public Orientation orientation = Orientation.none;

    private List<Matchable> matchables;
    public List<Matchable> Matchables { get => matchables; }

    private Matchable toBeUpgraded = null;
    //Get the matchable to be upgraded
    public Matchable ToBeUpgraded
    {
        get
        {
            if (toBeUpgraded != null)
                return toBeUpgraded;
            else
                return matchables[Random.Range(0, matchables.Count)];
        }
    }

    public int Count { get => matchables.Count + unlisted; }

    public bool Contains(Matchable toCompare)
    {
        return matchables.Contains(toCompare);
    }

    public Match()
    {
        matchables = new List<Matchable>();
    }

    public Match(Matchable original) : this()
    {
        AddMatchable(original);

        toBeUpgraded = original;
    }

    //Get the type of the match
    public MatchType Type
    {
        get
        {
            if (orientation == Orientation.both)
                return MatchType.cross;

            else if (matchables.Count > 4)
                return MatchType.match5;

            else if (matchables.Count == 4)
                return MatchType.match4;

            else if (matchables.Count == 3)
                return MatchType.match3;

            else
                return MatchType.invalid;
        }
    }

    public void AddMatchable(Matchable toAdd)
    {
        matchables.Add(toAdd);
    }

    public void AddUnlisted()
    {
        unlisted++;
    }

    //Remove a matchable from the list
    public void RemoveMatchable(Matchable toBeRemoved)
    {
        matchables.Remove(toBeRemoved);
    }

    public void Merge(Match toMerge)
    {
        matchables.AddRange(toMerge.matchables);

        //Update the match orientation

        if (orientation == Orientation.both || toMerge.orientation == Orientation.both ||
            (orientation == Orientation.horizontal && toMerge.orientation == Orientation.vertical) ||
            (orientation == Orientation.vertical && toMerge.orientation == Orientation.horizontal))
            orientation = Orientation.both;

        else if (toMerge.orientation == Orientation.horizontal)
            orientation = Orientation.horizontal;

        else if (toMerge.orientation == Orientation.vertical)
            orientation = Orientation.vertical;
    }

    public override string ToString()
    {
        string s = "Match of type " + matchables[0].Type + " : ";

        foreach (Matchable m in matchables)
        {
            s += "(" + m.position.x + ", " + m.position.y + ") ";
        }

        return s;
    }
}
