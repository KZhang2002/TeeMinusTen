using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts {
    public class MortarController : MonoBehaviour {
        [SerializeField] private GameObject muzzlePosObj;
        private Vector3 muzzlePos => muzzlePosObj.transform.position;
        [SerializeField] private GameObject barrelObj;
        private Vector3 barrelPos => barrelObj.transform.position;

        public float firingAngle = 45f;
        public float rotationAngle = 0f;
        
        void Start() {
            
        }

        // Update is called once per frame
        void Update() {
            transform.eulerAngles = new Vector3(0, rotationAngle, 0);
            barrelObj.transform.localEulerAngles = new Vector3(0, 0, 90f-firingAngle);
        }
    }
    
    
}
