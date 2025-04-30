using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace _Scripts {
    public class InputManager : MonoBehaviour {
        private PlayerControls _controls;

        private GameManager _gm;
        private MortarController _mc;

        public float firingAngleIncrement { get; private set; } = 0.1f; // maybe move to mortar controller
        public float rotationAngleIncrement { get; private set; } = 0.1f; // ditto

        private float tiltInput = 0f;
        private float rotateInput = 0f;

        private bool isSpeedMod = false;

        private void Awake() {
            _controls = new PlayerControls();
        }

        private void Start() {
            _gm = GameManager.instance;
            _mc = _gm.mortar;
        }

        private void Update() {
            float rawTilt = _controls.Standard.Tilt.ReadValue<float>();
            float rawRotate = _controls.Standard.Rotate.ReadValue<float>();

            tiltInput = rawTilt * (isSpeedMod ? 0.1f : 1f);
            rotateInput = rawRotate * (isSpeedMod ? 0.1f : 1f);

            _mc.ChangeFiringAngle(tiltInput * firingAngleIncrement * Time.deltaTime * 100f);
            _mc.ChangeRotationAngle(rotateInput * rotationAngleIncrement * Time.deltaTime * 100f);
            
            if (Input.GetKeyDown(KeyCode.P))
            {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(currentScene.name);
            }
        }

        private void OnEnable() {
            _controls.Enable();
            _controls.Standard.Fire.performed += OnFire;
            _controls.Standard.Reset.performed += OnReset;
            
            // _controls.Standard.Tilt.performed += OnTilt;
            // _controls.Standard.Tilt.canceled += OnTilt;
            //
            // _controls.Standard.Rotate.performed += OnRotate;
            // _controls.Standard.Rotate.canceled += OnRotate;
            
            _controls.Standard.SpeedModifier.performed += _ => isSpeedMod = true;
            _controls.Standard.SpeedModifier.canceled += _ => isSpeedMod = false;
        }
        
        private void OnDisable() {
            _controls.Disable();
        }

        private void OnFire(InputAction.CallbackContext context) {
            // Vector2 move = context.ReadValue<Vector2>();
            // Debug.Log($"Moving: {move}");
            _mc.FireShell();
        }

        // private void OnTilt(InputAction.CallbackContext context) {
        //     tiltInput = context.ReadValue<float>();
        //     if (isSpeedMod) tiltInput *= 0.1f;
        // }
        //
        // private void OnRotate(InputAction.CallbackContext context) {
        //     rotateInput = context.ReadValue<float>();
        //     if (isSpeedMod) rotateInput *= 0.1f;
        // }

        private void OnReset(InputAction.CallbackContext context) {
            _mc.ResetShell();
        }
    }
}