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
        // public InputManager input { get; private set; }

        private int _shellIDCounter = 0;
        private int _zoneIDCounter = 0;
        private int _targetCount = 0;
        private int _completedTargetsCounter = 0;
        private bool _reachedExtract = false;
        
        private Zone _extractZone;

        private Dictionary<int, Shell> _shells = new();
        private Dictionary<int, Zone> _zones = new();

        private void Awake() {
            if (instance != null && instance != this)
                Destroy(this);
            else
                instance = this;

            mortar = GameObject.FindWithTag("Mortar").GetComponent<MortarController>(); // todo replace with reference?
            // input = GetComponent<InputManager>();
        }

        public void RegisterZone(Zone zone) {
            if (zone.type == zoneType.Extract) {
                if (_extractZone) {
                    Debug.LogWarning("Encountered extra extract zone. Please use only one extract zone per level.");
                }
                else {
                    _extractZone = zone;
                }
                
                return;
            }
            
            zone.id = _zoneIDCounter++;
            _zones[zone.id] = zone;
            
            Debug.Log($"goal {zone.id} registered");
            // unnecessary type check
            if (zone.type == zoneType.Target) {
                ++_targetCount;
            }
        }
        
        public void TriggerZone(int zoneID) {
            Zone zone = _zones[zoneID];
            if (zone.type == zoneType.Extract) {
                _reachedExtract = true;
                return;
            }
            ++_completedTargetsCounter;
            if (_targetCount > 0 && _completedTargetsCounter == _targetCount) {
                Debug.Log("Level completed");
            }
        }

        public void CompleteGoal(int goalID) {
            // _zones[goalID].IsCompleted = true;
            ++_completedTargetsCounter;
            if (_targetCount > 0 && _completedTargetsCounter == _targetCount) {
                Debug.Log("All goals completed. Go to extract.");
                _extractZone.OpenExtract();
            }
        }

        public void RegisterShell(Shell shell) {
            shell.id = _shellIDCounter++;
            _shells[shell.id] = shell;
            Debug.Log($"shell {shell.id} registered");
        }

        public void Extract() {
            //todo add code for ending level here.
            Debug.Log("End level.");
        }

        private void TeleportMortar(Vector3 position) {
            
        }

    }
}