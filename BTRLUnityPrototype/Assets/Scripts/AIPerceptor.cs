using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPerceptor : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private float distanceFromPlayer;
    [SerializeField]
    private Collider radiusSensor;
    [SerializeField]
    private List<GameObject> targets;
    [SerializeField]
    private Enemy currentTarget;
    [SerializeField]
    private List<GameObject> interactables;
    [SerializeField]
    private float angleOfVision;
    //[SerializeField]
    //private BlackBoard blackboard;
    
    // Use this for initialization
    void Start()
    {
        targets = new List<GameObject>();
        interactables = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);
    }

    public float GetDistanceFromPlayer()
    {
        return distanceFromPlayer;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if(!targets.Contains(other.gameObject))
            {
                targets.Add(other.gameObject);
            }
        }
        if(other.CompareTag("Interactable"))
        {
            if(!interactables.Contains(other.gameObject))
            {
                interactables.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(targets.Contains(other.gameObject))
        {
            targets.Remove(other.gameObject);
        }
        if(interactables.Contains(other.gameObject))
        {
            interactables.Remove(other.gameObject);
        }
        if (currentTarget)
        {
            if (other.gameObject == currentTarget.gameObject)
            {
                currentTarget = null;
            }
        }
    }

    public void RemoveTargetFromList()
    {
        currentTarget.gameObject.SetActive(false);
        targets.Remove(currentTarget.gameObject);
        //Destroy(currentTarget.gameObject);
        currentTarget = null;
    }

    public bool SelectEnemy()
    {
        if (targets.Count > 0)
        {
            currentTarget = targets.FirstOrDefault(x => x.tag == "Enemy").GetComponent<Enemy>();
            if (currentTarget != null)
            {
                return true;
            }
        }
        return false;
    }

    public Enemy GetCurrentTarget()
    {
        return currentTarget;
    }

    public void SetCurrentTarget(Enemy newTarget)
    {
        currentTarget = newTarget;
    }
}
