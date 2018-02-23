using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {
    GameObject player;
    float rotSpeed = 5.0f;
    public float minAngle = 5.0f;
    public float maxAngle = 60.0f;
    public float xRot;
    // Use this for initialization
    void Start () {
        xRot = transform.rotation.eulerAngles.x;
        player = transform.parent.gameObject;
	}

    // Update is called once per frame
    void Update () {
        float verticalRot = Input.GetAxis("Mouse Y") * -rotSpeed;
        xRot += verticalRot;
        //float horizontalRot = Input.GetAxis("Mouse X") * rotSpeed;
        if (xRot <= maxAngle && xRot >= minAngle)
        {
            transform.RotateAround(player.transform.position, player.transform.right, verticalRot);
        }
        else
        {
            if(xRot > maxAngle)
            {
                xRot = maxAngle;
            }
            else
            {
                xRot = minAngle;
            }
        }
        
    }
}
