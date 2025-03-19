using System;
using UnityEngine;

namespace _Scripts {
    public class GoalZone : MonoBehaviour {
        [SerializeField] private float goalRadius = 0.5f;
        private SphereCollider _col;
        private GameManager _gm;
        public Boolean isCompleted = false;
    
        public int id = -1;

        private void Awake() {
            _col = GetComponent<SphereCollider>();
            _col.radius = goalRadius;
        }

        private void Start() {
            _gm = GameManager.instance;
            _gm.RegisterGoalZone(this);
        }
    
        private void OnTriggerEnter(Collider other) {
            GameObject obj = other.gameObject;
            Debug.Log("goal zone hit by obj: " + obj.name);
            bool isShell = obj.CompareTag("Shell");
            if (!isShell) return;

            isCompleted = true;
        }

        // Update is called once per frame
        void Update() {
        
        }
    
        void OnDrawGizmos() {
            Color sphereColor;
            if (isCompleted) sphereColor = Color.green;
            else sphereColor = Color.blue;
        
            sphereColor.a = 0.5f; // make transparent
            Gizmos.color = sphereColor;
            Gizmos.DrawSphere(transform.position, goalRadius);
        }
    }
}
