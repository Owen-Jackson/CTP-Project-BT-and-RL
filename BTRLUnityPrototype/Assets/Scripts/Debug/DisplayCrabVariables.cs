using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DisplayCrabVariables : MonoBehaviour
{
    TextMesh text;
    // Use this for initialization
    void Start()
    {
        text = GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Mud Crabs\n";
        text.text += "Attacks Used    Q Values\n";
        foreach (KeyValuePair<string, int> pair in Environment.Instance.MudCrabAttacked)
        {
            string straightenStr = "";
            for (int i = pair.Key.Count(); i < 5; i++)
            {
                straightenStr += " ";
            }
            text.text += pair.Key + ":   " + straightenStr + pair.Value + "             " + (Environment.Instance.MudCrabQValues[pair.Key] / Environment.Instance.MudCrabQValues.Max(x => x.Value)) + "\n";
        }
    }
}
