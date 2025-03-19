using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts {
    public enum shellType {
        Beacon,
        Package
    }
    
    public class Shell : MonoBehaviour {
        [SerializeField] private float _launchImpulse = 20;
        public float launchImpulse => _launchImpulse;

        public shellType type { get; private set; } = shellType.Beacon;
        public int id = -1;
        
        private Rigidbody _rb;
        private Collider _col;
        private GameManager _gm;
        private MortarController _mc;
        private TrailRenderer _trailR;
        
        private Transform tf => transform;
        
        [SerializeField] private GameObject geo;
        private Transform geoTr => geo.transform;

        private Boolean _isFired = false;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<Collider>();
            _trailR = GetComponent<TrailRenderer>();
        }

        private void Start() {
            _gm = GameManager.instance;
            _mc = _gm.mortar;
            transform.rotation = Quaternion.identity;
            
            MakeStatic();
            // _mc.LoadShell(this);
            _mc.RegisterShellRef(this);
            _gm.RegisterShell(this);
        }

        private void FixedUpdate() {
            Vector3 velocity = _rb.velocity;
            if (_isFired) {
                PointShell(velocity.normalized);
            }
            
            // Debug.DrawRay(transform.position, velocity.normalized * 5f, Color.magenta);
            // Debug.Log(transform.position);
            // Debug.Log(velocity.normalized);
        }
        
        public void LoadShell(Vector3 newPos, Vector3 dir) {
            MakeStatic();
            
            tf.position = newPos;
            PointShell(dir);
            tf.rotation = Quaternion.identity;
        }

        public void LoadShell(Vector3 newPos, Quaternion dir) {
            LoadShell(newPos, dir.eulerAngles);
        }
        
        public void PointShell(Quaternion dir) {
            PointShell(dir.eulerAngles);
        }

        public void PointShell(Vector3 dir) {
            if (dir.sqrMagnitude > 0f) {
                var headingChange = Quaternion.FromToRotation(geo.transform.up, dir);
                geo.transform.rotation *= headingChange;
            }
        }
        
        private void OnCollisionEnter(Collision other) {
            GameObject obj = other.gameObject;
            if (!obj.CompareTag("Terrain")) return;
            MakeStatic();
        }

        public void MakeStatic() {
            _isFired = false;
            
            _rb.useGravity = false;
            _rb.isKinematic = true;
            
            _col.enabled = false;
            
            _trailR.emitting = false;
        }

        private void MakeDynamic() {
            _trailR.Clear();
            _trailR.emitting = true;
            
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.velocity = Vector3.zero;
            
            _col.enabled = true;
            _isFired = true;
        }

        public void Fire(Vector3 dir) {
            Fire(launchImpulse, dir);
        }

        public void Fire(float impulseVal, Vector3 dir) {
            MakeDynamic();
            _rb.AddForce(dir * impulseVal, ForceMode.Impulse);
        }
    }
}