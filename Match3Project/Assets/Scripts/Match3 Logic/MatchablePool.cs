using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField]private int howManyTypes;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Color[] colors;

    [SerializeField] private Sprite match4PowerUp;
    [SerializeField] private Sprite match5PowerUp;
    [SerializeField] private Sprite crossPowerup;

    public void RandomizeType(Matchable toRandomize)
    {
        int random = Random.Range(0, howManyTypes);
        toRandomize.SetType(random, sprites[random], colors[random]);
    }

    public Matchable GetRandomMatchable()
    {
        Matchable randomMatchable = GetPooledObject();

        RandomizeType(randomMatchable);

        return randomMatchable;
    }

    public int NextType(Matchable matchable)
    {
        int nextType = (matchable.Type + 1) % howManyTypes;

        matchable.SetType(nextType, sprites[nextType], colors[nextType]);

        return nextType;
    }

    public Matchable UpgradeMatchable(Matchable toBeUpgraded, MatchType type)
    {
        if (type == MatchType.cross)
            return toBeUpgraded.Upgrade(MatchType.cross, crossPowerup);
        if(type == MatchType.match4)
            return toBeUpgraded.Upgrade(MatchType.match4, match4PowerUp);
        if (type == MatchType.match5)
            return toBeUpgraded.Upgrade(MatchType.match5, match5PowerUp);

        return toBeUpgraded;
    }

    public void ChangeType(Matchable toChange, int type)
    {
        toChange.SetType(type, sprites[type], colors[type]);
    }
}
