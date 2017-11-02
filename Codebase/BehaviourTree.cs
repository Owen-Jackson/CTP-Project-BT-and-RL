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
            SUCCESS = 0,
            FAILED = 1,
            RUNNING = 2,
            ERROR = 3
        }

        //Base class for any task within a Behaviour Tree
        class BTTask
        {
            protected StatusValue status;
            protected List<BTTask> children;

            virtual public StatusValue Run()
            {
                status = StatusValue.RUNNING;
                return status;
            }
        }

        //Base class for a condition check
        abstract class BTCondition : BTTask
        {
            abstract public bool CheckCondition();
        }

        //Base class for an action to perform
        //abstract class BTAction : BTTask
        //{
        //}

        //Stops at first successful action
        class BTSelector : BTTask
        {
            public override StatusValue Run()
            {
                status = StatusValue.RUNNING;
                foreach (BTTask c in children)
                {

                    if (c.Run() == StatusValue.SUCCESS)
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
            public override StatusValue Run()
            {
                children.Shuffle();
                foreach (BTTask c in children)
                {
                    if(c.Run() == StatusValue.SUCCESS)
                    {
                        status = StatusValue.SUCCESS;
                        return status;
                    }
                }
                status = StatusValue.FAILED;
                return status;
            }
        }

        //Stops at first failed action
        class BTSequence : BTTask
        {
            public override StatusValue Run()
            {
                foreach (BTTask c in children)
                {
                    if (c.Run() != StatusValue.SUCCESS)
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
            public override StatusValue Run()
            {
                children.Shuffle();
                foreach(BTTask c in children)
                {
                    if(c.Run() != StatusValue.SUCCESS)
                    {
                        status = StatusValue.FAILED;
                        return status;
                    }
                }
                status = StatusValue.SUCCESS;
                return status;
            }
        }

        //Decorator node
        class BTDecorator : BTTask
        {
            public override StatusValue Run()
            {
                if(children.Count > 0)
                {
                    if(children[0].Run() == StatusValue.SUCCESS)
                    {
                        status = StatusValue.SUCCESS;
                        return status;
                    }
                }
                return StatusValue.FAILED;
            }
        }

        //Decorator node that guards a thread INCOMPLETE
        /*
        class BTSemaphoreGuard : BTDecorator
        {
            private Semaphore semaphore;

            BTSemaphoreGuard(Semaphore semaphore)
            {
                this.semaphore = semaphore;
            }

            public override StatusValue Run()
            {
                if(semaphore.)
            }
        }
        */
    }
}
