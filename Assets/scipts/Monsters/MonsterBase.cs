using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster/Create new monster")]

public class MonsterBase : ScriptableObject
{
    //name and description of monster
    [SerializeField] string monsterName;

    [TextArea]
    [SerializeField] string description;
    //sprites
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    //typing
    [SerializeField] MonsterType type1;
    [SerializeField] MonsterType type2;

    //base stats
    [SerializeField] int maxHP;
    [SerializeField] int defence;
    [SerializeField] int attack;
    [SerializeField] int speed;
    [SerializeField] int captureRate = 255;
    [SerializeField] int xpYield;
    [SerializeField] GrowthRates growthRate;

    [SerializeField] List<LearnableMove> learnableMoves;

    public int GetXpForLevel(int level){
        if(growthRate == GrowthRates.Fast){
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRates.Medium){
            return level * level * level;
        }
        else if (growthRate == GrowthRates.Slow){
            return 5 * (level * level * level) / 4;
        }
        return -1;
    }

    public string Name{
        get{return monsterName;}
    }
    public string Description{
        get{return description;}
    }
    public Sprite FrontSprite{
        get{return frontSprite;}
    }
    public Sprite BackSprite{
        get{return backSprite;}
    }
    public MonsterType Type1{
        get{return type1;}
    }
    public MonsterType Type2{
        get{return type2;}
    }
    public int MaxHP{
        get{return maxHP;}
    }
    public int Defence{
        get{return defence;}
    }
    public int Attack{
        get{return attack;}
    }
    public int Speed{
        get{return speed;}
    }
    public int CaptureRate{
        get{return captureRate;}
    }
    public int XPYield{
        get{return xpYield;}
    }
    public GrowthRates GrowthRate{
        get{return growthRate;}
    }

    public List<LearnableMove> LearnableMoves{
        get{return learnableMoves;}
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
public class LearnableMove{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base{
        get{return moveBase;}
    }
    public int Level{
        get{return level;}
    }
}

public enum MonsterType
{
    None,
    Dark,
    Grass,
    Fire,
    Water,
    Ground,
    Normal,
    Metal,
    Fighting,
    Thunder,
    Void,
    Light,
    Flying,
    Poison
}

public enum GrowthRates{
    Fast,
    Medium,
    Slow
}

public enum Stat{
    Attack,
    Defence,
    Speed,
    //not actual stats
    Accuracy,
    Evasion
}

public class TypeChart{
    static float[][] chart = {
        //                          DAR      GRA      FIR      WAT      GRO      NOR      MET      FIG     THU      VOI      LIG      FLY      POS
        /*Dark*/ new float[]    {   0.5f ,   1f   ,   1f   ,   1f   ,   1f   ,   2f   ,   1f   ,  0.5f ,   1f   ,   1f   ,   2f   ,   1f   ,   1f   },
        /*Grass*/ new float[]   {   1f   ,   0.5f ,   0.5f ,   2f   ,   2f   ,   1f   ,   0.5f ,  1f   ,   0.5f ,   0.5f ,   1f   ,   0.5f ,   1f   },
        /*Fire*/ new float[]    {   1f   ,   2f   ,   0.5f ,   0.5f ,   0.5f ,   1f   ,   2f   ,  1f   ,   1f   ,   0.5f ,   1f   ,   1f   ,   1f   },
        /*Water*/ new float[]   {   1f   ,   0.5f ,   2f   ,   0.5f ,   2f   ,   1f   ,   0.5f ,  1f   ,   1f   ,   1f   ,   1f   ,   1f   ,   1f   },
        /*Ground*/ new float[]  {   1f   ,   0.5f ,   2f   ,   1f   ,   1f   ,   1f   ,   2f   ,  1f   ,   2f   ,   1f   ,   1f   ,   0f   ,   1f   },
        /*Normal*/ new float[]  {   1f   ,   1f   ,   1f   ,   1f   ,   1f   ,   1f   ,   0.5f ,  1f   ,   1f   ,   0f   ,   1f   ,   1f   ,   1f   },
        /*Metal*/ new float[]   {   1f   ,   1f   ,   0.5f ,   0.5f ,   2f   ,   1f   ,   0.5f ,  1f   ,   0.5f ,   1f   ,   2f   ,   1f   ,   1f   },
        /*Fighting*/ new float[]{   2f   ,   1f   ,   1f   ,   1f   ,   2f   ,   2f   ,   2f   ,  1f   ,   1f   ,   0f   ,   0.5f ,   0.5f ,   1f   },
        /*Thunder*/ new float[] {   1f   ,   1f   ,   1f   ,   2f   ,   0f   ,   1f   ,   1f   ,  1f   ,   0.5f ,   1f   ,   2f   ,   2f   ,   1f   },
        /*Void*/ new float[]    {   1f   ,   2f   ,   2f   ,   1f   ,   1f   ,   2f   ,   1f   ,  2f   ,   1f   ,   2f   ,   2f   ,   1f   ,   1f   },
        /*Light*/ new float[]   {   2f   ,   1f   ,   0.5f ,   1f   ,   1f   ,   1f   ,   0.5f ,  2f   ,   1f   ,   2f   ,   0.5f ,   1f   ,   1f   },
        /*Flying*/ new float[]  {   1f   ,   2f   ,   1f   ,   1f   ,   0.5f ,   1f   ,   0.5f ,  2f   ,   1f   ,   1f   ,   1f   ,   1f   ,   1f   },
        /*Poison*/ new float[]  {   1f   ,   2f   ,   1f   ,   1f   ,   0.5f ,   1f   ,   0f   ,  1f   ,   1f   ,   0.5f ,   2f   ,   1f   ,   0.5f }
    };

    public static float GetEffectiveness(MonsterType attackType, MonsterType defenceType){
        if (attackType == MonsterType.None || defenceType == MonsterType.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenceType - 1;

        return chart[row][col];
    }
}