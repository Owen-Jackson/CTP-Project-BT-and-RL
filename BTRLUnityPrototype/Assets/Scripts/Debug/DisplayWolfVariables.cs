using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DisplayWolfVariables : MonoBehaviour
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
        text.text = "Wolves\n";
        text.text += "Attacks Used    Q Values\n";
        foreach (KeyValuePair<string, int> pair in Environment.Instance.WolfAttacked)
        {
            string straightenStr = "";
            for(int i = pair.Key.Count(); i<5; i++)
            {
                straightenStr += " ";
            }
            text.text += pair.Key + ":   " + straightenStr + pair.Value + "             " + (Environment.Instance.WolfQValues[pair.Key] / Environment.Instance.WolfQValues.Max(x => x.Value)) + "\n";
        }
    }
}
