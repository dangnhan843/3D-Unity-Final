using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerT;
    private Vector3 _cameraOffset;

    [Range(0.01f, 1.0f)]
    public float SmoothFactor = 0.5f;
    void Start()
    {
        _cameraOffset = transform.position - playerT.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 newPos = playerT.position + _cameraOffset;
        transform.position = Vector3.Slerp(transform.position, newPos, SmoothFactor);
    }
}
