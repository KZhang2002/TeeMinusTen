using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace _Scripts {
    public class InputManager : MonoBehaviour {
        private PlayerControls _controls;

        private GameManager _gm;
        private MortarController _mc;

        private float tiltInput;
        private float rotateInput;

        private bool isSpeedMod; // speed modifier key (default: shift) is pressed

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

            _mc.ChangeFiringAngle(tiltInput);
            _mc.ChangeRotationAngle(rotateInput);
        }

        private void OnEnable() {
            _controls.Enable();
            
            _controls.Standard.Fire.performed += OnFire;
            _controls.Standard.Reload.performed += OnReload;

            _controls.Standard.TeleportDEBUG.performed += OnTeleport;
            _controls.Standard.ResetLevelDEBUG.performed += OnResetLevel;
            
            _controls.Standard.SpeedModifier.performed += _ => isSpeedMod = true;
            _controls.Standard.SpeedModifier.canceled += _ => isSpeedMod = false;
        }
        
        private void OnDisable() {
            _controls.Disable();
        }

        private void OnFire(InputAction.CallbackContext context) {
            _mc.FireShell();
        }

        private void OnTeleport(InputAction.CallbackContext context) {
            _mc.TeleportToShell();
        }

        private void OnReload(InputAction.CallbackContext context) {
            _mc.ResetShell();
        }

        private void OnResetLevel(InputAction.CallbackContext context) {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}