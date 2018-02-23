using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireElemental : Enemy {

	// Use this for initialization
	new void Start () {
        base.Start();
        m_resistances["Water"] = 2f;
        m_resistances["Earth"] = 1f;
        m_resistances["Air"] = 1f;
        m_resistances["Fire"] = 0f;
        m_maxHealth = 10000;
        m_health = 10000;
        m_name = "Fire Elemental";
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
