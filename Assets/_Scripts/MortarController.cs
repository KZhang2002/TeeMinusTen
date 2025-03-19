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
        
        // Direction barrel is pointed towards in world space
        private Vector3 barrelDir => barrelObj.transform.rotation * Vector3.up;
        
        
        public Shell currentShell { get; private set; }
        private Transform shellTf => currentShell.transform;
        private Rigidbody _shellRb;

        // Interaction
        public float firingAngle = -45f;
        public float rotationAngle = 0f;
        
        void FixedUpdate() {
            UpdateAngles();
        }

        void UpdateAngles() { 
            transform.eulerAngles = new Vector3(0, rotationAngle, 0);
            barrelObj.transform.localEulerAngles = new Vector3(0, 0, 90f - firingAngle);
        }

        public void ChangeFiringAngle(float n) {
            firingAngle += n;
        }
        
        public void ChangeRotationAngle(float n) {
            rotationAngle += n;
        }

        public void RegisterShellRef(Shell shell) {
            currentShell = shell;
            _shellRb = currentShell.GetComponent<Rigidbody>();
        }
        
        public void LoadShell(Shell shell) {
            // if (CurrentShell) Destroy(CurrentShell);
            RegisterShellRef(shell);
            ResetShell();
        }

        public void ResetShell() {
            barrelObj.transform.rotation = Quaternion.identity;
            shellTf.SetParent(barrelObj.transform, true);
            currentShell.LoadShell(muzzlePos, barrelDir);
        }

        public void FireShell() {
            if (!currentShell) return;
            shellTf.position = muzzlePos;
            shellTf.rotation = Quaternion.identity;
            shellTf.SetParent(null, true);
            currentShell.Fire(barrelDir);
            
            // CurrentShell = null;
        }
    }
}
