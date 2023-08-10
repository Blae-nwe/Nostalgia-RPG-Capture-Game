using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //event to start battles etc
    public event Action OnEncountered;
    public event Action<Collider2D> OnEnterTrainerFOV;

    // private variables
    private Vector2 input;
    private Character character;
    [SerializeField] Sprite sprite;
    [SerializeField] string playerName;
    
    //used to help determine the current player animation
    private void Awake(){
        character = GetComponent<Character>();
    }
    // Update is called once per frame
    public void HandleUpdate()
    {
        if (!character.isMoving)
        {
            //gets x and y input and saves it
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // remove diagonal movement
            if(input.x != 0) input.y = 0;

            //if the player is moving then change x and y coordinate so player can move
            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }
        character.HandleUpdate();

        if(Input.GetKeyDown(KeyCode.Z)){
            Interact();
        }
    }

    void Interact(){
        var facingDir = new Vector3(character.SpriteAnimator.moveX, character.SpriteAnimator.moveY);
        var interactPos = transform.position + facingDir;

        //line to see interaction while debugging
        //Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f);
        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.Instance.InteractablesLayer);
        if (collider != null){
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnMoveOver(){
        CheckForEncounter();
        CheckIfInTrainerFOV();
    }

    //function for wild encounters
    private void CheckForEncounter(){
        if(Physics2D.OverlapCircle(transform.position, 0.05f, GameLayers.Instance.GrassLayer) != null){
            //encounters are not all the time so only a select amount of time the player encounters monsters
            if(UnityEngine.Random.Range(1,101)<=10){
                character.SpriteAnimator.isMoving = false;
                OnEncountered();
            }
        }
    }
    private void CheckIfInTrainerFOV(){
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.Instance.FOVLayer);
        if(collider !=null){
            character.SpriteAnimator.isMoving = false;
            OnEnterTrainerFOV?.Invoke(collider);
            Debug.Log("In trainer view");
        }
    }
    public string Name{
        get{
            return playerName;
        }
    }
    public Sprite Sprite{
        get{
            return sprite;
        }
    }
}
