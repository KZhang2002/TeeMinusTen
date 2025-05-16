using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts {
    public class MortarController : MonoBehaviour {
        // Inspector References
        [SerializeField] private GameObject muzzlePosObj;
        private Vector3 muzzlePos => muzzlePosObj.transform.position;
        [SerializeField] private GameObject barrelObj;
        private Vector3 barrelPos => barrelObj.transform.position;
        private Vector3 barrelDir => barrelObj.transform.rotation * Vector3.up; // Direction the barrel is pointed towards in world space
        
        // Mortar Attributes
        public float minFiringAngle = 10f;
        public float maxFiringAngle = 100f;
        public float startingFiringAngle = 45f;
        public float startingRotationAngle = 0f;
        
        // Shell References
        public Shell currentShell { get; private set; }
        private Transform shellTf => currentShell.transform;
        
        // Interaction
        public float firingAngle;
        public float rotationAngle = 0f;
        
        private float _timer;
        private float updateInterval = 0.2f;

        public Light muzzleFlash;
        private bool flashOn;

        private void Awake() {
            firingAngle = startingFiringAngle;
            rotationAngle = startingRotationAngle;
            
            _timer = 0f;
            muzzleFlash.enabled = false;
            flashOn = false;
        }

        private void Update() {
            if (flashOn) _timer += Time.deltaTime;
            if (_timer >= updateInterval) {
                flashOn = false;
                muzzleFlash.enabled = false;
                _timer = 0f;
            }
        }

        private void ResetAngles() {
            ResetAngles(startingFiringAngle, startingRotationAngle);
        }

        private void ResetAngles(float fireAngle, float rotAngle) {
            firingAngle = fireAngle;
            rotationAngle = rotAngle;
        }

        private void FixedUpdate() {
            UpdateAngles();
        }

        private void UpdateAngles() {
            firingAngle = Mathf.Clamp(firingAngle, minFiringAngle, maxFiringAngle);
            transform.eulerAngles = new Vector3(0, rotationAngle, 0);
            barrelObj.transform.localEulerAngles = new Vector3(0, 0, firingAngle - 90f);
        }

        public void ChangeFiringAngle(float n) {
            firingAngle += n;
        }
        
        public void ChangeRotationAngle(float n) {
            rotationAngle += n;
            if (rotationAngle >= 360) rotationAngle -= 360;
            else if (rotationAngle < 0) rotationAngle += 360;
        }

        public void RegisterShellRef(Shell shell) {
            currentShell = shell;
        }
        
        public void LoadShell(Shell shell) {
            // if (CurrentShell) Destroy(CurrentShell);
            RegisterShellRef(shell);
            ResetShell();
        }

        public void ResetShell() {
            // barrelObj.transform.rotation = Quaternion.identity;
            shellTf.SetParent(barrelObj.transform, true);
            currentShell.LoadShell(muzzlePos, barrelDir);
        }

        public void FireShell() {
            if (!currentShell) return;
            shellTf.position = muzzlePos;
            shellTf.rotation = Quaternion.identity;
            shellTf.SetParent(null, true);
            currentShell.Fire(barrelDir);
            
            flashOn = true;
            muzzleFlash.enabled = true;
            
            // CurrentShell = null;
        }

        public void TeleportToShell() {
            if (!currentShell || !currentShell.isGrounded) return;
            
            Debug.Log("Teleporting to shell.");
            transform.position = shellTf.position;
        }
    }
}
