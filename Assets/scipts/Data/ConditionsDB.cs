using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init(){
        foreach(var kvp in Conditions){
            var conditionID = kvp.Key;
            var condition = kvp.Value;
            condition.ID = conditionID;
        }
    }
    public static Dictionary<ConditionID, Condition> Conditions {get;set;} = new Dictionary<ConditionID, Condition>{
        {
            ConditionID.psn,
            new Condition(){
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Monster monster) =>{
                    monster.UpdateHP(monster.MaxHP / 8);
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} took damage from the poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition(){
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Monster monster) =>{
                    monster.UpdateHP(monster.MaxHP / 16);
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} took damage from the burn");
                }
            }
        },
        {ConditionID.par,
        new Condition(){
            Name = "Paralysed",
            StartMessage = "has been paralysed",
            OnBeforeMove = (Monster monster) =>
            {
                if(Random.Range(1, 5) == 1){
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} is unable to move");
                    return false;
                }
                return true;
            }
        }},
        {ConditionID.slp,
        new Condition(){
            Name = "Sleep",
            StartMessage = "has fell asleep",
            OnStart = (Monster monster) =>{
                //sleep for 1-3 turns
                monster.StatusTime = Random.Range(1,4);
                Debug.Log($"{monster.StatusTime} turns to sleep");
            },
            OnBeforeMove = (Monster monster) =>{
                if (monster.StatusTime <= 0){
                    monster.CureStatus();
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} woke up");
                    return true;
                }
                monster.StatusTime--;
                monster.StatusChanges.Enqueue($"{monster.Base.Name} is still asleep");
                return false;
            }
        }},
        {ConditionID.frz,
        new Condition(){
            Name = "Frozen",
            StartMessage = "has been frozen",
            OnBeforeMove = (Monster monster) =>
            {
                if(Random.Range(1, 5) == 1){
                    monster.CureStatus();
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} thawed out");
                    return true;
                }
                return false;
            }
        }},
    //Volatile Status
        {ConditionID.confusion,
        new Condition(){
            Name = "Confusion",
            StartMessage = "has been confused",
            OnStart = (Monster monster) =>{
                //confusion for 1-4 turns
                monster.VolatileStatusTime = Random.Range(1,5);
                Debug.Log($"{monster.VolatileStatusTime} turns of confusion");
            },
            OnBeforeMove = (Monster monster) =>{
                if (monster.VolatileStatusTime <= 0){
                    monster.CureVolatileStatus();
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} snapped out of confusion");
                    return true;
                }
                monster.VolatileStatusTime--;

                //50% chance to do a move
                if(Random.Range(1, 3) == 1){
                    return true;
                }
                //Hurt itself from confusion
                monster.StatusChanges.Enqueue($"{monster.Base.Name} is confused");
                monster.UpdateHP(monster.MaxHP / 8);
                monster.StatusChanges.Enqueue($"{monster.Base.Name} hurt itself in confusion");
                return false;
            }
        }}
    };

    public static float GetStatusBonus(Condition condition){
        if(condition == null)
            return 1f;
        else if(condition.ID == ConditionID.slp || condition.ID == ConditionID.frz)
            return 2f;
        else if(condition.ID == ConditionID.psn || condition.ID == ConditionID.brn || condition.ID == ConditionID.par)
            return 1.5f;
        
        return 1f;
    }

    
}

public enum ConditionID{
    none, psn, brn, slp, par, frz,
    confusion
}
