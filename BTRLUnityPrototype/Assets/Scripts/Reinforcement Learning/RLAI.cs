using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

public class RLAI : MonoBehaviour
{
    public BTTree tree;
    [SerializeField]
    private int m_stepsToMinEps = 1000; //number of steps to go from starting epsilon to minEpsilon
    [SerializeField]
    private int m_episodeNum;
    [SerializeField]
    private AIPerceptor senses;
    [SerializeField]
    private List<GameObject> targetPositions;

    // Use this for initialization
    void Start()
    {
        senses = GetComponentInChildren<AIPerceptor>();
        tree = new BTTree(
            new BTSelector(new List<BTTask>()
            {
                new Timer(
                    new MoveTo(this.gameObject, targetPositions),
                    5f),

                new RLAttack(
                    new List<BTTask>()
                    {
                        new FireballAttack(this.gameObject),
                        new WaterAttack(this.gameObject),
                        new AirAttack(this.gameObject),
                        new EarthAttack(this.gameObject)
                        },
                    m_stepsToMinEps,
                    this.gameObject,
                    senses)
            }));

        tree.BeginTree();
        //tree.Tick();
    }

    public AIPerceptor GetSenses()
    {
        return senses;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(tree.GetStatus().ToString());
        if (tree.GetStatus() == StatusValue.RUNNING)
        {
            tree.Tick();
            //m_episodeNum++;
        }
        if (tree.GetStatus() != StatusValue.RUNNING)
        {
            tree.BeginTree();
        }
    }
}

