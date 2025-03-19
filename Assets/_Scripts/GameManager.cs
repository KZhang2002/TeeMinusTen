using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts {
    public enum GameState {
        LvlSelect,
        Live,
        Pause,
        Debrief
    }
    
    public class GameManager : MonoBehaviour {
        public static GameManager instance { get; private set; }
        
        public MortarController mortar;
        public InputManager input { get; private set; }

        private int _shellIDCounter = 0;
        private int _goalZoneIDCounter = 0;
        private int _goalCount = 0;
        private int _completedGoalsCounter = 0;

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

        public void RegisterGoalZone(GoalZone gz) {
            gz.id = _goalZoneIDCounter++;
            _goalZones[gz.id] = gz;
            Debug.Log($"goal {gz.id} registered");
            ++_goalCount;
        }

        public void CompleteGoal(int goalID) {
            _goalZones[goalID].isCompleted = true;
            ++_completedGoalsCounter;
            if (_goalCount > 0 && _completedGoalsCounter == _goalCount) {
                Debug.Log("Level completed");
            }
        }

        public void RegisterShell(Shell shell) {
            shell.id = _shellIDCounter++;
            _shells[shell.id] = shell;
            Debug.Log($"shell {shell.id} registered");
        }

    }
}