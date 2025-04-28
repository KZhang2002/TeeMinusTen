using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts {
    public class Marker : MonoBehaviour {
        [SerializeField] protected float goalRadius = 0.5f;
        protected SphereCollider Col;
        [SerializeField] protected Color gizmoColor = Color.magenta;
    
        public int id = -1;

        private void Awake() {
            Col = GetComponent<SphereCollider>();
            Col.radius = goalRadius;
        }
    
        void OnDrawGizmos() {
            Color sphereColor = gizmoColor;
            sphereColor.a = 0.5f; // make transparent
            Gizmos.color = sphereColor;
            Gizmos.DrawSphere(transform.position, goalRadius);
        }
    }
}