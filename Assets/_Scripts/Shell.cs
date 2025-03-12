using System;
using UnityEngine;

namespace _Scripts {
    public class Shell : MonoBehaviour {
        [SerializeField] private float _launchImpulse = 20;
        public float launchImpulse => _launchImpulse;
        
        private Rigidbody _rb;
        private Collider _col;
        private GameManager _gm;
        private MortarController _mc;
        private TrailRenderer _trailR;
        [SerializeField] private GameObject _geo;

        private Boolean _isFired = false;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<Collider>();
            _trailR = GetComponent<TrailRenderer>();
        }
        
        private void Start() {
            _gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
            _mc = _gm.mortar;
            transform.rotation = Quaternion.identity;
            
            PrepForLoad();
            _mc.LoadShell(this);
        }

        private void FixedUpdate() {
            Vector3 velocity = _rb.velocity;
            // if (_isFired) {
            //     PointShell(velocity);
            // }
        }

        public void PointShell(Vector3 dir) {
            if (dir.sqrMagnitude > 0f) {
                transform.rotation = Quaternion.LookRotation(dir, -transform.right);
            }
        }

        private void OnCollisionEnter(Collision other) {
            if (other.gameObject.CompareTag("Terrain")) {
                _isFired = false;
                _rb.useGravity = false;
                _col.enabled = false;
                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }
        }

        public void PrepForLoad() {
            _rb.useGravity = false;
            _col.enabled = false;
        }
        
        public void Fire() {
            _trailR.Clear();
            _rb.isKinematic = false;  
            _rb.useGravity = true;
            _col.enabled = true;
            _isFired = true;
            Fire(launchImpulse);
        }

        public void Fire(float impulseVal) {
            _rb.velocity = Vector3.zero;
            _rb.AddForce(transform.up * impulseVal, ForceMode.Impulse);
        }

        // private void OnCollisionEnter(Collision col) {
        //     GameObject obj = col.gameObject;
        //     if (!obj.CompareTag("Terrain")) return;
        //     if ()
        // }
    }
}