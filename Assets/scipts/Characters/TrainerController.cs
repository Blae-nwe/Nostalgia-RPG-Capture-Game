using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterDefeat;
    [SerializeField] GameObject alert;
    [SerializeField] GameObject fov;
    [SerializeField] Sprite sprite;
    [SerializeField] string trainerName;
    Character character;

    //state of trainer
    bool battleLost = false;

    private void Awake(){
        character = GetComponent<Character>();
    }
    private void Start(){
        SetFOVRotation(character.SpriteAnimator.DefaultDirection);
    }
    private void Update() {
        character.HandleUpdate();
    }
    public void Interact(Transform initiator)
        {
            character.LookTowards(initiator.position);
            if(!battleLost){
                StartCoroutine( DialogManager.Instance.ShowDialog(dialog, ()=>{
                GameController.Instance.StartTrainerBattle(this);
                Debug.Log("starting trainer battle");
                }));
            }
            else{
                StartCoroutine( DialogManager.Instance.ShowDialog(dialogAfterDefeat));
            }
        }

    public IEnumerator TriggerTrainerBattle(PlayerController player){
        alert.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        alert.SetActive(false);

        var difference = player.transform.position - transform.position;
        var moveVector = difference - difference.normalized;

        moveVector = new Vector2(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));

        yield return character.Move(moveVector);

        StartCoroutine( DialogManager.Instance.ShowDialog(dialog, ()=>{
            GameController.Instance.StartTrainerBattle(this);
            Debug.Log("starting trainer battle");
        }));
    }

    public void LostBattle(){
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    public void SetFOVRotation(DirectionFacing direction){
        float angle = 0f;
        if(direction == DirectionFacing.Right)
            angle = 90f;
        else if(direction == DirectionFacing.Up)
            angle = 180f;
        else if(direction == DirectionFacing.Left)
            angle = 270f;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public string Name{
        get{
            return trainerName;
        }
    }
    public Sprite Sprite{
        get{
            return sprite;
        }
    }
}
