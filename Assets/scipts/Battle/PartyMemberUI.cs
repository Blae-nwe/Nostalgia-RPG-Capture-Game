using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color highlightedColour;

    Monster _monster;

    public void SetData(Monster monster){
        _monster = monster;

        nameText.text = monster.Base.Name;
        levelText.text = "Lvl" + monster.Level;
        hpBar.SetHP((float)monster.HP/monster.MaxHP);
    }

    public IEnumerator UpdateHP(){
        yield return hpBar.SetHPSmooth((float)_monster.HP / _monster.MaxHP);
    }

    public void SetSelected(bool selected){
        if(selected)
            nameText.color = highlightedColour;
        else    
            nameText.color = Color.black;
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
