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
            
            PrepForLoad();
            _mc.LoadShell(this);
        }

        private void Update() {
            if (_isFired) {
                
            }
        }

        public void PrepForLoad() {
            _rb.useGravity = false;
            _col.enabled = false;
        }
        
        public void Fire() {
            _trailR.Clear();
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