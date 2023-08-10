using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<Text> moveTexts;
    [SerializeField] Color highlightColour;
    int curSelect = 0;
    
    public void SetMoveData(List<MoveBase> curMoves, MoveBase newMove){
        for(int i=0; i<curMoves.Count; ++i){
            moveTexts[i].text = curMoves[i].Name;
        }
        moveTexts[curMoves.Count].text = newMove.Name;
    }

    public void HandleMoveSelection(Action<int> onSelected){
        if(Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S))
            ++curSelect;
        else if(Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.W))
            --curSelect;
        curSelect = Mathf.Clamp(curSelect, 0, 4);
        UpdateMoveSelection(curSelect);

        if(Input.GetKeyDown(KeyCode.Z))
            onSelected?.Invoke(curSelect);
    }

    public void UpdateMoveSelection(int selection){
        for(int i=0; i < 5; i++){
            if(i== selection)
                moveTexts[i].color = highlightColour;
            else
                moveTexts[i].color = Color.black;
        }
    }
}
