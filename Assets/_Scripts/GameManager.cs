using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts {
    public class GameManager : MonoBehaviour {
        public static GameManager instance { get; private set; }
        
        // public MortarController mortar { get; private set; }
        public MortarController mortar;
        public InputManager input { get; private set; }

        private int _shellIDCounter = 0;
        private int _goalZoneIDCounter = 0;

        private Dictionary<int, Shell> _shells = new();
        private Dictionary<int, GoalZone> _goalZones = new();
        
        private void Awake() {
            if (instance != null && instance != this)
                Destroy(this);
            else
                instance = this;

            mortar = GameObject.FindWithTag("Mortar").GetComponent<MortarController>(); // todo replace with reference?
            input = GetComponent<InputManager>();
        }

        public void RegisterShell(Shell shell) {
            shell.id = _shellIDCounter++;
            _shells[shell.id] = shell;
            Debug.Log($"shell {shell.id} registered");
        }
        
        public void RegisterGoalZone(GoalZone gz) {
            gz.id = _goalZoneIDCounter++;
            _goalZones[gz.id] = gz;
            Debug.Log($"goal {gz.id} registered");
        }

        private void Start() {
            
        }

        // enum GameState {
        //     
        // }
        // Start is called before the first frame update

        // Update is called once per frame
        private void Update() {
        }
    }
}