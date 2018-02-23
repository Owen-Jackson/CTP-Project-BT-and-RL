using System.Collections;
using System.Collections.Generic;
using BT_and_RL.Behaviour_Tree;
using UnityEngine;

//This decorator will have a countdown timer between when it runs its children
public class Timer : BTDecorator {
    float countdown;
    float timer = 0f;
    bool ready = true;

    public Timer(BTTask child, float countdownAmount) : base(child)
    {
        taskName = "Timer";
        countdown = countdownAmount;
    }

    public override StatusValue Tick()
    {
        if(ready)
        {
            status = child.Tick();
            if(status == StatusValue.SUCCESS)
            {
                ready = false;
                timer = countdown;
            }
        }
        else
        {
            Countdown();
            if (timer <= 0f)
            {
                ready = true;
            }
            status = StatusValue.FAILED;
        }
        return status;
    }

	private void Countdown()
    {
        timer -= Time.deltaTime;
    }
}
