using System;
using UnityEngine;

namespace _Scripts {
    public enum zoneType {
        Extract,
        Target,
    }
    
    public class Zone : MonoBehaviour {
        [SerializeField] protected float goalRadius = 0.5f;
        protected SphereCollider Col;
        protected GameManager Gm;
        public bool IsCompleted { get; protected set; } = false;
        public zoneType Type = zoneType.Target;
        [SerializeField] protected Color gizmoColor = Color.blue;
    
        public int id = -1;

        private void Awake() {
            Col = GetComponent<SphereCollider>();
            Col.radius = goalRadius;
        }

        private void Start() {
            Gm = GameManager.instance;
            Gm.RegisterZone(this);
        }
    
        private void OnTriggerEnter(Collider other) {
            if (IsCompleted) return;
            
            GameObject obj = other.gameObject;
            // Debug.Log("goal zone hit by obj: " + obj.name);
            bool isShell = obj.CompareTag("Shell");
            if (!isShell) return;
            
            IsCompleted = true;
            Debug.Log($"completed goal. ID: {id}");
            Gm.CompleteGoal(id);
        }
        
        // private void OnTriggerExit(Collider other) {
        //     if (isCompleted) return;
        //     
        //     GameObject obj = other.gameObject;
        //     // Debug.Log("goal zone hit by obj: " + obj.name);
        //     bool isShell = obj.CompareTag("Shell");
        //     if (!isShell) return;
        //     
        //     isCompleted = true;
        //     Debug.Log($"completed goal. ID: {id}");
        //     _gm.CompleteGoal(id);
        // }

        // Update is called once per frame
        void Update() {
        
        }
    
        void OnDrawGizmos() {
            Color sphereColor;
            if (IsCompleted) sphereColor = Color.black;
            else sphereColor = gizmoColor;
        
            sphereColor.a = 0.5f; // make transparent
            Gizmos.color = sphereColor;
            Gizmos.DrawSphere(transform.position, goalRadius);
        }
    }
}