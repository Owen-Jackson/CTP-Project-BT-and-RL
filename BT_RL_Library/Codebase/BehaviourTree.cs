using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using UnityEngine;
using CustomExtensions;

namespace BT_and_RL
{
    //namespace used for behaviour trees
    namespace Behaviour_Tree
    {
        public enum StatusValue
        {
            NULL = 0,
            SUCCESS,
            FAILED,
            RUNNING,
        }

        //Blackboard class used by the tree to read and write information that this agent knows
        //NOTE: Might not be thread-safe yet (will look into if it becomes a problem)
        [Serializable]
        public class Blackboard
        {
            public Dictionary<string, object> memory;

            public Blackboard()
            {
                memory = new Dictionary<string, object>();
            }

            public object GetValue(string valueName)
            {
                if (memory.ContainsKey(valueName))
                {
                    return memory[valueName];
                }
                return null;
            }

            public void SetValue(string valueName, object newValue)
            {
                if(memory.ContainsKey(valueName))
                {
                    memory[valueName] = newValue;
                }
                else
                {
                    memory.Add(valueName, newValue);
                }
            }
        }

        //Class for the main tree to inherit from. This is the root node of the tree and it should only have one child
        [Serializable]
        public class BTTree
        {
            protected StatusValue status;
            [SerializeField]
            protected BTTask child;
            protected Blackboard blackboard;
            public Blackboard Blackboard
            {
                get { return blackboard; }
                set { blackboard = value; }
            }

            public BTTree(BTTask root)
            {
                blackboard = new Blackboard();
                child = root;
                BeginTree();
            }

            public void BeginTree()
            {
                status = StatusValue.RUNNING;
            }

            //Each frame ticks the tree once
            public void Tick()
            {
                status = child.Tick(blackboard);
            }

            public StatusValue GetStatus()
            {
                return status;
            }
        }

        //Base class for any task within a Behaviour Tree
        [Serializable]
        public class BTTask
        {
            [SerializeField]
            protected StatusValue status;
            [SerializeField]
            protected string taskName;

            protected HashSet<int> compatibility;   //stores the situations that this task can be applied in (links to an enum defined in the game's project)

            public BTTask()
            {
                compatibility = new HashSet<int>();
            }
            
            //called when first ticked to set it as running
            virtual public void Begin()
            {
                status = StatusValue.RUNNING;
            }

            virtual public StatusValue Tick(Blackboard blackboard)
            {
                status = StatusValue.RUNNING;
                return status;
            }

            virtual public void Terminate()
            {
                status = StatusValue.FAILED;
            }

            public StatusValue GetStatus()
            {
                return status;
            }

            public string GetName()
            {
                return taskName;
            }

