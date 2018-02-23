using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

[System.Serializable]
public class StateClass {
    //[SerializeField]
    //private int m_stateID;
    [SerializeField]
    private string m_stateName;
    [SerializeField]
    private Dictionary<string, float> m_scoreValues;    //relates an action (by its name) to a q value
    [SerializeField]
    private float m_epsilon; //The policy value for this state
    public float Epsilon
    {
        get
        {
            return m_epsilon;
        }
        set
        {
            m_epsilon = value;
        }
    }

    [SerializeField]
    private int m_episodesToComplete = 500;
    public int EpisodeCount
    {
        get
        {
            return m_episodesToComplete;
        }
        set
        {
            m_episodesToComplete = value;
        }
    }

    public bool displayedFinalResults = false;

    public StateClass(string name, List<BTTask> tasks, float startingEpsilon)
    {
        m_stateName = name;
        //initialise a dictionary to store the q value for each action in this state
        m_scoreValues = new Dictionary<string, float>();
        foreach (BTTask task in tasks)
        {
            m_scoreValues.Add(task.GetName(), 0);
        }
        Epsilon = startingEpsilon;
    }

    public Dictionary<string, float> GetScoresList()
    {
        return m_scoreValues;
    }

    public void UpdateValueDisplays(string damageType)
    {
        Environment.Instance.SetQValue(m_stateName, damageType, m_scoreValues[damageType]);
    }

    /*
    public int GetID()
    {
        return m_stateID;
    }

    public void SetID(int newID)
    {
        m_stateID = newID;
    }
    */

    public string GetName()
    {
        return m_stateName;
    }

    public float GetScore(string index)
    {
        return m_scoreValues[index];
    }

    public int GetListLength()
    {
        return m_scoreValues.Count;
    }

    public void SetScore(string index, float score)
    {
        if(!m_scoreValues.ContainsKey(index))
        {
            AddAction(index);
        }
        m_scoreValues[index] = score;
    }

    public void AddAction(string actionName) //adapt later for BT actions
    {
        if (!m_scoreValues.ContainsKey(actionName))
        {
            m_scoreValues.Add(actionName, 0);
        }
    }
}
