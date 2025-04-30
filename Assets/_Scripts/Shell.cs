using System;
using UnityEngine;

namespace _Scripts {
    public enum shellType {
        Beacon,
        Package
    }

    public class Shell : MonoBehaviour {
        [SerializeField] private float _launchImpulse = 20;
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

        public shellType type { get; private set; } = shellType.Beacon;

        private Transform tf => transform;
        private Transform geoTr => geo.transform;

        private TerminalUIEvents _ui;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<Collider>();
            _trailR = GetComponent<TrailRenderer>();
            Time.timeScale = 2.0f;
        }

        private void Start() {
            _gm = GameManager.instance;
            _mc = _gm.mortar;
            transform.rotation = Quaternion.identity;
            _ui = TerminalUIEvents.instance;

            MakeStatic();
            _mc.LoadShell(this);
            // _mc.RegisterShellRef(this);
            _gm.RegisterShell(this);
        }

        private void LateUpdate() {
            if (_isFired) {
                var velocity = _rb.velocity;
                // PointShell(velocity.normalized);
            }
        }

        private void OnEnable() {
            ShellEvent.OnShellLanded += HandleShellLanded;
            ShellEvent.OnShellFired += HandleShellFired;
            ShellEvent.OnShellLoaded += HandleShellLoaded;
        }

        private void OnDisable() {
            ShellEvent.OnShellLanded -= HandleShellLanded;
            ShellEvent.OnShellFired -= HandleShellFired;
            ShellEvent.OnShellLoaded -= HandleShellLoaded;
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
            _ui.HideShellIcon();
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
            ShellEvent.ShellFired();
        }

        public void Fire(float impulseVal, Vector3 dir) {
            MakeDynamic();
            _rb.AddForce(dir * impulseVal, ForceMode.Impulse);
            _ui.ShowShellIcon();
        }

        private void HandleShellLanded() {
            Debug.Log("Shell has landed.");
        }

        private void HandleShellFired() {
            Debug.Log("Shell has been fired.");
        }

        private void HandleShellLoaded() {
            _trailR.Clear();
        }
    }
}