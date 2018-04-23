using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BT_and_RL.Behaviour_Tree;

namespace BT_and_RL.QLearning
{
    //object factory class to instantiate actions when dynamically added to the behaviour tree
    public class ActionFactory
    {
        protected Dictionary<string, BTTask> actionPool;

        public object GetAction(string name)
        {
            if (actionPool.ContainsKey(name))
            {
                return actionPool[name];
            }

            return "action not found";
        }

        public BTTask GetRandomAction()
        {
            BTTask get = actionPool.Values.ElementAt(CustomExtensions.ThreadSafeRandom.ThisThreadsRandom.Next(0, actionPool.Count));
            return get;
        }
    }

    /*
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
                Type[] actions = assemblies[i].GetTypes().Where(t => t.IsSubclassOf(typeof(BTTask))).ToArray();
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
        if (actionPool.ContainsKey(name))
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
    }*/
}
