using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts;
using UnityEngine;
using UnityEngine.Serialization;

public class debugUI : MonoBehaviour {
    private GameManager _gm;
    private MortarController _mc;
    private Shell _shell;
    private Rigidbody _shellRb;
    private Transform shellTf => _shell.transform;

    public GUIStyle debugGuiStyle = new();
    [SerializeField] private float leftPadding = 10f;
    [SerializeField] private float upPadding = 10f;
    [SerializeField] private float verticalSpacing = 36f; // todo change to depend on guiStyle.lineHeight

    private void Start() {
        _gm = GameManager.instance;
        _mc = _gm.mortar;
        // _shell = _mc.currentShell;
        // if (_shell) _shellRb = _shell.GetComponent<Rigidbody>();
    }
    
    private void OnGUI() {
        List<string> debugStrings = new List<string>();
        if (_shell) {
            debugStrings.Add($"Shell Position: {shellTf.position}");
            debugStrings.Add($"Shell Rotation: {shellTf.eulerAngles}");
            debugStrings.Add($"Shell Velocity: {_shellRb.velocity}");
            debugStrings.Add($"Shell Speed: {_shellRb.velocity.magnitude}");
            debugStrings.Add($"Mortar firing angle: {_mc.firingAngle}");
            debugStrings.Add($"Mortar rotation angle: {_mc.rotationAngle}");
            
        }
        else {
            _shell = _mc.currentShell;
            _shellRb = _shell.GetComponent<Rigidbody>();
        }

        if (!debugStrings.Any()) {
            debugStrings.Add($"ERROR - References not found.");
        }

        for (int i = 0; i < debugStrings.Count; i++) {
            GUI.Label(
                new Rect(leftPadding, upPadding + verticalSpacing * i, 200, 50), 
                debugStrings[i], 
                debugGuiStyle
                );
        }
    }
}