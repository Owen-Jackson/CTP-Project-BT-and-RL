using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using BT_and_RL.Behaviour_Tree;

namespace BT_and_RL.QLearning
{
    [Serializable]
    public class QLearningBrain
    {
        protected Dictionary<string, StateClass> states;    //all of the states that the agent can be in

        private int stepsCompleted = 0;   //how many times this has been ticked so far this episode
        public int StepsCompleted
        {
            get
            {
                return stepsCompleted;
            }
            set
            {
                stepsCompleted = value;
            }
        }

        private int stepsInEpisode = 1000;    //how many times this algorithm is stepped through in a single episode 
        public int StepsInEpisode
        {
            get
            {
                return stepsInEpisode;
            }
            set
            {
                stepsInEpisode = value;
            }
        }

        private float gammaDiscountFactor = 0.8f; //determines the importance of future rewards
        public float GammaDiscountFactor
        {
            get
            {
                return gammaDiscountFactor;
            }
            set
            {
                gammaDiscountFactor = value;
            }
        }

        private float learningRate = 0.5f;    //How fast this node learns
        public float LearningRate
        {
            get
            {
                return learningRate;
            }
            set
            {
                learningRate = value;
            }
        }

        private float maxEpsilon = 1.0f; //probability of selecting a random action
        public float MaxEpsilon
        {
            get
            {
                return maxEpsilon;
            }
            set
            {
                maxEpsilon = value;
            }
        }

        private float minEpsilon = 0.1f;  //min epsilon value
        public float MinEpsilon
        {
            get
            {
                return minEpsilon;
            }
            set
            {
                minEpsilon = value;
            }
        }

        private string currentStateName = "";  //which state the agent is currently in (used to index states dictionary)
        public string CurrentStateName
        {
            get
            {
                return currentStateName;
            }
            set
            {
                currentStateName = value;
            }
        }

        private string previousStateName = ""; //the state that the agent was previously in (used to index states dictionary)
        public string PreviousStateName
        {
            get
            {
                return previousStateName;
            }
            set
            {
                previousStateName = value;
            }
        }

        private string currentActionName = ""; //the name of the action that is being performed (used to index the actions dictionary for the current state)
        public string CurrentActionName
        {
            get
            {
                return currentActionName;
            }
            set
            {
                currentActionName = value;
            }
        }

        private int currentActionIndex;   //the index of the action being performed
        public int CurrentActionIndex
        {
            get
            {
                return currentActionIndex;
            }
            set
            {
                currentActionIndex = value;
            }
        }

        public QLearningBrain()
        {
            states = new Dictionary<string, StateClass>();
        }

        public QLearningBrain(List<BTTask> tasks)
        {
            states = new Dictionary<string, StateClass>();

            for (int i = 0; i < tasks.Count; i++)
            {
                string taskName = tasks[i].GetName();
                states.Add(taskName, new StateClass(taskName));
            }
        }

        public Dictionary<string, StateClass> GetStates()
        {
            return states;
        }

        public void AddState(string newStateName, List<BTTask> tasks)
        {
            if (!states.ContainsKey(newStateName))
            {
                states.Add(newStateName, new StateClass(newStateName, tasks));
            }
        }

        public void AddAction(string actionName)
        {
            states[CurrentStateName].AddAction(actionName);
        }

        public StateClass GetCurrentState()
        {
            if (states.ContainsKey(CurrentStateName))
            {
                if (states[CurrentStateName] != null)
                {
                    return states[CurrentStateName];
                }
            }
            return null;
        }

        public StateClass GetPreviousState()
        {
            if (states.ContainsKey(PreviousStateName))
            {
                if (states[PreviousStateName] != null)
                {
                    return states[PreviousStateName];
                }
            }
            return null;
        }

        public void SetCurrentStateName(string stateName)
        {
            if (!states.ContainsKey(stateName))
            {
                states.Add(stateName, new StateClass(stateName));
            }
            CurrentStateName = stateName;
        }

        public void SetPreviousStateName(string stateName)
        {
            if (!states.ContainsKey(stateName))
            {
                states.Add(stateName, new StateClass(stateName));
            }
            PreviousStateName = stateName;
        }

        //selects an action, either randomly or by choosing the highest scoring
        //returns the index of the action to perform
        public int SelectAnAction(List<BTTask> tasks)
        {
            //create an rng using the thread safe random extension
            System.Random randomNumberGen = CustomExtensions.ThreadSafeRandom.ThisThreadsRandom;
            if ((float)randomNumberGen.NextDouble() < states[CurrentStateName].Epsilon)
            {
                //limits the number of attempts to break out of infinite loop
                int count = 0;
                do
                {
                    CurrentActionIndex = randomNumberGen.Next(0, tasks.Count);
                    CurrentActionName = tasks[CurrentActionIndex].GetName();    //random
                    //check that the current state does not have this action
                    if(!states[CurrentStateName].GetScoresList().ContainsKey(CurrentActionName))
                    {
                        states[CurrentStateName].AddAction(CurrentActionName);
                    }
                    count++;
                } while (CheckIfRejected(tasks[CurrentActionIndex]) && count < 30);
            }
            else
            {
                //Find the best action from the Q value table
                CurrentActionName = FindBestAction();   //states[CurrentStateName].GetScoresList().Where(x => x.Value == states[CurrentStateName].GetScoresList().Max(y => y.Value)).Select(z => z.Key).First(); //max value
                CurrentActionIndex = tasks.IndexOf(tasks.Find(x => x.GetName() == CurrentActionName));
                //Debug.Log("current best action: " + CurrentActionName);
            }
            //decrease the epsilon value to reduce future probability
            if (states[CurrentStateName].Epsilon > MinEpsilon)
            {
                states[CurrentStateName].Epsilon -= (1f - MinEpsilon) / StepsInEpisode;
                //Debug.Log("current epsilon: " + states[CurrentStateName].Epsilon);
            }
            return CurrentActionIndex;
        }

