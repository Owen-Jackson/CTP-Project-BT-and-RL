using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

namespace BT_and_RL
{
    namespace QLearning
    {
        //This class stores a group of child actions that are used with Q-Learning to learn which one is the best for a given state
        [Serializable]
        public class RLSelector : BTSelector
        {
            [SerializeField]
            protected Dictionary<string, StateClass> states;

            [SerializeField]
            private int m_stepsCompleted = 0;
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
            private int m_stepsInEpisode = 200;
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
            private float m_gammaDiscountFactor = 0.8f;
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

            /*
            [SerializeField]
            protected List<StateClass> m_statesList;  //stores all states that the AI can be in
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
            */

            [SerializeField]
            private float m_learningRate = 0.5f;    //How fast this node learns
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
            private float m_maxEpsilon = 1.0f; //used for random action selection
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
            private float m_minEpsilon = 0.1f;  //min epsilon value
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
            private string m_currentState;
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
            private string m_previousState;
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
            private string m_currentActionName;
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
            private int m_currentActionIndex;
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

            public RLSelector(List<BTTask> tasks, int numOfSteps) : base(tasks)
            {
                m_stepsInEpisode = numOfSteps;
            }

            public RLSelector(List<BTTask> tasks, int numOfSteps, float minEpsilon) : base(tasks)
            {
                m_stepsInEpisode = numOfSteps;
                m_minEpsilon = minEpsilon;
            }

            public RLSelector(List<BTTask> tasks, int numOfSteps, float minEpsilon, float discountRate) : base(tasks)
            {
                m_stepsInEpisode = numOfSteps;
                m_minEpsilon = minEpsilon;
                m_gammaDiscountFactor = discountRate;
            }

            public RLSelector(List<BTTask> tasks, int numOfSteps, float minEpsilon, float discountRate, float learningRate) : base(tasks)
            {
                m_stepsInEpisode = numOfSteps;
                m_minEpsilon = minEpsilon;
                m_gammaDiscountFactor = discountRate;
                m_learningRate = learningRate;
            }

            //Add a new task to the selector
            public override void AddTask(string newTask)
            {
                if (ActionPool.Instance.actionPool.ContainsKey(newTask))
                {
                    //BTTask check = (BTTask)ActionPool.Instance.GetAction(newTask);
                    //check that the task it is trying to add is suitable
                    //if (check.IsTaskCompatible(this.compatibility))
                    //{
                    if (children.Find(t => t.GetType().Name == newTask) == null)
                    {
                        //Debug.Log("GOT IT ADDING NOW: " + ActionPool.Instance.actionPool[newTask].Name);
                        object toAdd = Activator.CreateInstance(ActionPool.Instance.actionPool[newTask]);
                        children.Add((BTTask)toAdd);

                        //check that this new action is not already in the current state's list of possible actions
                        if (!states[CurrentState].GetScoresList().ContainsKey(toAdd.GetType().Name))
                        {
                            states[CurrentState].AddAction(toAdd.GetType().Name);
                        }
                    }
                    else
                    {
                        //Debug.Log("children already has this task");
                    }
                }
                //}
                else
                {
                    //Debug.Log("action pool doesn't have this name");
                }
            }

            //if all states reject this task then there is no need for it here anymore
            //returns true if the task is no longer needed
            public bool IsTaskIsUseless(BTTask taskToCheck)
            {
                int rejectCount = 0;
                foreach(StateClass state in states.Values)
                {
                    if(state.CheckIfRejected(taskToCheck.GetType()))
                    {
                        rejectCount++;
                    }
                }
                if(rejectCount == states.Count)
                {
                    return true;
                }
                return false;
            }

            //checks if the task has been rejected from the current state
            private bool CheckIfRejected(BTTask task)
            {
                if (states[CurrentState].CheckIfRejected(task.GetType()))
                {
                    return true;
                }
                return false;
            }

            //Remove one of the selector's current tasks
            public void RemoveTask(BTTask taskToRemove)
            {
                if (children.Contains(taskToRemove))
                {
                    children.Remove(taskToRemove);
                }
            }

