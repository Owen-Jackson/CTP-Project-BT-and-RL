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
            protected QLearningBrain learner;
            protected int indexOfSelectedAction = 0;
            protected bool isNodeDynamic = false;   //set this to true to allow this node to automatically add and remove children

            //minimum number of times an action must be done before checking if it should be rejected
            //should avoid prematurely rejecting ok actions
            protected int minimumActionPerformances = 30;

            [SerializeField]
            protected bool isInitialised = false; //used when checking if this node's values have been initialised

            //default and parameterised constructors
            public RLSelector()
            {
                learner = new QLearningBrain();
            }
            public RLSelector(List<BTTask> tasks) : base(tasks)
            {
                learner = new QLearningBrain(tasks);
                for(int i = 0; i < tasks.Count; i++)
                {
                    children[i].SetTreeDepth(treeDepth + 1);
                }
            }

            //This is the general tick function, variations are made by overriding the FirstTimeInit, GetState and GetReward functions
            public override StatusValue Tick(Blackboard blackboard)
            {
                if (!isInitialised)
                {
                    //initialise any custom variables
                    FirstTimeInit(blackboard);

                    //set the initial state
                    learner.SetPreviousStateName(GetState(blackboard));

                    isInitialised = true;
                }

                //only do this part when the current action has finished
                if (status != StatusValue.RUNNING)
                {
                    //get the current state for the learner
                    learner.SetCurrentStateName(GetState(blackboard));

                    //random probability to add an action
                    if(isNodeDynamic)
                    {
                        System.Random random = CustomExtensions.ThreadSafeRandom.ThisThreadsRandom;
                        if (children.Count == 0 || random.NextDouble() < 0.25f)
                        {
                            //retrieve a random task from the action pool
                            BTTask newTask = (BTTask)ActionPool.Instance.GetRandomAction();
                            //add the task if it is not rejected by the current state
                            if (!learner.GetPreviousState().CheckIfRejected(newTask.GetName()))
                            {
                                //Debug.Log("learner has not rejected: " + newTask.GetName());
                                AddTask(newTask);
                            }
                        }
                    }

                    //get the index of the action that the RL decides to use
                    indexOfSelectedAction = learner.SelectAnAction(children);
                }

                //update this node's status by ticking the selected action
                status = children[indexOfSelectedAction].Tick(blackboard);

                //if the action is still running, return running until is has finished
                if(status == StatusValue.RUNNING)
                {
                    return status;
                }

                //update the states
                learner.SetPreviousStateName(learner.CurrentStateName);
                learner.SetCurrentStateName(GetState(blackboard));

                //get the reward for the previous state, i.e. the one this was in when starting the action
                float reward = GetReward(learner.GetStates()[learner.PreviousStateName], children[indexOfSelectedAction]);

                //update the Q value table
                learner.UpdateQValueTable(reward);

                //decrement the number of steps left to perform (might not be used)

                //update the use count for this action
                learner.GetPreviousState().UpdateActionUseCount(learner.CurrentActionName);

                //check if the action should be rejected
                //action should be performed a minimum number of times before checking this
                if (learner.GetPreviousState().GetActionUseCount(learner.CurrentActionName) > minimumActionPerformances)
                {
                    //check if rejected
                    if(ShouldActionBeRejected(blackboard))
                    {
                        //reject from this state
                        learner.GetPreviousState().RejectAction(children[indexOfSelectedAction].GetName());
                        //if all states reject the action then remove it from this node
                        if (ShouldActionBeRemoved(learner.CurrentActionName))
                        {
                            children.RemoveAt(learner.CurrentActionIndex);
                        }
                    }
                }

                QLearningValues(blackboard);

                return StatusValue.SUCCESS;
            }

            //override this to setup any values that might be needed in future iterations
            public virtual void FirstTimeInit(Blackboard blackboard)
            {
                //initialise values here
            }

            //function to be overridden by inherited classes. It uses user-defined conditions to get a state
            public virtual string GetState(Blackboard blackboard)
            {
                string stateName = "placeholder";
                return stateName;
            }

            //override this to get the reward for the performing this state-action pair
            public virtual float GetReward(StateClass state, BTTask action)
            {
                //make reward = the reward here
                return 0.0f;
            }

            //adds an action as a child to this node
            public void AddTask(BTTask newAction)
            {
                for(int i = 0; i < children.Count; i++)
                {
                    if(children[i].GetName() == newAction.GetName())
                    {
                        return;
                    }
                }
                children.Add(newAction);
                newAction.SetTreeDepth(treeDepth + 1);
            }

            //removes a task from this node's children
            public void RemoveTask(string taskName)
            {
                for(int i = 0; i < children.Count; i++)
                {
                    if(children[i].GetName() == taskName)
                    {
                        children.RemoveAt(i);
                        return;
                    }
                }
            }

            //override this with a metric for rejecting a task, e.g. score thresholds
            public virtual bool ShouldActionBeRejected(Blackboard blackboard)
            {
                //check whether this action should be rejected
                return false;
            }

            //if the action just used has been rejected in all states then we can remove it from the tree.
            protected bool ShouldActionBeRemoved(string actionToCheck)
            {
                int rejectCount = 0;
                foreach(StateClass state in learner.GetStates().Values)
                {
                    if(state.CheckIfRejected(actionToCheck))
                    {
                        rejectCount++;
                    }
                }
                
                if(rejectCount == learner.GetStates().Count)
                {
                    return true;
                }
                return false;
            }

            //get the learning component of this node
            public QLearningBrain GetLearner()
            {
                return learner;
            }

            //displays the status of the tree's children
            public override void DisplayValues(ref string fullOutputString)
            {
                fullOutputString += "\n";
                fullOutputString += new string('\t', treeDepth);
                fullOutputString += GetName();

                for (int i = 0; i < children.Count; i++)
                {
                    children[i].DisplayValues(ref fullOutputString);
                    if(i == indexOfSelectedAction)
                    {
                        fullOutputString += " <--";
                    }
                }
            }

            //used for displaying the q values that the agent has learrned
            public void QLearningValues(Blackboard blackboard)
            {
                string fullOutputString = (string)blackboard.GetValue("QValueDebugString");
                fullOutputString += learner.PreviousStateName + "\n";
                for (int i = 0; i < children.Count; i++)
                {
                    fullOutputString += children[i].GetName() + "\t";
                    if (learner.GetPreviousState() != null)
                    {
                        fullOutputString += "\tq-value: " + learner.GetPreviousState().GetScore(children[i].GetName()) + "\tnumber of uses: " + learner.GetPreviousState().GetActionUseCount(children[i].GetName());
                    }
                    fullOutputString += "\n";
                }
                //Debug.Log("full q value string: " + fullOutputString);
                blackboard.SetValue("QValueDebugString", fullOutputString);
            }

            public override void SetTreeDepth(int depth)
            {
                base.SetTreeDepth(depth);
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].SetTreeDepth(depth + 1);
                }
            }
        }
    }
}

