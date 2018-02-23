using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

public class CheckInRange : BTCondition {
    Vector3 startPos;
    Vector3 endPos;
    float distance;

    CheckInRange(Vector3 start, Vector3 end, float minDist)
    {
        startPos = start;
        endPos = end;
        distance = minDist;
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
        if(Vector3.Distance(startPos, endPos) <= distance)
        {
            return true;
        }
        return false;
    }

}
