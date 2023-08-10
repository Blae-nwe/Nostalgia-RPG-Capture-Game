using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    NPCState state;
    float idleTime;
    int curPattern;

    Character character;
    Healer healer;

    private void Awake(){
        character = GetComponent<Character>();
        healer = GetComponent<Healer>();
    }

    public void Interact(Transform initiator){
        if (state == NPCState.Idle){
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);
            if(healer!=null){
                StartCoroutine(healer.Heal(initiator, dialog));
            }
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, ()=>{idleTime = 0f;state = NPCState.Idle;}));
            Debug.Log("interacting with NPC");
            
        }
    }

    private void Update(){
        if(state == NPCState.Idle){
            idleTime += Time.deltaTime;
            if(idleTime > timeBetweenPattern){
                idleTime = 0f;
                if(movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();
    }
    IEnumerator Walk(){
        state = NPCState.Walking;
        var prevPos = transform.position;
        yield return character.Move(movementPattern[curPattern]);
        if(transform.position != prevPos){
            curPattern = (curPattern + 1) % movementPattern.Count;
        }
        state = NPCState.Idle;
    }
}
public enum NPCState{Idle, Walking, Dialog}
