using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Party : MonoBehaviour
{
    [SerializeField] List<Monster> monsters;
    public event Action OnUpdated;

    public List<Monster> Monsters{
        get{
            return monsters;
        }
    }
    // Start is called before the first frame update
    private void Start()
    {
        foreach (var monster in monsters){
            monster.Init();
        }
    }

    public Monster GetHealthyMonster(){
        return monsters.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddMonster(Monster newMonster){
        if(monsters.Count < 6){
            monsters.Add(newMonster);
        }
        else{
            //add monster to pc
        }
    }
    public void PartyUpdated(){
        OnUpdated?.Invoke();
    }
}
