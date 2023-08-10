using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHUD hud;

    public BattleHUD Hud{
        get{
            return hud;
        }
    }

    public bool IsPlayerUnit{
        get{
            return isPlayerUnit;
        }
    }

    public Monster Monster {get;set;}

    public void SetUp(Monster monster){
        Monster = monster;
        if(isPlayerUnit)
            GetComponent<Image>().sprite = Monster.Base.BackSprite;
        else
        {
            GetComponent<Image>().sprite = Monster.Base.FrontSprite;
        }
        hud.gameObject.SetActive(true);
        hud.SetData(monster);
    }
    public void Clear(){
        hud.gameObject.SetActive(false);
    }
}
