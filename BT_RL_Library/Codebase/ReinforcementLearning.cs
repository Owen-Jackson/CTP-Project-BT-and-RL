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

            [SerializeField]
            private List<StateClass> m_statesList;  //stores all states that the AI can be in
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
            public void AddTask(BTTask newTask)
            {
                if (!children.Contains(newTask))
                {
                    children.Add(newTask);
                    //check that this new action is not already in the current state's list of possible actions
                    if (m_statesList.Find(x => x.GetScoresList().ContainsKey(CurrentState)) != null)
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

            public void CheckIfUsingRandomAction()
            {
                //create an rng using the thread safe random extension
                System.Random random = CustomExtensions.ThreadSafeRandom.ThisThreadsRandom;
                if ((float)random.NextDouble() < states[CurrentState].Epsilon)
                {
                    CurrentActionIndex = random.Next(0, children.Count);
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

            /*public override StatusValue Tick()
            {
                if(children.Count == 0)
                {

                }
                    
                return StatusValue.FAILED;
            }
            */
        }

        //Used to store the variables for the Q-Learning states
        [Serializable]
        public class StateClass
        {
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
                    m_scoreValues.Add(task.GetName(), 0);
                }
                Epsilon = startingEpsilon;
            }

            public Dictionary<string, float> GetScoresList()
            {
                return m_scoreValues;
            }

            /*
            public void UpdateValueDisplays(string damageType)
            {
                Environment.Instance.SetQValue(m_stateName, damageType, m_scoreValues[damageType]);
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
        }

        public class ActionPool : CustomExtensions.Singleton
        {
            public Dictionary<string, Type> actionPool;

            public ActionPool()
            {

                actionPool = new Dictionary<string, Type>();
                System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    Type[] actions = assemblies[i].GetTypes().Where(t => t.IsSubclassOf(typeof(BTAction))).ToArray();
                    for (int j = 0; j < actions.Length; j++)
                    {
                        Debug.Log("adding " + actions[i].Name + "to the action pool");
                        if (!actionPool.ContainsValue(actions[j]))
                        {
                            actionPool.Add(actions[j].Name, actions[j].GetType());
                        }
                    }
                }
            }
                //System.Reflection.Assembly.GetAssembly().GetTypes().Where(t => t.IsSubclassOf())
        }
    }
}
