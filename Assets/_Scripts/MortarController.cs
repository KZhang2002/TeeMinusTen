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
        public Shell CurrentShell { get; private set; }
        private Rigidbody _shellRB;

        // Interaction
        public float firingAngle = 45f;
        public float rotationAngle = 0f;
        
        void Awake() {
            _pm = ProjectileManager.Instance;
        }
        
        void Update() {
            UpdatePosition();
        }

        void UpdatePosition() { 
            transform.eulerAngles = new Vector3(0, rotationAngle, 0);
            barrelObj.transform.localEulerAngles = new Vector3(0, 0, 90f - firingAngle);
        }
        
        public void LoadShell(Shell shell) {
            // if (CurrentShell) Destroy(CurrentShell);
            
            
            CurrentShell = shell;
            _shellRB = CurrentShell.GetComponent<Rigidbody>();
        }

        public void FireShell() {
            if (!CurrentShell) return;
            // _pm.FireShell(CurrentShell);
            CurrentShell.Fire();
            
            // CurrentShell = null;
        }
    }
}
