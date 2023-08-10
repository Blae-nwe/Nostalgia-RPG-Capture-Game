using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject xpBar;
    
    [SerializeField] Color psnColour;
    [SerializeField] Color brnColour;
    [SerializeField] Color parColour;
    [SerializeField] Color slpColour;
    [SerializeField] Color frzColour;

    Monster _monster;
    Dictionary<ConditionID, Color> statusColours;

    public void SetData(Monster monster){
        _monster = monster;

        nameText.text = monster.Base.Name;
        Debug.Log($"{monster.Base.Name}");
        Setlvl();
        hpBar.SetHP((float)monster.HP/monster.MaxHP);
        SetXP();

        statusColours = new Dictionary<ConditionID, Color>(){
            {ConditionID.psn, psnColour},
            {ConditionID.brn, brnColour},
            {ConditionID.par, parColour},
            {ConditionID.slp, slpColour},
            {ConditionID.frz, frzColour}
        };

        SetStatusText();
        _monster.OnStatusChanged += SetStatusText;
    }

    public IEnumerator UpdateHP(){
        if(_monster.HpChanged){
            yield return hpBar.SetHPSmooth((float)_monster.HP / _monster.MaxHP);
            _monster.HpChanged = false;
        }
    }

    public void SetXP(bool reset=false){
        if(xpBar == null) return;
        if(reset)
            xpBar.transform.localScale = new Vector3(0, 1 , 1);
        float normalizedXP = GetNormalizedXP();
        xpBar.transform.localScale = new Vector3(normalizedXP, 1 , 1);
    }
    float GetNormalizedXP(){
        int curLvlXP = _monster.Base.GetXpForLevel(_monster.Level);
        int nextLvlXP = _monster.Base.GetXpForLevel(_monster.Level + 1);

        float normalizedXP = (float)(_monster.Xp - curLvlXP) / (nextLvlXP - curLvlXP);
        return Mathf.Clamp01(normalizedXP);
    }

    public void Setlvl(){
        levelText.text = "Lvl: " + _monster.Level;
    }

    void SetStatusText(){
        if(_monster.Status == null){
            statusText.text = "";
        }
        else{
            statusText.text = _monster.Status.ID.ToString().ToUpper();
            statusText.color = statusColours[_monster.Status.ID];
        }
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
