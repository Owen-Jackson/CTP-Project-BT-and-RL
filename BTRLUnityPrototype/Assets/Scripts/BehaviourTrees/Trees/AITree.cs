using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

public class AITree : BTTree {
    public AITree(BTTask task) : base(task)
    {
        child = task;
    }
    
    public void AddAction(BTTask action, BTTask parent)
    {
        
    }

}
