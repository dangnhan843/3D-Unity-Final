using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Rigidbody myRigidbody;

    Vector3 velocity;
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }


    void FixedUpdate()
    {
        myRigidbody.MovePosition(myRigidbody.position +
            velocity * Time.fixedDeltaTime);
    }

    public void LookAt(Vector3 lookPoint)
    {

        // change later if RPG follow scene
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x,
            transform.position.y, lookPoint.z);

        transform.LookAt(heightCorrectedPoint);
    }
}
