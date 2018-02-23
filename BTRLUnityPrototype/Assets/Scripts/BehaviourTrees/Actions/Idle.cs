using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

public class Idle : BTTask
{
    public Idle()
    {
        taskName = "Idle";
    }

    float timer = 0;
    public override StatusValue Tick()
    {
        if (Mathf.FloorToInt(timer) % 2 == 0)
        {
            Debug.Log("I am Idle");
        }

        if(timer >= 5.0f)
        {
            timer = 0;
            Debug.Log("success");
            return StatusValue.SUCCESS;
        }

        timer += Time.deltaTime;
        return StatusValue.RUNNING;
    }
}
