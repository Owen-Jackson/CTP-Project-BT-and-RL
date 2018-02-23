using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

[System.Serializable]
public class RLSelector : BTSelector {
    //private Dictionary<string, BTTask> taskLookup;   //Used to find tasks by their name
    [SerializeField]
    protected Dictionary<string, StateClass> states;

    public int StepsCompleted
    {
        get
        {
            return m_stepsCompleted;
        }
        set
        {
            m_stepsCompleted = value;
        }
    }
    [SerializeField]
    private int m_stepsCompleted = 0;

    public int StepsInEpisode
    {
        get
        {
            return m_stepsInEpisode;
        }
        set
        {
            m_stepsInEpisode = value;
        }
    }
    [SerializeField]
    private int m_stepsInEpisode = 200;

    public float GammaDiscountFactor
    {
        get
        {
            return m_gammaDiscountFactor;
        }
        set
        {
            m_gammaDiscountFactor = value;
        }
    }
    [SerializeField]
    private float m_gammaDiscountFactor = 0.8f;

    public List<StateClass> StatesList
    {
        get
        {
            return m_statesList;
        }
        set
        {
            m_statesList = value;
        }
    }
    [SerializeField]
    private List<StateClass> m_statesList;  //stores all states that the AI can be in

    public float LearningRate
    {
        get
        {
            return m_learningRate;
        }
        set
        {
            m_learningRate = value;
        }
    }
    [SerializeField]
    private float m_learningRate = 0.5f;


    [SerializeField]
    public float MaxEpsilon
    {
        get
        {
            return m_maxEpsilon;
        }
        set
        {
            m_maxEpsilon = value;
        }
    }
    [SerializeField]
    private float m_maxEpsilon = 1.0f; //used for random action selection

    public float MinEpsilon
    {
        get
        {
            return m_minEpsilon;
        }
        set
        {
            m_minEpsilon = value;
        }
    }
    [SerializeField]
    private float m_minEpsilon = 0.1f;  //min epsilon value

    public string CurrentState
    {
        get
        {
            return m_currentState;
        }
        set
        {
            m_currentState = value;
        }
    }
    [SerializeField]
    private string m_currentState;

    public string PreviousState
    {
        get
        {
            return m_previousState;
        }
        set
        {
            m_previousState = value;
        }
    }
    [SerializeField]
    private string m_previousState;

    public string CurrentActionName
    {
        get
        {
            return m_currentActionName;
        }
        set
        {
            m_currentActionName = value;
        }
    }
    [SerializeField]
    private string m_currentActionName;

    public int CurrentActionIndex
    {
        get
        {
            return m_currentActionIndex;
        }
        set
        {
            m_currentActionIndex = value;
        }
    }
    [SerializeField]
    private int m_currentActionIndex;

    public RLSelector(List<BTTask> tasks, int numOfSteps) : base(tasks)
    {
        m_stepsInEpisode = numOfSteps;
        //children = tasks;
    }

    //Add a new task to the selector
    public void AddTask(BTTask newTask)
    {
        if (!children.Contains(newTask))
        {
            children.Add(newTask);
            //check that this new action is not already in the current state's list of possible actions
            if(m_statesList.Find(x => x.GetScoresList().ContainsKey(CurrentState)) != null)
            {
                //add it to the state's dictionary
                m_statesList.Find(x => x.GetName() == CurrentState).AddAction(newTask.GetName());
            }
        }
    }

    //Remove one of the selector's current tasks
    public void RemoveTask(BTTask taskToRemove)
    {
        if (children.Contains(taskToRemove))
        {
            children.Remove(taskToRemove);
        }
    }

    protected string FindBestAction()
    {
        string bestAction = "";

        bestAction = states[CurrentState].GetScoresList().FirstOrDefault(x => x.Value == states[CurrentState].GetScoresList().Values.Max()).Key;    //gets the name of the highest valued action

        return bestAction;
    }

    //Each Tick select an action based on the current policy
    //public override StatusValue Tick()
    //{                
    //return StatusValue.SUCCESS;
    //}
}
