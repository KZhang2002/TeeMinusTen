using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts {
    public enum zoneType {
        Extract,
        Target,
    }
    
    public class Zone : MonoBehaviour {
        [SerializeField] public float goalRadius = 0.5f;
        [SerializeField] public float goalHeight = 0.5f;
        [SerializeField] public Mesh mesh;
        protected CapsuleCollider Col;
        protected GameManager Gm;
        [SerializeField] public bool isCompleted = false;
        public zoneType type = zoneType.Target;
        
        [SerializeField] protected Color gizmoColor = Color.blue;
        [SerializeField] public bool isOpen = true; // determines if zone is interactable or not
    
        public int id = -1;
        private Vector3 colCenterLocal => Vector3.up * (goalHeight / 2 - goalRadius);

        private void Awake() {
            Col = GetComponent<CapsuleCollider>();
            Col.radius = goalRadius;
            Col.height = goalHeight;
            Col.center = colCenterLocal;
        }

        private void Start() {
            Gm = GameManager.instance;
            Gm.RegisterZone(this);
            if (type == zoneType.Extract) {
                isOpen = false;
            }
        }

        private void OnEnable() {
            ZoneEvent.OnAllTargetsCompleted += HandleTargetsCompleted;
        }
        
        private void OnDisable() {
            ZoneEvent.OnAllTargetsCompleted -= HandleTargetsCompleted;
        }

        private void HandleTargetsCompleted() {
            OpenExtract();
        }

        // private void OnTriggerEnter(Collider other) {
        //     if (isCompleted) return;
        //     
        //     GameObject obj = other.gameObject;
        //     bool isShell = obj.CompareTag("Shell");
        //     if (!isShell) return;
        //     
        //     Gm.SetCurrentZone(id);
        // }
        //
        // private void OnTriggerExit(Collider other) {
        //     if (isCompleted) return;
        //     
        //     GameObject obj = other.gameObject;
        //     bool isShell = obj.CompareTag("Shell");
        //     if (!isShell) return;
        //     
        //     Gm.SetCurrentZone(-1);
        // }

        public void CompleteGoal() {
            isCompleted = true;
            Debug.Log($"completed goal. ID: {id}");
            
            if (type == zoneType.Extract && isOpen) {
                isCompleted = true;
                Gm.Extract();
            }
        }
        
        public void OpenExtract() {
            if (type != zoneType.Extract) return;
            isOpen = true;
        }
    
        void OnDrawGizmos() {
            Color finalColor = gizmoColor;

            if (isCompleted || !isOpen) finalColor = Color.white;
        
            finalColor.a = 0.5f; // make transparent
            Gizmos.color = finalColor;
            Vector3 origin = transform.position;
            
            float goalDiameter = goalRadius * 2;
            Gizmos.DrawMesh(mesh, colCenterLocal + origin, quaternion.identity, new Vector3(goalDiameter, goalHeight/2 - goalRadius, goalDiameter));
        }
    }
}