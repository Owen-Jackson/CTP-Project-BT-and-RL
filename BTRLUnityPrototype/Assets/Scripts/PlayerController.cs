using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private float moveSpeed = 5.0f;
    private float rotSpeed = 5.0f;
    private Vector3 velocity = new Vector3(0, 0, 0);
    private Vector3 acceleration;

	// Use this for initialization
	void Start () {
		
	}

    void Update()
    {
        float rotAngle = Input.GetAxisRaw("Mouse X") * rotSpeed;
        transform.Rotate(Vector3.up, rotAngle);

        float right = Input.GetAxis("Horizontal");
        float forward = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(right, 0, forward) * Time.deltaTime * moveSpeed;

        transform.Translate(movement);
    }
}
