using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField]Text messageText;

    PartyMemberUI[] memberSlots;
    List<Monster> monsters;

    public void Init(){
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }
    public void SetPartyData(List<Monster> monsters){
        this.monsters = monsters;
        for(int i = 0; i < memberSlots.Length; i++){
            if (i < monsters.Count){
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(monsters[i]);
            }
            else    
                memberSlots[i].gameObject.SetActive(false);
        }
        messageText.text = "Choose a Monster";
    }

    public void UpdateMemberSelection(int selectedMember){
        for (int i = 0; i < monsters.Count; i ++){
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else    
                memberSlots[i].SetSelected(false);
        }
    }

    public void SetMessageText(string message){
        messageText.text = message;
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
