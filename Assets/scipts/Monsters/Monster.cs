using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

public class Monster
{
    [SerializeField] MonsterBase _base;
    [SerializeField] int level;

    public Monster(MonsterBase mbase, int mlevel){
        _base = mbase;
        level = mlevel;
        Init();
    }
    public MonsterBase Base {
        get{
            return _base;
        }
    }
    public int Level {
        get{
            return level;
        }
    }

    public int HP{get;set;}
    public int Xp{get;set;}
    public List<Move> Moves {get;set;}
    public Move CurrentMove {get;set;}
    public Dictionary<Stat, int> Stats {get; private set;}
    public Dictionary<Stat, int> StatBoosts {get; private set;}
    public Condition Status {get; private set;}
    public int StatusTime {get; set;}
    public Condition VolatileStatus{get;private set;}
    public int VolatileStatusTime {get; set;}

    public Queue<string> StatusChanges{get;private set;}
    public bool HpChanged {get;set;}
    public event System.Action OnStatusChanged;

    public void Init(){
        //generate moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if(move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= 4)
                break;
        }

        Xp = Base.GetXpForLevel(level);

        CalculateStats();
        HP = MaxHP;

        StatusChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    void CalculateStats(){
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defence, Mathf.FloorToInt((Base.Defence * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + Level;
    }

    void ResetStatBoost(){
        StatBoosts = new Dictionary<Stat, int>(){
            {Stat.Attack, 0},
            {Stat.Defence, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}
        };
    }

    int GetStat(Stat stat){
        int statVal = Stats[stat];
        //apply stat changes
        int boost = StatBoosts[stat];
        var boostValues = new float[] {1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f};

        if (boost >= 0)
            statVal = Mathf.FloorToInt( statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt( statVal / boostValues[-boost]);
        return statVal;
    }

    public void ApplyBoost(List<StatBoost> statBoosts){
        foreach( var statBoost in statBoosts){
            var stat = statBoost.stat;
            var boost = statBoost.boost;
            StatBoosts[stat] = Mathf.Clamp( StatBoosts[stat] + boost, -6, 6);

            if(boost > 0){
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else{
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }
            Debug.Log($"{Base.Name}'s {stat} has been changed by {StatBoosts[stat]}");
        }
    }

    public bool CheckForLvlUp(){
        if(Xp > Base.GetXpForLevel(level + 1)){
            ++level;
            return true;
        }
        return false;
    }

    public LearnableMove GetLearnableMoveAtLevel(){
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(LearnableMove moveToLearn){
        if(Moves.Count > 4)
            return;
        Moves.Add(new Move(moveToLearn.Base));
    }
    public void Heal(){
        HP = MaxHP;
        Status = null;
    }

    public int Attack{
        get{return GetStat(Stat.Attack);}
    }
    public int Defence{
        get{return GetStat(Stat.Defence);}
    }
    public int Speed{
        get{return GetStat(Stat.Speed);}
    }
    public int MaxHP{get;private set;}

    // basic damage calculation in pokemon
    public DamageDetails TakeDamage(Move move, Monster attacker){
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
            critical = 2f;
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails(){
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attacker.Attack / Defence) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);
        return damageDetails;
    }

    public void UpdateHP(int damage){
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        HpChanged = true;
    }

    public void SetStatus(ConditionID conditionID){
        if(Status != null) return;
        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }
    public void SetVolatileStatus(ConditionID conditionID){
        if(VolatileStatus != null) return;
        VolatileStatus = ConditionsDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public Move GetRandomMove(){
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public void CureStatus(){
        Status = null;
        OnStatusChanged?.Invoke();
    }
    public void CureVolatileStatus(){
        VolatileStatus = null;
        
    }

    public void OnAfterTurn(){
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public bool OnBeforeMove(){
        bool canPerformMove = true;
        if(Status?.OnBeforeMove != null){
            if(!Status.OnBeforeMove(this)){
                canPerformMove = false;
            }
        }
        if(VolatileStatus?.OnBeforeMove != null){
            if(!VolatileStatus.OnBeforeMove(this)){
                canPerformMove = false;
            }
        }
        return canPerformMove;
    }

    public void OnBattleOver(){
        VolatileStatus = null;
        ResetStatBoost();
    }
}

public class DamageDetails{
    public bool Fainted {get; set;}
    public float Critical {get; set;}
    public float TypeEffectiveness {get; set;}
}