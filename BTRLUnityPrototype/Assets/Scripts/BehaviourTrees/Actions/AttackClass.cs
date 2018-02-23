using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT_and_RL.Behaviour_Tree;

public class AttackClass : BTTask {
    [SerializeField]
    protected Enemy m_target;
    [SerializeField]
    protected GameObject m_caster;
    [SerializeField]
    protected float m_baseDamage;
    [SerializeField]
    protected float m_damageDealt;  //stores how much damage was done the last time this attack was used
    [SerializeField]
    protected string m_damageType;

    public AttackClass(GameObject _caster)
    {
        m_caster = _caster;
    }

    public void SetTarget(Enemy _target)
    {
        m_target = _target;
    }

    public string GetElementType()
    {
        return m_damageType;
    }

    public virtual float LaunchAttack()
    {
        //calculate damage based on base damage and the target's resistances (in some games this would also involve the caster's stats)
        m_target = m_caster.GetComponent<RLAI>().GetSenses().GetCurrentTarget();
        float damage = m_baseDamage * m_target.GetResistance(m_damageType);
        //deal the damage to the enemy
        m_target.TakeDamage(damage);

        //the damage value returned is used as the reward value
        return damage;
    }

    public float GetDamageDealt()
    {
        return m_damageDealt;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
