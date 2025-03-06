using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts {
    public class InputManager : MonoBehaviour {
        private PlayerControls _controls;

        private GameManager _gm;
        private MortarController _mc;

        private void Awake() {
            _controls = new PlayerControls();
        }

        private void Start() {
            _gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
            _mc = _gm.mortar;
        }

        private void OnEnable() {
            _controls.Enable();
            _controls.Standard.Fire.performed += OnFire;
        }

        private void OnDisable() {
            _controls.Disable();
        }

        private void OnFire(InputAction.CallbackContext context) {
            // Vector2 move = context.ReadValue<Vector2>();
            // Debug.Log($"Moving: {move}");
            _mc.FireShell();
        }
    }
}