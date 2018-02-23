using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoGUI : MonoBehaviour {

    GameObject AI;
    //I was about to add some GUI info - displaying the current state of the behaviour tree and what the q values of the ai are
    private void OnGUI()
    {
        //make background box
        GUI.Box(new Rect(10, 10, 100, 90), "Debug");


        DisplayParams();
    }

    void DisplayParams()
    {

    }
}
