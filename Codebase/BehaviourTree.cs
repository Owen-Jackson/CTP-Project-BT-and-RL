using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using CustomExtensions;

namespace BT_and_RL
{
    //namespace used for behaviour trees
    namespace Behaviour_Tree
    {
        enum StatusValue
        {
            NULL = 0,
            SUCCESS,
            FAILED,
            RUNNING,
        }

        //Class for the main tree to inherit from. Contains the nodes as a set of children
        class BTTree
        {
            protected List<BTTask> children;

            //Each frame tick the tree once
            public void Tick()
            {
                foreach(BTTask c in children)
                {
                    c.Tick();
                }
            }
        }

        //Base class for any task within a Behaviour Tree
        class BTTask
        {
            protected StatusValue status;
            protected List<BTTask> children;

            virtual public StatusValue Tick()
            {
                status = StatusValue.RUNNING;
                return status;
            }

            virtual public void Terminate()
            {
                status = StatusValue.FAILED;
            }
        }

        //Base class for a condition check
        abstract class BTCondition : BTTask
        {
            abstract public bool CheckCondition();
        }

        //Base class for a single action to perform (leaf node)
        abstract class BTAction : BTTask
        {
            private StatusValue status;

            abstract public StatusValue PerformAction();
        }

        //A composite task that stops at first successful action
        class BTSelector : BTTask
        {
            private BTTask currentChild = null;

            public override StatusValue Tick()
            {
                status = StatusValue.RUNNING;
                foreach (BTTask c in children)
                {
                    if (c.Tick() == StatusValue.SUCCESS)
                    {
                        status = StatusValue.SUCCESS;
                        return status;
                    }
                }                
                status = StatusValue.FAILED;
                return status;
            }
        }

        //Selector that randomises list before checking
        class BTShuffleSelector : BTTask
        {
            public override StatusValue Tick()
            {
                children.Shuffle();
                foreach (BTTask c in children)
                {
                    if(c.Tick() == StatusValue.SUCCESS)
                    {
                        status = StatusValue.SUCCESS;
                        return status;
                    }
                }
                status = StatusValue.FAILED;
                return status;
            }
        }

        //A composite task that stops at first failed action
        class BTSequence : BTTask
        {
            public override StatusValue Tick()
            {
                foreach (BTTask c in children)
                {
                    if (c.Tick() != StatusValue.SUCCESS)
                    {
                        status = StatusValue.FAILED;
                        return status;
                    }
                }
                status = StatusValue.SUCCESS;
                return status;
            }
        }

        //Sequence that randomises list before checking
        class BTShuffleSequence : BTTask
        {
            public override StatusValue Tick()
            {
                children.Shuffle();
                foreach(BTTask c in children)
                {
                    if(c.Tick() != StatusValue.SUCCESS)
                    {
                        status = StatusValue.FAILED;
                        return status;
                    }
                }
                status = StatusValue.SUCCESS;
                return status;
            }
        }

        //A parallel task that runs its children concurrently
        class BTParallel : BTTask
        {
            //List of children currently running
            protected List<BTTask> running_children;

            StatusValue result;

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
        class BTDecorator : BTTask
        {
            protected BTTask child = null;
            public override StatusValue Tick()
            {
                status = StatusValue.RUNNING;
                if(child != null)
                {
                    if(child.Tick() == StatusValue.SUCCESS)
                    {
                        status = StatusValue.SUCCESS;
                        return status;
                    }
                }
                return StatusValue.FAILED;
            }
        }

        //Decorator that inverts the its child's return value
        class BTInverter : BTDecorator
        {
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
        class BTSemaphoreGuard : BTDecorator
        {
            private Semaphore semaphore;

            BTSemaphoreGuard(Semaphore semaphore)
            {
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
