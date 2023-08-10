using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class for characters/sprites to move
public class Character : MonoBehaviour
{
    SpriteAnimationController spriteAnimator;
    public float moveSpeed;
    public bool isMoving{get;private set;}

    private void Awake() {
        spriteAnimator = GetComponent<SpriteAnimationController>();
    }

    //function for characters to move
    public IEnumerator Move(Vector2 moveVector, Action OnMoveOver = null)
    {
        spriteAnimator.moveX = Mathf.Clamp(moveVector.x, -1f, 1f);
        spriteAnimator.moveY = Mathf.Clamp(moveVector.y, -1f, 1f);

        //moves character
        var targetPos = transform.position;
        targetPos.x += moveVector.x;
        targetPos.y += moveVector.y;

        if(!isClearPath(targetPos))
            yield break;

        //player starts to move
        isMoving = true;
        //loop to move the character for the amount of time the player holds move button
        while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            //moves player character in the world
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        //new character position
        transform.position = targetPos;
        //player is now not moving
        isMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate(){
        spriteAnimator.isMoving = isMoving;
    }

    private bool isClearPath(Vector3 targetPos){
        var difference = targetPos - transform.position;
        var direction = difference.normalized;
        if(Physics2D.BoxCast(transform.position + direction, new Vector2(0.2f,0.2f), 0f, direction, difference.magnitude - 1, GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractablesLayer | GameLayers.Instance.PlayerLayer ) == true){
            return false;
        }
        return true;
    }

    //function to determine if tiles are walkable
    private bool IsWalkable(Vector3 targetPos){
        //collider to determine if player comes into contact with the solidObjects layer and if so tile is not walkable
        if(Physics2D.OverlapCircle(targetPos, 0.1f, GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractablesLayer ) != null){
            return false;
        }
        //otherwise player can walk on tile
        return true;
    }

    public void LookTowards(Vector3 targetPos){
        var xDifference = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var yDifference = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if(xDifference == 0 || yDifference == 0){
            spriteAnimator.moveX = Mathf.Clamp(xDifference, -1f, 1f);
            spriteAnimator.moveY = Mathf.Clamp(yDifference, -1f, 1f);
        }
        else
            Debug.LogError("You cant get NPCs to look diagonally");
    }

    public SpriteAnimationController SpriteAnimator{
        get =>spriteAnimator;
    }
}
