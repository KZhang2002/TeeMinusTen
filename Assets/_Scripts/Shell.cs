using System;
using UnityEngine;

namespace _Scripts {
    public class Shell : MonoBehaviour {
        public float launchImpulse { get; private set; } = 20;
        private Rigidbody _rb;
        private GameManager _gm;
        private MortarController _mc;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
        }
        
        private void Start() {
            _gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
            _mc = _gm.mortar;
            
            PrepForLoad();
            _mc.LoadShell(this);
        }

        private void Update() {
            
        }

        public void PrepForLoad() {
            _rb.useGravity = false;
        }
        
        public void Fire() {
            _rb.useGravity = true;
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