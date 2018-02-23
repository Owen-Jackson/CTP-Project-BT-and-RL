using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

public class CompanionTest : MonoBehaviour {
    public Vector3 m_targetPos;

    [SerializeField]
    private AITree m_testTree;
    [SerializeField]
    private StatusValue m_treeStatus;
    [SerializeField]
    private AIPerceptor m_environmentPerception;
    //[SerializeField]
    //private float timer = 0;

    // Use this for initialization
    void Start()
    {
        m_testTree = new AITree(new BTSequence(new List<BTTask>() { new MoveTo(this.gameObject, null), new Idle() }));
        m_testTree.BeginTree();
        m_environmentPerception = GetComponent<AIPerceptor>();
    }

    // Update is called once per frame
    void Update()
    {
        m_treeStatus = m_testTree.GetStatus();
        if(m_treeStatus == StatusValue.RUNNING)
        {
            m_testTree.Tick();
        }
        //timer += Time.deltaTime;
    }
}
