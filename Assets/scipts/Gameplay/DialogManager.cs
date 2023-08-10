using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject textDialogBox;
    [SerializeField] Text dialogBoxText;
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    public static DialogManager Instance {get; private set;}

    private void Awake(){
        Instance = this;
    }

    Dialog dialog;
    Action onFinishedDialog;
    int curLine = 0;

    bool isTypingText;

    public bool IsShowing{get;set;}

    public IEnumerator ShowDialog(Dialog dialog, Action onFinished=null){
        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();

        IsShowing = true;
        this.dialog = dialog;
        onFinishedDialog = onFinished;
        textDialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));
    }

    public void HandleUpdate(){
        if(Input.GetKeyDown(KeyCode.Z) && !isTypingText){
            ++curLine;
            if(curLine < dialog.Lines.Count){
                StartCoroutine(TypeDialog(dialog.Lines[curLine]));

            }
            else{
                curLine = 0;
                IsShowing = false;
                textDialogBox.SetActive(false);
                onFinishedDialog?.Invoke();
                OnCloseDialog?.Invoke();
            }
        }
    }

    public IEnumerator TypeDialog(string dialogLine){
        isTypingText = true;
        dialogBoxText.text = "";
        foreach (var letter in dialogLine.ToCharArray()){
            dialogBoxText.text += letter;
            yield return new WaitForSeconds(1f/lettersPerSecond);
        }
        isTypingText = false;
    }
}



