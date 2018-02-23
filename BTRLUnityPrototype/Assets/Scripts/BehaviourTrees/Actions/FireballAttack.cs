using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

public class FireballAttack : AttackClass {

    public FireballAttack(GameObject _caster) : base(_caster)
    {
        m_baseDamage = 50;
        m_damageType = "Fire";
        taskName = "FireballAttack";
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