            public void CheckIfUsingRandomAction()
            {
                //create an rng using the thread safe random extension
                System.Random random = CustomExtensions.ThreadSafeRandom.ThisThreadsRandom;
                if ((float)random.NextDouble() < states[CurrentState].Epsilon)
                {
                    int count = 0;
                    do
                    {
                        CurrentActionIndex = random.Next(0, children.Count);
                        CurrentActionName = children[CurrentActionIndex].GetType().Name;    //random
                        count++;
                    } while (CheckIfRejected(children[m_currentActionIndex]));
                }
                else
                {
                    //Find the best action from the Q value table
                    CurrentActionName = states[CurrentState].GetScoresList().Where(x => x.Value == states[CurrentState].GetScoresList().Max(y => y.Value)).Select(z => z.Key).First(); //max value
                    CurrentActionIndex = children.IndexOf(children.Find(x => x.GetType().Name == CurrentActionName));
                    //Debug.Log("current best action: " + CurrentActionName);
                }
                //decrease the epsilon value to reduce future probability
                if (states[CurrentState].Epsilon > MinEpsilon)
                {
                    states[CurrentState].Epsilon -= (1f - MinEpsilon) / StepsInEpisode;
                    //Debug.Log("current epsilon: " + Epsilon);
                }
            }

            //Returns the name of the highest valued action
            protected string FindBestAction()
            {
                string bestAction = "";

                bestAction = states[CurrentState].GetScoresList().FirstOrDefault(x => x.Value == states[CurrentState].GetScoresList().Values.Max()).Key;    //gets the name of the highest valued action

                return bestAction;
            }

            //Uses the Q-Learning formula to caluclate the new q-value and returns it
            protected float GetNewQValue(float reward, string maxArg)
            {
                return (1 - LearningRate) * states[PreviousState].GetScoresList()[CurrentActionName] + LearningRate * (reward + GammaDiscountFactor * states[CurrentState].GetScoresList()[maxArg]);
            }
        }

        //Used to store the variables for the Q-Learning states
        [Serializable]
        public class StateClass
        {
            [SerializeField]
            private string m_stateName;

            [SerializeField]
            private Dictionary<string, float> m_scoreValues;    //relates an action (by its name) to a q value

            private HashSet<Type> rejectedActions;  //stores any actions that are so bad that they should be avoided

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
            private int m_episodesToComplete = 500; //Each state can learn at an individual rate, meaning the AI can remember how much it has learned with a skill already
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

            public bool DisplayedFinalResults { get; set; } = false;

            public StateClass(string name, List<BTTask> tasks, float startingEpsilon)
            {
                m_stateName = name;
                //initialise a dictionary to store the q value for each action in this state
                m_scoreValues = new Dictionary<string, float>();
                foreach (BTTask task in tasks)
                {
                    m_scoreValues.Add(task.GetType().Name, 0);
                }
                Epsilon = startingEpsilon;
                rejectedActions = new HashSet<Type>();
            }

            public Dictionary<string, float> GetScoresList()
            {
                return m_scoreValues;
            }

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
                if (!m_scoreValues.ContainsKey(index))
                {
                    AddAction(index);
                }
                m_scoreValues[index] = score;
            }

            public void AddAction(string actionName) //Adds a new action if the state does not already contain it
            {
                if (!m_scoreValues.ContainsKey(actionName))
                {
                    m_scoreValues.Add(actionName, 0);
                }
            }

            public void RejectAction(Type actionType)
            {
                if (!rejectedActions.Contains(actionType))
                {
                    rejectedActions.Add(actionType);
                }
            }

            public bool CheckIfRejected(Type actionType)
            {
                if(rejectedActions.Contains(actionType))
                {
                    return true;
                }
                return false;
            }

            public HashSet<Type> GetRejectedActions()
            {
                return rejectedActions;
            }
        }

        public class ActionPool
        {
            private static ActionPool instance;
            public static ActionPool Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new ActionPool();
                    }
                    return instance;
                }
            }

            public Dictionary<string, Type> actionPool;
            public bool isAvailable = false;  //going to be used if the constructor takes too long and creates issues

            public ActionPool()
            {
                actionPool = new Dictionary<string, Type>();
                System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    Type[] actions = assemblies[i].GetTypes().Where(t => t.IsSubclassOf(typeof(BTAction))).ToArray();
                    for (int j = 0; j < actions.Length; j++)
                    {
                        //Debug.Log("adding " + actions[j].Name + " to the action pool");
                        if (!actionPool.ContainsKey(actions[j].Name))
                        {
                            actionPool.Add(actions[j].Name, actions[j]);
                        }
                    }
                }
                isAvailable = true;
            }

            public object GetAction(string name)
            {
                if(actionPool.ContainsKey(name))
                {
                    return actionPool[name];
                }
                return "action not found";
            }

            public Type GetRandomAction()
            {
                int index = CustomExtensions.ThreadSafeRandom.ThisThreadsRandom.Next(0, actionPool.Count);
                //Debug.Log("getting element number " + index);
                Type get = actionPool.Values.ElementAt(CustomExtensions.ThreadSafeRandom.ThisThreadsRandom.Next(0, actionPool.Count));
                //Debug.Log("received object " + get.Name);
                return get;
            }
        }
    }
}
