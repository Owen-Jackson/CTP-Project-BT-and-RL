using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

public class CheckOutOfRange : BTCondition {
    Vector3 startPos;
    Vector3 endPos;
    float distance;

    CheckOutOfRange(Vector3 start, Vector3 end, float maxDist)
    {
        startPos = start;
        endPos = end;
        distance = maxDist;
    }

    public override StatusValue Tick()
    {
        if(CheckCondition())
        {
            return StatusValue.SUCCESS;
        }
        else
        {
            return StatusValue.FAILED;
        }
    }

    public override bool CheckCondition()
    {
        if(Vector3.Distance(startPos, endPos) > distance)
        {
            return true;
        }
        return false;
    }

}
