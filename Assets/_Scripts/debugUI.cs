using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts;
using UnityEngine;

public class debugUI : MonoBehaviour {
    private GameManager _gm;
    private MortarController _mc;
    private Shell _shell;

    private readonly GUIStyle _debugGuiStyle = new();

    void Awake() {
        _gm = GameManager.instance;
    }

    private void Start() {
        _mc = _gm.mortar;
        _shell = _mc.currentShell;
    }

    private void OnGUI() {
        _debugGuiStyle.fontSize = 12;
        _debugGuiStyle.normal.textColor = Color.black;
        float x = 10f;
        float y = 0f;

        List<string> debugStrings = new List<string>();
        if (_shell) {
            Rigidbody shellRb = _shell.GetComponent<Rigidbody>();
            debugStrings.Add($"Shell Position: {_shell.transform.position}");
            debugStrings.Add($"Shell Rotation: {_shell.transform.eulerAngles}");
            debugStrings.Add($"Shell Velocity: {shellRb.velocity}");
            debugStrings.Add($"Shell Speed: {shellRb.velocity.magnitude}");
        }
        else {
            _shell = _mc.currentShell;
        }

        for (int i = 0; i < debugStrings.Count; i++) {
            GUI.Label(new Rect(x, y + (12 * (i + 1)), 200, 50), debugStrings[i], _debugGuiStyle);
        }
    }
}