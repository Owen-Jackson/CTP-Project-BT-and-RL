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

        //Class for the main tree to inherit from. This is the root node of the tree and it should only have one child
        [Serializable]
        public class BTTree
        {
            protected StatusValue status;
            protected BTTask child;

            public BTTree(BTTask root)
            {
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
                status = child.Tick();
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
            protected StatusValue status;
            protected string taskName;

            //called when first ticked to set it as running
            virtual public void Begin()
            {
                status = StatusValue.RUNNING;
            }

            virtual public StatusValue Tick()
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

            //public void AddChild(BTTask newChild)
            //{
            //    children.Add(newChild);
            //}
        }

        //Base class for a condition check
        [Serializable]
        public abstract class BTCondition : BTTask
        {
            abstract public bool CheckCondition();
        }

        //Base class for a single action to perform (leaf node)
        [Serializable]
        public abstract class BTAction : BTTask
        {
            protected StatusValue status;

            abstract public StatusValue PerformAction();
        }

        //A composite task that stops at first successful action
        [Serializable]
        public class BTSelector : BTTask
        {
            protected int currentChildIndex = 0;
            protected List<BTTask> children = new List<BTTask>();

            public BTSelector(List<BTTask> tasks)
            {
                children = tasks;
            }

            public void Initialise()
            {
                currentChildIndex = 0;
            }

            public override StatusValue Tick()
            {
                currentChildIndex = 0;
                for (int i = 0; i < children.Count; i++)
                {
                    if (i == currentChildIndex)
                    {
                        status = children[i].Tick();
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
        }

        //Selector that randomises list before checking
        [Serializable]
        public class BTShuffleSelector : BTTask
        {
            protected List<BTTask> children = new List<BTTask>();

            public BTShuffleSelector(List<BTTask> tasks)
            {
                children = tasks;
            }

            public override StatusValue Tick()
            {
                children.Shuffle();
                foreach (BTTask c in children)
                {
                    status = c.Tick();
                    if(status != StatusValue.FAILED)
                    {
                        return status;
                    }
                }
                status = StatusValue.FAILED;
                return status;
            }
        }

        //A composite task that stops at first failed action
        [Serializable]
        public class BTSequence : BTTask
        {
            protected int currentChildIndex = 0;
            protected List<BTTask> children = new List<BTTask>();

            public BTSequence(List<BTTask> tasks)
            {
                children = tasks;
            }

            public void Initialise()
            {
                currentChildIndex = 0;
            }

            public override StatusValue Tick()
            {
                currentChildIndex = 0;
                for(int i = 0; i < children.Count(); i++)
                {
                    if (i == currentChildIndex)
                    {
                        status = children[i].Tick();
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
        }

        //Sequence that randomises list before checking
        [Serializable]
        public class BTShuffleSequence : BTTask
        {
            protected List<BTTask> children = new List<BTTask>();

            public BTShuffleSequence(List<BTTask> tasks)
            {
                children = tasks;
            }

            public override StatusValue Tick()
            {
                children.Shuffle();
                foreach(BTTask c in children)
                {
                    status = c.Tick();
                    if (status != StatusValue.SUCCESS)
                    {
                        return status;
                    }
                }
                status = StatusValue.SUCCESS;
                return status;
            }
        }

        //A parallel task that runs its children concurrently
        [Serializable]
        public class BTParallel : BTTask
        {
            //List of children currently running
            protected List<BTTask> running_children;

            StatusValue result;
            protected List<BTTask> children = new List<BTTask>();

            public BTParallel(List<BTTask> tasks)
            {
                children = tasks;
            }

            public override StatusValue Tick()
            {
                result = StatusValue.NULL;

                foreach(BTTask c in children)
                {
                    Thread thread = new Thread(() => RunChild(c));
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
            protected void RunChild(BTTask child)
            {
                running_children.Add(child);
                StatusValue returned = child.Tick();
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
        }

        //A decorator task that only has one child
        public class BTDecorator : BTTask
        {
            protected BTTask child = null;

            public BTDecorator(BTTask task)
            {
                child = task;
            }

            public override StatusValue Tick()
            {
                status = StatusValue.RUNNING;
                if(child != null)
                {
                    status = child.Tick();
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

            public override StatusValue Tick()
            {
                status = StatusValue.RUNNING;
                status = base.Tick();
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

            public override StatusValue Tick()
            {
                if(semaphore.WaitOne())
                {
                    status = child.Tick();
                    semaphore.Release();
                    return status;
                }
                return StatusValue.FAILED;
            }
        }        
    }
}
