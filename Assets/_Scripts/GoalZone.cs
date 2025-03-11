using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalZone : MonoBehaviour {
    [SerializeField] private float goalRadius = 0.5f;
    private SphereCollider col;
    // Start is called before the first frame update
    void Start() {
        col = GetComponent<SphereCollider>();
        col.radius = goalRadius;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnDrawGizmos() {
        Color sphereColor = Color.blue;
        sphereColor.a = 0.5f;
        Gizmos.color = sphereColor;
        Gizmos.DrawSphere(transform.position, goalRadius);
    }
}
