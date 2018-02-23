using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

public class WaterAttack : AttackClass {

    public WaterAttack(GameObject _caster) : base(_caster)
    {
        m_baseDamage = 50;
        m_damageType = "Water";
        taskName = "WaterAttack";
    }

    public override StatusValue Tick()
    {
        m_damageDealt = LaunchAttack();
        return StatusValue.SUCCESS;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