//OLD CODE
/*
                //see if we are adding a new task
                if (children.Count == 0 || CustomExtensions.ThreadSafeRandom.ThisThreadsRandom.NextDouble() < states[CurrentState].Epsilon)
                {
                    Type addType = ActionPool.Instance.GetRandomAction();
                    //if this node doesn't already have this task
                    if (children.Find(t => t.GetType().Name == addType.Name) == null)
                    {
                        //if this state has not rejected this task
                        if (!states[CurrentState].CheckIfRejected(addType))
                        {
                            //add it
                            AddTask(addType.Name);

                            //if this state has not seen this task before
                            if (!states[CurrentState].GetScoresList().ContainsKey(addType.Name))
                            {
                                //add it to this state
                                states[CurrentState].AddAction(addType.Name);
                            }
                        }
                    }
                }
                else
                {
                    return StatusValue.FAILED;
                }

                //select which action to use
                CheckIfUsingRandomAction();

                //perform the selected action
                status = children[CurrentActionIndex].Tick(blackboard);

                //update the states
                PreviousState = CurrentState;
                GetState(blackboard);

                //get the reward based on this state and the previous state
                float reward = GetReward(states[PreviousState], children[CurrentActionIndex]);

                //update the Q value table
                string maxArg = FindBestAction();
                float newQ = GetNewQValue(reward, maxArg);
                states[PreviousState].GetScoresList()[CurrentActionName] = newQ;

                //decrement number of episodes left
                states[PreviousState].EpisodeCount--;
                StepsCompleted++;

                //check whether to reject the task we just performed
                if (ShouldActionBeRejected(blackboard))
                {
                    states[CurrentState].RejectAction(children[CurrentActionIndex].GetType());
                }

                //check if this task should be remove from this node entirely (i.e. it will never be useful)
                if (IsTaskUseless(children[CurrentActionIndex]))
                {
                    children.RemoveAt(CurrentActionIndex);
                }

                //return the node's status
                return status;
            }
*/
/*
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
            public bool IsTaskUseless(BTTask taskToCheck)
            {
                int rejectCount = 0;
                foreach (StateClass state in states.Values)
                {
                    if (state.CheckIfRejected(taskToCheck.GetType()))
                    {
                        rejectCount++;
                    }
                }
                if (rejectCount == states.Count)
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

            //Uses the Q-Learning formula to calculate the new q-value and returns it
            protected float GetNewQValue(float reward, string maxArg)
            {
                return (1 - LearningRate) * states[PreviousState].GetScoresList()[CurrentActionName] + LearningRate * (reward + GammaDiscountFactor * states[CurrentState].GetScoresList()[maxArg]);
            }
            */
