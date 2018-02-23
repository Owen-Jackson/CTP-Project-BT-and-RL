using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DisplayFireVariables : MonoBehaviour
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
        text.text = "Fire Elementals\n";
        text.text += "Attacks Used    Q Values\n";
        foreach (KeyValuePair<string, int> pair in Environment.Instance.FireEleAttacked)
        {
            string straightenStr = "";
            for (int i = pair.Key.Count(); i < 5; i++)
            {
                straightenStr += " ";
            }
            text.text += pair.Key + ":   " + straightenStr + pair.Value + "             " + (Environment.Instance.FireEleQValues[pair.Key] / Environment.Instance.FireEleQValues.Max(x => x.Value)) + "\n";
        }
    }
}
