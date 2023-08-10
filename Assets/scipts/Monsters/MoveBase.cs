using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Monster/Create new move")]

public class MoveBase : ScriptableObject
{
    [SerializeField] string moveName;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] MonsterType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp;
    [SerializeField] int priority;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecEffects> secEffects;
    [SerializeField] MoveTarget target;

    public string Name{
        get{return moveName;}
    }
    public string Description{
        get{return description;}
    }
    public MonsterType Type{
        get{return type;}
    }
    public int Power{
        get{return power;}
    }
    public int Accuracy{
        get{return accuracy;}
    }
    public bool AlwaysHits{
        get{return alwaysHits;}
    }
    public int PP{
        get{return pp;}
    }
    public int Priority{
        get{return priority;}
    }

    public MoveCategory Category{
        get{return category;}
    }
    public MoveEffects Effects{
        get{return effects;}
    }
    public List<SecEffects> SecEffects{
        get{return secEffects;}
    }
    public MoveTarget Target{
        get{return target;}
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]

public class MoveEffects{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;

    public List<StatBoost> Boosts{
        get{return boosts;}
    }
    public ConditionID Status{
        get{return status;}
    }
    public ConditionID VolatileStatus{
        get{return volatileStatus;}
    }
}

[System.Serializable]

public class SecEffects : MoveEffects{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance{
        get{return chance;}
    }
    public MoveTarget Target{
        get{return target;}
    }
}

[System.Serializable]

public class StatBoost{
    public Stat stat;
    public int boost;
}

public enum MoveCategory{
    Status,
    Priority,
    Standard
}

public enum MoveTarget{
    Foe, 
    Self
}