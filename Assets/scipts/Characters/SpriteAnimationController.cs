using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimationController : MonoBehaviour
{
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] DirectionFacing defaultDirection = DirectionFacing.Down;
    //parameters
    public float moveX {get; set;}
    public float moveY {get; set;}
    public bool isMoving {get; set;}
    //states
    SpriteAnimator walkUpAnimation;
    SpriteAnimator walkDownAnimation;
    SpriteAnimator walkRightAnimation;
    SpriteAnimator walkLeftAnimation;

    SpriteAnimator currentAnimation;
    bool previouslyMoving;
    //references
    SpriteRenderer spriteRenderer;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkUpAnimation = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkDownAnimation = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkLeftAnimation = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        walkRightAnimation = new SpriteAnimator(walkRightSprites, spriteRenderer);
        SetFacingDirection(defaultDirection);

        currentAnimation = walkDownAnimation;
    }
    private void Update(){
        var previousAnimation = currentAnimation;

        if(moveX == 1)
            currentAnimation = walkRightAnimation;
        else if(moveX == -1)
            currentAnimation = walkLeftAnimation;
        else if(moveY == 1)
            currentAnimation = walkUpAnimation;
        else if(moveY == -1)
            currentAnimation = walkDownAnimation;

        if(currentAnimation != previousAnimation || isMoving != previouslyMoving)
            currentAnimation.Start();

        if (isMoving)
            currentAnimation.HandleUpdate();
        else 
            spriteRenderer.sprite = currentAnimation.Frames[0];

        previouslyMoving = isMoving;
    }

    public void SetFacingDirection(DirectionFacing direction){
        if (direction == DirectionFacing.Right)
            moveX = 1;
        else if (direction == DirectionFacing.Left)
            moveX = -1;
        else if (direction == DirectionFacing.Down)
            moveY = -1;
        else if (direction == DirectionFacing.Up)
            moveY = 1;
    }
    public DirectionFacing DefaultDirection{
        get{
            return defaultDirection;
        }
    }
}
public enum DirectionFacing{Up, Down, Left, Right}
