using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator
{
    SpriteRenderer spriteRenderer;
    List<Sprite> frames;
    float fps;

    int currentFrame;
    float timer;

    public SpriteAnimator(List<Sprite> frames, SpriteRenderer spriteRenderer, float fps=0.16f){
        this.frames = frames;
        this.spriteRenderer = spriteRenderer;
        this.fps = fps;
    }
    // Start is called before the first frame update
    public void Start()
    {
        currentFrame = 0;
        timer = 0;
        spriteRenderer.sprite = frames[0];
    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        if(timer > fps){
            currentFrame = (currentFrame + 1) % frames.Count;
            spriteRenderer.sprite = frames[currentFrame];
            timer -= fps;
        }
    }

    public List<Sprite> Frames{
        get{return frames;}
    }
}
