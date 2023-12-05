using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match
{
    private List<Matchable> matchables;
    public List<Matchable> Matchables { get => matchables; }

    public int Count { get => matchables.Count; }

    public Match()
    {
        matchables = new List<Matchable>();
    }

    public Match(Matchable original) : this()
    {
        AddMatchable(original);
    }

    public void AddMatchable(Matchable toAdd)
    {
        matchables.Add(toAdd);
    }

    public void Merge(Match toMerge)
    {
        matchables.AddRange(toMerge.matchables);
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
