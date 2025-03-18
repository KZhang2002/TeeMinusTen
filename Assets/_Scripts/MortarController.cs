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

        public void RegisterShell(Shell shell) {
            currentShell = shell;
            _shellRb = currentShell.GetComponent<Rigidbody>();
        }
        
        public void LoadShell(Shell shell) {
            // if (CurrentShell) Destroy(CurrentShell);
            RegisterShell(shell);
            ResetShell();
        }

        public void ResetShell() {
            currentShell.transform.SetParent(barrelObj.transform);
            currentShell.LoadShell(muzzlePos);
        }

        public void FireShell() {
            if (!currentShell) return;
            LoadShell(currentShell);
            
            // shellTf.rotation = barrelObj.transform.rotation;
            
            // currentShell.PointShell(barrelObj.transform.up);
            
            currentShell.transform.SetParent(null);
            currentShell.Fire();
            
            // CurrentShell = null;
        }
    }
}
