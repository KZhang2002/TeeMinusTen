using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts {
    public enum zoneType {
        Extract,
        Target,
    }
    
    public class Zone : MonoBehaviour {
        [SerializeField] public float goalRadius = 0.5f;
        protected SphereCollider Col;
        protected GameManager Gm;
        public bool isCompleted { get; protected set; } = false;
        public zoneType type = zoneType.Target;
        [SerializeField] protected Color gizmoColor = Color.blue;
        
        [SerializeField] public bool isOpen = true;
    
        public int id = -1;

        private void Awake() {
            Col = GetComponent<SphereCollider>();
            Col.radius = goalRadius;
        }

        private void Start() {
            Gm = GameManager.instance;
            Gm.RegisterZone(this);
            if (type == zoneType.Extract) {
                isOpen = false;
            }
        }
    
        private void OnTriggerEnter(Collider other) {
            if (isCompleted) return;
            
            GameObject obj = other.gameObject;
            // Debug.Log("goal zone hit by obj: " + obj.name);
            bool isShell = obj.CompareTag("Shell");
            if (!isShell) return;
            
            isCompleted = true;
            Debug.Log($"completed goal. ID: {id}");
            Gm.CompleteGoal(id);

            if (type == zoneType.Extract && isOpen) {
                isCompleted = true;
                Gm.Extract();
            }
        }

        //todo connect to event system
        public void OpenExtract() {
            if (type != zoneType.Extract) return;
            
            isOpen = true;
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
            Color sphereColor = gizmoColor;

            // if (type == zoneType.Extract) {
            //     sphereColor = _isOpen ? Color.green : Color.black;
            // }

            if (isCompleted || !isOpen) sphereColor = Color.black;
        
            sphereColor.a = 0.5f; // make transparent
            Gizmos.color = sphereColor;
            Gizmos.DrawSphere(transform.position, goalRadius);
        }
    }
}