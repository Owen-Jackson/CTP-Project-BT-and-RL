using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BT_and_RL.Behaviour_Tree;

namespace BT_and_RL.QLearning
{
    //This class has all of the Q-Learning functionality. Add it as a component to a BT node to make it an RL node 
    [Serializable]
    public class QLearningBrain
    {
        protected Dictionary<string, StateClass> states;    //all of the states that the agent can be in

        //how many times this has been ticked so far this episode
        private int stepsCompleted = 0; 
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

        //how many times this algorithm is stepped through in a single episode 
        private int stepsInEpisode = 1000;
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

        //determines the importance of future rewards
        private float gammaDiscountFactor = 0.8f;
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

        //How fast this node learns
        private float learningRate = 0.5f;
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

        //probability of selecting a random action
        private float maxEpsilon = 1.0f;
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

        //min epsilon value
        private float minEpsilon = 0.1f;
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

        //which state the agent is currently in (used to index states dictionary)
        private string currentStateName = "";
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

        //the state that the agent was previously in (used to index states dictionary)
        private string previousStateName = "";
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

        //the name of the action that is being performed (used to index the actions dictionary for the current state)
        private string currentActionName = "";
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

        //the index of the action being performed
        private int currentActionIndex;
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

        public Dictionary<string, StateClass> GetStates()
        {
            return states;
        }

        //add a new state to the current set of states
        public void AddState(string newStateName, List<BTTask> tasks)
        {
            if (!states.ContainsKey(newStateName))
            {
                states.Add(newStateName, new StateClass(newStateName, tasks));
            }
        }

        //add a new action type to the current state
        public void AddAction(string actionName)
        {
            states[CurrentStateName].AddAction(actionName);
        }

        //getters and setters for the current and previous states
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
            //create an RNG using the thread safe random extension
            System.Random randomNumberGen = CustomExtensions.ThreadSafeRandom.ThisThreadsRandom;
            if ((float)randomNumberGen.NextDouble() < states[CurrentStateName].Epsilon)
            {
                //limits the number of attempts to prevent infinite loops
                int count = 0;
                do
                {
                    //randomly pick one of the currently available actions
                    CurrentActionIndex = randomNumberGen.Next(0, tasks.Count);
                    CurrentActionName = tasks[CurrentActionIndex].GetName();
                    //add this action if the current state does not know about it
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
                CurrentActionName = FindBestAction();
                CurrentActionIndex = tasks.IndexOf(tasks.Find(x => x.GetName() == CurrentActionName));
            }
            //decrease the epsilon value to reduce future probability
            if (states[CurrentStateName].Epsilon > MinEpsilon)
            {
                states[CurrentStateName].Epsilon -= (1f - MinEpsilon) / StepsInEpisode;
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

        //returns the score of the highest scoring action
        public float GetHighScore(string stateName)
        {
            return states[stateName].GetScore(FindBestAction());
        }

        //calculate the new Q value for the action using the Q-Learning formula
        private float GetNewQValue(float reward, string maxArg)
        {
            return (1 - LearningRate) * states[PreviousStateName].GetScore(CurrentActionName)
                + LearningRate * (reward + GammaDiscountFactor * states[CurrentStateName].GetScore(maxArg));
        }

        //set the new q-value in the state-action table
        private void UpdateQValue(float newQValue)
        {
            states[PreviousStateName].SetScore(CurrentActionName, newQValue);
        }

        //update the q-value for the current state-action pair
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
        private Dictionary<string, int> actionUseCounts;  //stores how many times each action has been performed

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

        //public bool DisplayedFinalResults { get; set; } = false;

        //constructors
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

        //increments the number of times this action has been used
        public void UpdateActionUseCount(string actionName)
        {
            if (actionUseCounts.ContainsKey(actionName))
            {
                actionUseCounts[actionName]++;
            }
        }

        //Adds a new action if the state does not already contain it
        public void AddAction(string actionName)
        {
            if (!scoreValues.ContainsKey(actionName))
            {
                scoreValues.Add(actionName, 0);
                actionUseCounts.Add(actionName, 0);
            }
        }

        //reject the action with the given name
        public void RejectAction(string actionName)
        {
            if (!rejectedActions.Contains(actionName))
            {
                rejectedActions.Add(actionName);
            }
        }

        //used when adding actions to avoid re-adding previously rejected actions
        //also when checking whether to permanently remove an action from the node
        public bool CheckIfRejected(string actionName)
        {
            if (rejectedActions.Contains(actionName))
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
