using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileCam : MonoBehaviour
{
    public Transform target1;
    private Vector3 t1Pos => target1.position;
    public Transform target2;
    private Vector3 t2Pos => target2.position;
    
    public float distanceMultiplier = 0.5f; // Adjusts distance sensitivity
    public float bufferFactor = 1.2f;
    
    // public float zDistMultiplier = 5f;

    [SerializeField] private float xOffset = 0f;
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private float zOffset = 0f;
    
    private Transform _tr;
    // private Vector3 _pos => _tr.position;
    private Camera _cam;

    private void Start() {
        _tr = transform;
        _cam = GetComponent<Camera>();
    }

    void LateUpdate() {
        if (target1 == null || target2 == null) return;
        
        float distance = Vector3.Distance(t1Pos, t2Pos);
        Vector3 midpoint = Vector3.Lerp(t1Pos, t2Pos, 0.5f);
        
        float minDistance = distance / (Mathf.Tan(_cam.fieldOfView * distanceMultiplier * Mathf.Deg2Rad) * 2f);
        minDistance /= _cam.aspect;  // Adjust the distance based on the screen's aspect ratio
        minDistance *= bufferFactor;

        float newDistance = Mathf.Clamp(minDistance, 11, 1000);

        // Set the new camera position, placing it at the correct distance along the z-axis
        Vector3 newCamPos = midpoint - transform.forward * newDistance;
        _tr.position = newCamPos;

        // Ensure the camera always looks at the midpoint between the two targets
        transform.LookAt(midpoint);
    }


}