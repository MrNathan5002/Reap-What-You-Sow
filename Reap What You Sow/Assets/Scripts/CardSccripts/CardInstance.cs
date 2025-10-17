using UnityEngine;

public class CardInstance
{
    private static int _nextId = 1;

    public readonly int instanceId;
    public readonly CardEditor def;
    public bool isUpgraded;

    public CardInstance(CardEditor def, bool upgraded)
    {
        this.def = def;
        this.isUpgraded = upgraded;
        instanceId = _nextId++;
    }
}