        //Returns the name of the highest valued action
        private string FindBestAction()
        {
            string bestAction = "null";
            //get the name of the highest valued action
            bestAction = states[PreviousStateName].GetScoresList().FirstOrDefault(x => x.Value == states[PreviousStateName].GetScoresList().Values.Max()).Key;
            return bestAction;
        }

        //calculate the new Q value for the action using the Q-Learning formula
        private float GetNewQValue(float reward, string maxArg)
        {
            return (1 - LearningRate) * states[PreviousStateName].GetScore(CurrentActionName)
                + LearningRate * (reward + GammaDiscountFactor * states[CurrentStateName].GetScore(maxArg));
        }

        private void UpdateQValue(float newQValue)
        {
            states[PreviousStateName].SetScore(CurrentActionName, newQValue);
            //states[PreviousStateName].Epsilon -= 1.0f / states[PreviousStateName].EpisodesToComplete;
        }

        public void UpdateQValueTable(float reward)
        {
            string bestActionName = FindBestAction();
            float newQ = GetNewQValue(reward, bestActionName);
            UpdateQValue(newQ);
        }

        //checks if the task has been rejected from the current state
        private bool CheckIfRejected(BTTask task)
        {
            if (states[CurrentStateName].CheckIfRejected(task.GetName()))
            {
                return true;
            }
            return false;
        }
    }

    //Used to store the variables for the Q-Learning states
    [Serializable]
    public class StateClass
    {
        private string stateName;

        private Dictionary<string, float> scoreValues;    //relates an action (by its name) to a q value
        private Dictionary<string, int> actionUseCounts; //stores how many times each action has been performed

        private HashSet<string> rejectedActions;  //stores any actions that are so bad that they should be avoided

        private float epsilon = 1.0f; //The policy value for this state
        public float Epsilon
        {
            get
            {
                return epsilon;
            }
            set
            {
                epsilon = value;
            }
        }

        private int episodesToComplete = 1000; //Each state can learn at an individual rate, meaning the AI can remember how much it has learned with a skill already
        public int EpisodesToComplete
        {
            get
            {
                return episodesToComplete;
            }
            set
            {
                episodesToComplete = value;
            }
        }

        public bool DisplayedFinalResults { get; set; } = false;

        public StateClass(string name)
        {
            stateName = name;
            InitialiseDataSets();
        }

        public StateClass(string name, List<BTTask> tasks)
        {
            stateName = name;
            InitialiseDataSets();
            foreach (BTTask task in tasks)
            {
                scoreValues.Add(task.GetName(), 0);
                actionUseCounts.Add(task.GetName(), 0);
            }
        }

        //makes sure the data sets for this class are created
        private void InitialiseDataSets()
        {
            //dictionary to store the q value for each action in this state
            scoreValues = new Dictionary<string, float>();
            //stores how many times an action has been used
            actionUseCounts = new Dictionary<string, int>();
            //stores the actions that the agent has deemed useless in this state
            rejectedActions = new HashSet<string>();
        }

        public Dictionary<string, float> GetScoresList()
        {
            return scoreValues;
        }

        public Dictionary<string, int> GetActionUseList()
        {
            return actionUseCounts;
        }

        public string GetName()
        {
            return stateName;
        }

        public float GetScore(string index)
        {
            if (scoreValues.ContainsKey(index))
            {
                return scoreValues[index];
            }
            return 0;
        }

        public int GetActionUseCount(string actionName)
        {
            if (actionUseCounts.ContainsKey(actionName))
            {
                return actionUseCounts[actionName];
            }
            return 0;
        }

        public int GetListLength()
        {
            return scoreValues.Count;
        }

        public void SetScore(string actionName, float score)
        {
            if (!scoreValues.ContainsKey(actionName))
            {
                AddAction(actionName);
            }
            scoreValues[actionName] = score;
        }

        public void UpdateActionUseCount(string actionName)
        {
            if (actionUseCounts.ContainsKey(actionName))
            {
                actionUseCounts[actionName]++;
            }
        }

        public void AddAction(string actionName) //Adds a new action if the state does not already contain it
        {
            if (!scoreValues.ContainsKey(actionName))
            {
                scoreValues.Add(actionName, 0);
                actionUseCounts.Add(actionName, 0);
            }
        }

        public void RejectAction(string actionNanme)
        {
            if (!rejectedActions.Contains(actionNanme))
            {
                rejectedActions.Add(actionNanme);
            }
        }

        public bool CheckIfRejected(string actionNanme)
        {
            if (rejectedActions.Contains(actionNanme))
            {
                return true;
            }
            return false;
        }

        public HashSet<string> GetRejectedActions()
        {
            return rejectedActions;
        }
    }
}
