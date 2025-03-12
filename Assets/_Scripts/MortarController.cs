using UnityEngine;
using UnityEngine.Serialization;
using VInspector.Libs;

namespace _Scripts {
    public class MortarController : MonoBehaviour {
        // References
        [SerializeField] private GameObject muzzlePosObj;
        private Vector3 muzzlePos => muzzlePosObj.transform.position;
        [SerializeField] private GameObject barrelObj;
        private Vector3 barrelPos => barrelObj.transform.position;
        private ProjectileManager _pm;
        
        public Shell currentShell { get; private set; }
        private Transform shellTf => currentShell.transform;
        private Rigidbody _shellRb;

        // Interaction
        public float firingAngle = -45f;
        public float rotationAngle = 0f;
        
        void Awake() {
            _pm = ProjectileManager.Instance;
        }
        
        void FixedUpdate() {
            UpdatePosition();
        }

        void UpdatePosition() { 
            transform.eulerAngles = new Vector3(0, rotationAngle, 0);
            barrelObj.transform.localEulerAngles = new Vector3(0, 0, 90f - firingAngle);
        }

        public void ChangeFiringAngle(float n) {
            firingAngle += n;
        }
        
        public void ChangeRotationAngle(float n) {
            rotationAngle += n;
        }
        
        public void LoadShell(Shell shell) {
            // if (CurrentShell) Destroy(CurrentShell);
            
            Transform shellTr = currentShell.transform;
            currentShell = shell;
            _shellRb = currentShell.GetComponent<Rigidbody>();
            currentShell.transform.position = muzzlePos;
            
            currentShell.transform.SetParent(barrelObj.transform);
        }

        public void FireShell() {
            if (!currentShell) return;
            // _pm.FireShell(CurrentShell);
            _shellRb.velocity = Vector3.zero;
            _shellRb.angularVelocity = Vector3.zero;
            shellTf.localRotation = Quaternion.identity;
            shellTf.position = muzzlePos;
            currentShell.Fire();
            
            // CurrentShell = null;
        }
    }
}
