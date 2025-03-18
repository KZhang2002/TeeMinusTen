using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts {
    public class GameManager : MonoBehaviour {
        public static GameManager instance { get; private set; }
        
        public MortarController mortar { get; private set; }
        public InputManager input { get; private set; }

        private int _shellIDCounter = 0;
        private int _goalZoneIDCounter = 0;

        private Dictionary<int, Shell> _shells;
        private Dictionary<int, GoalZone> _goalZones;
        
        private void Awake() {
            if (instance != null && instance != this)
                Destroy(this);
            else
                instance = this;

            mortar = GameObject.FindWithTag("Mortar").GetComponent<MortarController>(); // todo replace with reference?
            input = GetComponent<InputManager>();
        }

        public void RegisterShell(Shell shell) {
            shell.ID = _shellIDCounter++;
            _shells[shell.ID] = shell;
        }
        
        public void RegisterGoalZone(GoalZone gz) {
            gz.ID = _goalZoneIDCounter++;
            _goalZones[gz.ID] = gz;
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