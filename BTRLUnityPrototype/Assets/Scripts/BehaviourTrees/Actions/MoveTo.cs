using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

public class MoveTo : BTTask {
    GameObject owner;
    List<GameObject> targets;
    int currentIndex = 0;

    public MoveTo(GameObject newOwner, List<GameObject> _targets)
    {
        owner = newOwner;
        targets = _targets;
        taskName = "MoveTo";
    }

    public override StatusValue Tick()
    {
        if (targets.Count == 0)
        {
            return StatusValue.FAILED;
        }
        if (Vector3.Distance(owner.transform.position, targets[currentIndex].transform.position) <= 1f)
        {
            Debug.Log("arrived");
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
        MoveToPosition(owner, targets[currentIndex].transform.position);

        return StatusValue.RUNNING;
    }

    void MoveToPosition(GameObject character, Vector3 pos)
    {
        Vector3 move_dir = Vector3.Normalize(pos - character.transform.position);
        move_dir.y = 0; //prevent from flying up
        character.transform.position += move_dir * 10f * Time.deltaTime;
        //Debug.Log("moving to position");
    }
}
