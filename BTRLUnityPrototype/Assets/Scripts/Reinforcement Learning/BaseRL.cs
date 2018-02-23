using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseRL {
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

    //[SerializeField]
    //private float[][] valueTable;   //these are the reward estimates for the AI

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

    //[SerializeField]
    //private Dictionary<string, List<float>> valueDictionary;    //stores all states as strings that are linked to a list of values

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
    private float m_epsilon = 1.0f; //used for random action selection

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

    public int CurrentState
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
    private int m_currentState = 0;

    public int PreviousState
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
    private int m_previousState = 0;

    public int CurrentAction
    {
        get
        {
            return m_currentAction;
        }
        set
        {
            m_currentAction = value;
        }
    }
    [SerializeField]
    private int m_currentAction = -1;
    

    //eGreedyValue determines whether to explore or exploit
    //decrease the value over time as the agent becomes more confident in its estimated Q-Values    

    /*
    public BaseRL(int stateSize, int actionSize)
    {


        
         *valueTable = new float[stateSize][];
        for (int i = 0; i < stateSize; i++)
        {
            valueTable[i] = new float[actionSize];
            for (int j = 0; j < actionSize; j++)
            {
                valueTable[i][j] = 0.0f;
            }
        }
        
    }
    */


    public BaseRL(List<string> stateNames, int actionSize)
    {
        //initialise the list of states
        StatesList = new List<StateClass>();
        for (int i = 0; i < stateNames.Count; i++)
        {
            //StatesList.Add(new StateClass(i, stateNames[i], actionSize));
        }
    }


    public void Setup(List<string> stateNames, int actionSize)
    {
        //initialise the list of states
        StatesList = new List<StateClass>();
        for (int i = 0; i < stateNames.Count; i++)
        {
           //StatesList.Add(new StateClass(i, stateNames[i], actionSize));
        }
    }

    //Q-Value of action a in state s = (1 - learningRate) * Q(s, a) + learningRate * observed Q(s,a)
    //observed Q(s, a) = reward(s, a) + discountFactor * max Q(s + 1, a)
    //max Q(s + 1, a) = estimate of optimal future value
    void UpdateValues(float reward)
    {
        int maxArg = FindBestAction(CurrentState);
        //valueTable[previousState][currentAction] = (1 - learningRate) * valueTable[previousState][currentAction] + learningRate * (reward + gammaDiscountFactor * valueTable[currentState][maxArg]);       
        //StatesList[PreviousState].SetScore(CurrentAction, (1 - LearningRate) * StatesList[PreviousState].GetScore(CurrentAction) 
            //+ LearningRate * (reward + GammaDiscountFactor * StatesList[CurrentState].GetScore(maxArg)));
    }

    //used by the environment to get the reward for the current step
    public void SendReward(float reward)
    {
        UpdateValues(reward);
    }

    int FindBestAction(int state)
    {
        int bestAction = 0;
        float bestVal = 0;
        if (state >= 0)
        {
            for(int i = 0; i < StatesList[state].GetListLength(); i++)
            {
                //if(StatesList[state].GetScore(i) > bestVal)
                {
                    bestAction = i;
                    //bestVal = StatesList[state].GetScore(i);
                }
            }
            /*
            for (int i = 0; i < valueTable[state].Length; i++)
            {
                if (valueTable[state][i] > bestVal)
                {
                    bestAction = i;
                    bestVal = valueTable[state][i];
                }
            }*/
        }
        return bestAction;
    }
}
