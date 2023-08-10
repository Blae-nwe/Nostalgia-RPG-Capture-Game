using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition 
{
    public ConditionID ID {get;set;}
    public string Name {get;set;}
    public string Description {get;set;}
    public string StartMessage {get;set;}

    public Action<Monster> OnStart{get;set;}
    public Func<Monster, bool> OnBeforeMove {get;set;}
    public Action<Monster> OnAfterTurn {get;set;}
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
