using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts {
    
    // angles are 30, 40, 45, 50, 60, 70, 80
    public static class ShellRangeData {
        public static readonly Dictionary<impulseType, string> RangeTable = new() {
            { impulseType.Weak, "1\n2\n3\n4\n5\n6\n7" },
            { impulseType.Medium, "360\n368\n357\n338\n279\n197\n102" },
            { impulseType.M49A2, "1095\n1058\n1005\n933\n748\n520\n266" }
        };
    }
    
    public enum shellType {
        Beacon,
        Package
    }

    public enum impulseType {
        Weak,
        Medium,
        M49A2
    }

    public class Shell : MonoBehaviour {
        [SerializeField] private float _launchImpulse = 20;
        [SerializeField] private impulseType _impulseType = impulseType.Weak;
        public int id = -1;

        [SerializeField] private GameObject geo;
        public bool isGrounded;
        private Collider _col;
        private GameManager _gm;

        private bool _isFired;
        private MortarController _mc;

        private Rigidbody _rb;
        private TrailRenderer _trailR;
        public float launchImpulse => _launchImpulse;
        public impulseType impulseType => _impulseType;

        public shellType type { get; private set; } = shellType.Beacon;

        private Transform tf => transform;
        private Transform geoTr => geo.transform;

        private UIManager _uiManager;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<Collider>();
            _trailR = GetComponent<TrailRenderer>();
        }

        private void Start() {
            _gm = GameManager.instance;
            _mc = _gm.mortar;
            transform.rotation = Quaternion.identity;
            _uiManager = UIManager.instance;

            MakeStatic();
            _mc.LoadShell(this);
            _gm.RegisterShell(this);
        }

        private void OnCollisionEnter(Collision other) {
            var obj = other.gameObject;
            if (!obj.CompareTag("Terrain")) return;
            if (obj.CompareTag("KillBarrier")) {
                _mc.LoadShell(this);
            }
            MakeStatic();
            isGrounded = true;
            ShellEvent.ShellLanded();
        }

        public void LoadShell(Vector3 newPos, Vector3 dir) {
            MakeStatic();

            tf.position = newPos;
            PointShell(dir);
            tf.rotation = Quaternion.identity;

            ShellEvent.ShellLoaded();
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
            ShellEvent.ShellFired();
        }
        
        void OnDrawGizmos() {
            Color sphereColor = Color.magenta;
            sphereColor.a = 0.5f; // make transparent
            Gizmos.color = sphereColor;
            Gizmos.DrawSphere(transform.position, 1.0f);
        }
    }
}