            //this checks if the task is compatible with the node that is trying to add it (for RL purposes)
            public bool IsTaskCompatible(HashSet<int> compareWith)
            {
                foreach(int num in compareWith)
                {
                    if(compatibility.Contains(num))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        //Base class for a condition check
        [Serializable]
        public abstract class BTCondition : BTTask
        {
            abstract public bool CheckCondition();
        }

        //Base class for a single action to perform (leaf node)
        //This class is no different to a BTTask, however it is used by the system to differentiate between
        //a character action and a general BTTask class.
        //This is how the action pool is filled
        [Serializable]
        public abstract class BTAction : BTTask
        {
            public BTAction()
            {

            }
        }

        //A composite task that stops at first successful action
        [Serializable]
        public class BTSelector : BTTask
        {
            protected int currentChildIndex = 0;
            [SerializeField]
            protected List<BTTask> children = new List<BTTask>();

            public BTSelector(List<BTTask> tasks)
            {
                children = tasks;
            }

            public void Initialise()
            {
                currentChildIndex = 0;
            }

            public override StatusValue Tick(Blackboard blackboard)
            {
                currentChildIndex = 0;
                for (int i = 0; i < children.Count; i++)
                {
                    if (i == currentChildIndex)
                    {
                        status = children[i].Tick(blackboard);
                        if (status != StatusValue.FAILED)
                        {
                            return status;
                        }
                    }
                    currentChildIndex++;
                }
                status = StatusValue.FAILED;
                return status;
            }

            public virtual void AddTask(string taskName)
            {
                if(QLearning.ActionPool.Instance.actionPool.ContainsKey(taskName))
                {
                    children.Add((BTTask)Activator.CreateInstance(QLearning.ActionPool.Instance.actionPool[taskName]));
                }
            }

        }

        //Selector that randomises list before checking
        [Serializable]
        public class BTShuffleSelector : BTTask
        {
            [SerializeField]
            protected List<BTTask> children = new List<BTTask>();

            public BTShuffleSelector(List<BTTask> tasks)
            {
                children = tasks;
            }

            public override StatusValue Tick(Blackboard blackboard)
            {
                children.Shuffle();
                foreach (BTTask c in children)
                {
                    status = c.Tick(blackboard);
                    if(status != StatusValue.FAILED)
                    {
                        return status;
                    }
                }
                status = StatusValue.FAILED;
                return status;
            }

            public void AddTask(string taskName)
            {
                if (QLearning.ActionPool.Instance.actionPool.ContainsKey(taskName))
                {
                    children.Add((BTTask)Activator.CreateInstance(QLearning.ActionPool.Instance.actionPool[taskName]));
                }
            }
        }

        //A composite task that stops at first failed action
        [Serializable]
        public class BTSequence : BTTask
        {
            protected int currentChildIndex = 0;
            [SerializeField]
            protected List<BTTask> children = new List<BTTask>();

            public BTSequence(List<BTTask> tasks)
            {
                children = tasks;
            }

            public void Initialise()
            {
                currentChildIndex = 0;
            }

            public override StatusValue Tick(Blackboard blackboard)
            {
                currentChildIndex = 0;
                for(int i = 0; i < children.Count(); i++)
                {
                    if (i == currentChildIndex)
                    {
                        status = children[i].Tick(blackboard);
                        if (status != StatusValue.SUCCESS)
                        {
                            return status;
                        }
                    }
                    currentChildIndex++;
                }
                status = StatusValue.SUCCESS;
                return status;
            }

            public virtual void AddTask(string taskName)
            {
                if (QLearning.ActionPool.Instance.actionPool.ContainsKey(taskName))
                {
                    children.Add((BTTask)Activator.CreateInstance(QLearning.ActionPool.Instance.actionPool[taskName]));
                }
            }
        }

        //Sequence that randomises list before checking
        [Serializable]
        public class BTShuffleSequence : BTTask
        {
            [SerializeField]
            protected List<BTTask> children = new List<BTTask>();

            public BTShuffleSequence(List<BTTask> tasks)
            {
                children = tasks;
            }

            public override StatusValue Tick(Blackboard blackboard)
            {
                children.Shuffle();
                foreach(BTTask c in children)
                {
                    status = c.Tick(blackboard);
                    if (status != StatusValue.SUCCESS)
                    {
                        return status;
                    }
                }
                status = StatusValue.SUCCESS;
                return status;
            }

            public void AddTask(string taskName)
            {
                if (QLearning.ActionPool.Instance.actionPool.ContainsKey(taskName))
                {
                    children.Add((BTTask)Activator.CreateInstance(QLearning.ActionPool.Instance.actionPool[taskName]));
                }
            }
        }

        //A parallel task that runs its children concurrently
        [Serializable]
        public class BTParallel : BTTask
        {
            //List of children currently running
            protected List<BTTask> running_children;

            StatusValue result;
            [SerializeField]
            protected List<BTTask> children = new List<BTTask>();

            public BTParallel(List<BTTask> tasks)
            {
                children = tasks;
            }

            public override StatusValue Tick(Blackboard blackboard)
            {
                result = StatusValue.NULL;

                foreach(BTTask c in children)
                {
                    Thread thread = new Thread(() => RunChild(c, blackboard));
                    thread.Start();
                }
                //Sleep this thread between checks for completion
                while(result == StatusValue.NULL)
                {
                    Thread.Sleep(100);
                }
                return result;
            }

            //Runs the current child in its own thread
            protected void RunChild(BTTask child, Blackboard blackboard)
            {
                running_children.Add(child);
                StatusValue returned = child.Tick(blackboard);
                running_children.Remove(child);

                //If the child fails, terminate
                if(returned == StatusValue.FAILED)
                {
                    Terminate();
                    result = StatusValue.FAILED;
                }
                //If all children succeed, this has succeeded
                else if(running_children.Count == 0)
                {
                    result = StatusValue.SUCCESS;
                }
            }

            //Parallel tasks fail when any child fails. It must then tell all other children to terminate
            public override void Terminate()
            {
                foreach(BTTask c in running_children)
                {
                    c.Terminate();
                }
            }
            public void AddTask(string taskName)
            {
                if (QLearning.ActionPool.Instance.actionPool.ContainsKey(taskName))
                {
                    children.Add((BTTask)Activator.CreateInstance(QLearning.ActionPool.Instance.actionPool[taskName]));
                }
            }

        }

        //A decorator task that only has one child
        public class BTDecorator : BTTask
        {
            [SerializeField]
            protected BTTask child = null;

            public BTDecorator(BTTask task)
            {
                child = task;
            }

            public override StatusValue Tick(Blackboard blackboard)
            {
                status = StatusValue.RUNNING;
                if(child != null)
                {
                    status = child.Tick(blackboard);
                    if (status != StatusValue.FAILED)
                    {
                        return status;
                    }
                }
                return StatusValue.FAILED;
            }
        }

        //Decorator that inverts the its child's return value
        [Serializable]
        public class BTInverter : BTDecorator
        {
            public BTInverter(BTTask task) : base(task)
            {
                child = task;
            }

            public override StatusValue Tick(Blackboard blackboard)
            {
                status = StatusValue.RUNNING;
                status = base.Tick(blackboard);
                //Invert the result of the child node
                if(status == StatusValue.SUCCESS)
                {
                    status = StatusValue.FAILED;
                    return status;
                }
                else if(status == StatusValue.FAILED)
                {
                    status = StatusValue.SUCCESS;
                    return status;
                }
                return status;
            }
        }

        //Decorator node that guards a thread
        [Serializable]
        public class BTSemaphoreGuard : BTDecorator
        {
            protected Semaphore semaphore;

            BTSemaphoreGuard(BTTask task, Semaphore semaphore) : base(task)
            {
                child = task;
                this.semaphore = semaphore;
            }

            public override StatusValue Tick(Blackboard blackboard)
            {
                if(semaphore.WaitOne())
                {
                    status = child.Tick(blackboard);
                    semaphore.Release();
                    return status;
                }
                return StatusValue.FAILED;
            }
        }        
    }
}
