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
    public float bufferFactorOrtho = 1.2f;
    
    // public float zDistMultiplier = 5f;

    [SerializeField] private float xOffset = 0f;
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private float zOffset = 1f;

    private float _initialZOffset = -10f;
    
    private float _topHeight;
    private float _bottomHeight;
    private float _nextYPos;
    private float _maxHeight => Mathf.Abs(_topHeight - _bottomHeight);
    
    private float _maxRight;
    private float _maxLeft;
    private float _maxWidth => Mathf.Abs(_maxLeft - _maxRight);
    
    private float _nextSize;
    
    public float minimumCameraSizeOrtho = 1f;
    private float MinOrthoCamSize => minimumCameraSizeOrtho;
    
    private Transform _tr;
    // private Vector3 _pos => _tr.position;
    private Camera _cam;

    private float shellDist;
    private Vector3 midpoint;

    private void Start() {
        _tr = transform;
        _cam = GetComponent<Camera>();
        
        Vector3 midpoint = Vector3.Lerp(t1Pos, t2Pos, 0.5f);
        _initialZOffset = _tr.position.z - midpoint.z;
        _topHeight = Mathf.Max(t1Pos.y, t2Pos.y);
        _bottomHeight = Mathf.Min(t1Pos.y, t2Pos.y);
        _maxLeft = Mathf.Max(t1Pos.x, t2Pos.x);
        _maxRight = Mathf.Min(t1Pos.x, t2Pos.x);
        
        if (target1 == null || target2 == null) _tr.gameObject.SetActive(false);
    }

    void LateUpdate() {
        shellDist = Vector3.Distance(t1Pos, t2Pos);
        midpoint = Vector3.Lerp(t1Pos, t2Pos, 0.5f);
        
        _topHeight = Mathf.Max(t1Pos.y, t2Pos.y, _topHeight);
        _bottomHeight = Mathf.Min(t1Pos.y, t2Pos.y, _bottomHeight);
        _nextYPos = Mathf.Lerp(_topHeight, _bottomHeight, 0.5f);
        
        _maxLeft = Mathf.Max(t1Pos.x, t2Pos.x, _maxLeft);
        _maxRight = Mathf.Min(t1Pos.x, t2Pos.x, _maxRight);
        

        if (_cam.orthographic) {
            ManipulateOrtho();
        }
        else {
            ManipulatePerspective();
        }
        
    }

    private void ManipulateOrtho() {
        float newSize = Mathf.Max(shellDist, _maxHeight, _maxWidth) / 2f * bufferFactorOrtho;
        _cam.orthographicSize = Mathf.Max(newSize, MinOrthoCamSize);
        _tr.position = new Vector3(midpoint.x, _nextYPos, _initialZOffset);
    }

    private void ManipulatePerspective() {
        float minDistance = shellDist / (Mathf.Tan(_cam.fieldOfView * distanceMultiplier * Mathf.Deg2Rad) * 2f);
        minDistance /= _cam.aspect;  // Adjust the distance based on the screen's aspect ratio
        minDistance *= bufferFactor;

        float newDistance = Mathf.Clamp(minDistance, 11, 1000);
        Vector3 offsetDir = new Vector3(xOffset, yOffset, zOffset).normalized;
        
        // Set the new camera pos, placing it at the correct distance along chosen axis

        // todo rewrite this part to account for different offsets
        Vector3 newCamPos = new Vector3(midpoint.x, _nextYPos, midpoint.z);
        newCamPos -= offsetDir * newDistance;
        _tr.position = newCamPos;
            
        // transform.LookAt(midpoint);
    }
}