using System;
using UnityEngine;

namespace _Scripts {
    public enum shellType {
        Beacon,
        Package
    }
    
    public class Shell : MonoBehaviour {
        [SerializeField] private float _launchImpulse = 20;
        public float launchImpulse => _launchImpulse;

        public shellType type { get; private set; } = shellType.Beacon;
        public int ID = -1;
        
        private Rigidbody _rb;
        private Collider _col;
        private GameManager _gm;
        private MortarController _mc;
        private TrailRenderer _trailR;
        
        private Transform tf => transform;
        
        [SerializeField] private GameObject _geo;

        private Boolean _isFired = false;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<Collider>();
            _trailR = GetComponent<TrailRenderer>();
        }

        private void OnEnable() {
            _gm = GameManager.instance;
            _mc = _gm.mortar;
            transform.rotation = Quaternion.identity;
            
            MakeStatic();
            // _mc.LoadShell(this);
            _mc.RegisterShell(this);
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

        public void LoadShell(Vector3 newPos) {
            MakeStatic();
            
            tf.position = newPos;
            tf.localRotation = Quaternion.identity;
            PointShell(Vector3.zero);
        }

        public void PointShell(Vector3 dir) {
            if (dir.sqrMagnitude > 0f) {
                var headingChange = Quaternion.FromToRotation(_geo.transform.up, dir);
                _geo.transform.rotation *= headingChange;
            }
        }

        private void OnCollisionEnter(Collision other) {
            if (other.gameObject.CompareTag("Terrain")) {
                MakeStatic();
            }
        }

        public void MakeStatic() {
            _isFired = false;
            _rb.useGravity = false;
            _col.enabled = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
        }

        private void PrepForFire() {
            _trailR.Clear();
            _trailR.enabled = true;
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _col.enabled = true;
            _isFired = true;
            _rb.velocity = Vector3.zero;
        }

        public void Fire() {
            Fire(launchImpulse);
        }

        public void Fire(float impulseVal) {
            PrepForFire();
            _rb.AddForce(transform.up * impulseVal, ForceMode.Impulse);
        }

        // private void OnCollisionEnter(Collision col) {
        //     GameObject obj = col.gameObject;
        //     if (!obj.CompareTag("Terrain")) return;
        //     if ()
        // }
    }
}