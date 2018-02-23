using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

//This selector uses Reinforcement Learning to reconstruct itself based on what type of enemy the current target is
[System.Serializable]
public class RLAttack : RLSelector {
    private GameObject owner;
    private AIPerceptor senses;

    public RLAttack(List<BTTask> tasks, int numOfSteps, GameObject _owner, AIPerceptor _senses) : base(tasks, numOfSteps)
    {
        taskName = "RLAttack";
        owner = _owner;
        senses = _senses;
        states = new Dictionary<string, StateClass>();        
    }    

    public override StatusValue Tick()
    {
        if (senses.GetCurrentTarget() == null)
        {
            if(!senses.SelectEnemy())
            {
                return StatusValue.FAILED;
            }
        }

        //Get which enemy type we are attacking
        CurrentState = senses.GetCurrentTarget().GetName();
        if (!states.ContainsKey(CurrentState))
        {
            states.Add(CurrentState, new StateClass(CurrentState, children, MaxEpsilon));
        }

        //Select the action to perform
        //Check whether we are using a random action or the max value one
        if (Random.Range(0f, 1f) < states[CurrentState].Epsilon)
        {
            CurrentActionIndex = Random.Range(0, children.Count);
            CurrentActionName = children[CurrentActionIndex].GetName();    //random

        }
        else
        {
            //Find the best action from the Q value table
            CurrentActionName = states[CurrentState].GetScoresList().Where(x => x.Value == states[CurrentState].GetScoresList().Max(y => y.Value)).Select(z => z.Key).First(); //max value
            CurrentActionIndex = children.IndexOf(children.Find(x => x.GetName() == CurrentActionName));
            //Debug.Log("current best action: " + CurrentActionName);
        }
        //decrease the epsilon value to reduce future probability
        if (states[CurrentState].Epsilon > MinEpsilon)
        {
            states[CurrentState].Epsilon -= (1f-MinEpsilon) / StepsInEpisode;
            //Debug.Log("current epsilon: " + Epsilon);
        }
        //Debug.Log("This time i used: " + CurrentActionName);

        //Perform the action (Tick it)
        Debug.Log("attacking enemy");
        status = children[CurrentActionIndex].Tick();
        if(states[CurrentState].EpisodeCount >= 0)
        {
            Environment.Instance.AddToDamageDictionary(senses.GetCurrentTarget().GetName(), (children[CurrentActionIndex] as AttackClass).GetElementType());
        }
        PreviousState = CurrentState;
        if (!senses.GetCurrentTarget().IsAlive())
        {
            senses.RemoveTargetFromList();
        }

        //Get the reward
        float reward = (children[CurrentActionIndex] as AttackClass).GetDamageDealt();
        //Debug.Log(CurrentActionName + " reward received: " + reward);

        //Update the q value table with the reward value
        //Q-Value of action a in state s = (1 - learningRate) * Q(s, a) + learningRate * observed Q(s,a)
        //observed Q(s, a) = reward(s, a) + discountFactor * max Q(s + 1, a)
        //max Q(s + 1, a) = estimate of optimal future value
        string maxArg = FindBestAction();
        //Debug.Log("best action to take is: " + maxArg);
        float newQ = (1 - LearningRate) * states[PreviousState].GetScoresList()[CurrentActionName] + LearningRate * (reward + GammaDiscountFactor * states[CurrentState].GetScoresList()[maxArg]);
        states[PreviousState].GetScoresList()[CurrentActionName] = newQ;
        if(states[PreviousState].EpisodeCount >= 0)
        {
            Environment.Instance.SetQValue(states[PreviousState].GetName(), (children[CurrentActionIndex] as AttackClass).GetElementType(), newQ);
        }

        //Debug.Log("Updated scores from state: " + PreviousState);
        states[PreviousState].EpisodeCount--;
        if (states[PreviousState].EpisodeCount <= 0 && !states[PreviousState].displayedFinalResults)
        {
            Debug.Log("Completed learning for how to fight a " + PreviousState + "results by attack type: ");
            float maxQ = states[PreviousState].GetScoresList().Max(x => x.Value);
            Debug.Log("max value is: " + maxQ);
            foreach (KeyValuePair<string, float> innerPair in states[PreviousState].GetScoresList())
            {
                Debug.Log(innerPair.Key + " : " + innerPair.Value/maxQ);
            }
            states[PreviousState].displayedFinalResults = true;
            /*
            foreach (KeyValuePair<string, StateClass> outerPair in states)
            {
                Debug.Log("My values for state " + outerPair.Key + " are:");
                foreach (KeyValuePair<string, float> innerPair in states[outerPair.Key].GetScoresList())
                {
                    Debug.Log(innerPair.Key + " : " + innerPair.Value);
                }
            }
            */
        }
        return StatusValue.SUCCESS;
    }
}
