using System.Collections;
using System.Collections.Generic;
using BT_and_RL.Behaviour_Tree;
using UnityEngine;

public class AirAttack : AttackClass {

    public AirAttack(GameObject _caster) : base(_caster)
    {
        m_baseDamage = 50;
        m_damageType = "Air";
        taskName = "AirAttack";
    }

    public override StatusValue Tick()
    {
        m_damageDealt = LaunchAttack();
        return StatusValue.SUCCESS;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
