using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts;
using UnityEngine;

public class GoalZone : MonoBehaviour {
    [SerializeField] private float goalRadius = 0.5f;
    private SphereCollider _col;
    private GameManager _gm;
    
    public int ID;

    void Start() {
        
    }

    private void Awake() {
        _col = GetComponent<SphereCollider>();
        _col.radius = goalRadius;
    }

    private void OnEnable() {
        _gm = GameManager.instance;
        
        _gm.RegisterGoalZone(this);
    }
    
    private void OnCollisionEnter(Collision other) {
        bool isShell = other.gameObject.CompareTag("Shell");
        if (!isShell) return;
        
        
    }

    // Update is called once per frame
    void Update() {
        
    }
    
    void OnDrawGizmos() {
        Color sphereColor = Color.blue;
        sphereColor.a = 0.5f;
        Gizmos.color = sphereColor;
        Gizmos.DrawSphere(transform.position, goalRadius);
    }
}
