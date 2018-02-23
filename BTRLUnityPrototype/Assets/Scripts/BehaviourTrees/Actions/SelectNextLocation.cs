using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

//This action selects the next target position for the AI to move towards
public class SelectNextLocation : BTAction {
    List<GameObject> targets;
    GameObject owner;
    int currentIndex = 0;

    SelectNextLocation(List<GameObject> _targets, GameObject AI)
    {
        targets = _targets;
        owner = AI;
        taskName = "Select Next Location";
    }

    public override StatusValue PerformAction()
    {
        if(targets.Count == 0)
        {
            return StatusValue.FAILED;
        }
        if(currentIndex == targets.Count - 1)
        {
            currentIndex = 0;
        }
        else
        {
            currentIndex++;
        }
        return StatusValue.SUCCESS;
    }
}
