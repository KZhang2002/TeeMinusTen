using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace _Scripts {
    public class TerminalUIEvents : MonoBehaviour {
        private GameManager _gm;
        private MortarController _mc;
        private Shell _shell;
        private Rigidbody _shellRb;
        private Transform shellTf => _shell.transform;
        
        private UIDocument _doc;
        private Label _firingAngle;
        private string _fAPrefix;
        private Label _mortarRotation;
        private string _mRPrefix;
        private Label _shellSpeed;
        private string _sSPrefix;
        private Label _shellDistance;
        private string _sDPrefix;
        
        private float _timer = 0f;
        public float updateInterval = 1f;

        private void Start() {
            _doc = GetComponent<UIDocument>();
            
            _firingAngle = _doc.rootVisualElement.Q("firingAngle") as Label;
            _mortarRotation = _doc.rootVisualElement.Q("mortarRotation") as Label;
            _shellSpeed = _doc.rootVisualElement.Q("shellSpeed") as Label;
            _shellDistance = _doc.rootVisualElement.Q("shellDistance") as Label;
            
            Debug.Log($"fa text: {_firingAngle}");

            _fAPrefix = _firingAngle.text;
            _mRPrefix = _mortarRotation.text;
            _sSPrefix = _shellSpeed.text;
            _sDPrefix = _shellDistance.text;
            
            Debug.Log($"fa text: {_fAPrefix}");
            
            _gm = GameManager.instance;
            _mc = _gm.mortar;
            _shell = _mc.currentShell;
            _shellRb = _shell.GetComponent<Rigidbody>();

            _timer = updateInterval;
        }

        private float Round(float num) {
            return Mathf.Round(num * 10f) / 10f;
        }

        private void UpdateDataText() {
            _firingAngle.text = _fAPrefix + $" {Round(_mc.firingAngle)}°";
            _mortarRotation.text = _mRPrefix + $" {Round(_mc.rotationAngle)}°";
            
            float dist = Vector3.Distance(_mc.gameObject.transform.position, shellTf.position);
            if (dist < 5f) dist = 0;
            _shellDistance.text = 
                _sDPrefix + $" {Round(dist)} METERS";
            
            _shellSpeed.text = _sSPrefix + $" {Round(_shellRb.velocity.magnitude)} M/S";
        }

        private void Update() {
            _timer += Time.deltaTime;
            if (_timer >= updateInterval) {
                if (!_shell) _shell = _mc.currentShell;
                if (!_shellRb) _shellRb = _shell.GetComponent<Rigidbody>();
                
                UpdateDataText();
                _timer = 0f;
            }
            // Debug.Log($"fa text: {_firingAngle.text}");
        }

        // debugStrings.Add($"Shell Position: {shellTf.position}");
        // debugStrings.Add($"Shell Rotation: {shellTf.eulerAngles}");
        // debugStrings.Add($"Shell Velocity: {_shellRb.velocity}");
        // debugStrings.Add($"Shell Speed: {_shellRb.velocity.magnitude}");
        // debugStrings.Add($"Mortar firing angle: {Mathf.Round(_mc.firingAngle * 10f) / 10f}° ");
        // debugStrings.Add($"Mortar rotation angle: {Mathf.Round(_mc.rotationAngle * 10f) / 10f}");
        // debugStrings.Add($"Shell Distance: {Vector3.Distance(_mc.gameObject.transform.position, _shell.transform.position)}m");
        
        
    }
}