using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    GameObject car;
    private Vector3 offset;
    Vector3 camoffset = new Vector3(0.0f,1.2f,0.0f);
    public float smoothTime = 1.0F;
    private Vector3 velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        // カメラの球からの相対的位置
    }

    // Update is called once per frame
    void LateUpdate()
    {
    	car = GameObject.Find("Car@Blue(Clone)");
        //this.transform.position = list[0] + camoffset;//car.transform.position + camoffset + car.transform.forward * -6.0f;
        //this.transform.LookAt(car.transform.position);// = list[0] - this.transform.position;//car.transform.rotation * Quaternion.Euler(6.0f, 0.0f, 0.0f);
    	Vector3 targetPosition = car.transform.TransformPoint(new Vector3(0, 1.0f, -4));
     	var targetRotation = Quaternion.LookRotation(car.transform.position - transform.position);
       
             // Smoothly move the camera towards that target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5.0f * Time.deltaTime);
        
        
    }
}
