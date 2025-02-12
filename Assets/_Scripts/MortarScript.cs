using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts {
    public class MortarScript : MonoBehaviour {
        [SerializeField] private GameObject muzzlePosObj;
        private Vector3 muzzlePos => muzzlePosObj.transform.position;
        [SerializeField] private GameObject barrelObj;
        private Vector3 barrelPos => barrelObj.transform.position;

        public float firingAngle;
        public float rotationAngle;
        
        void Start() {
            
        }

        // Update is called once per frame
        void Update() {
            transform.localEulerAngles = new Vector3(0, rotationAngle, 0);
            barrelObj.transform.localEulerAngles = new Vector3(0, 0, 90f-firingAngle);
        }
    }
}
