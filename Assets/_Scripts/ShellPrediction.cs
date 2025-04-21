using System;
using UnityEngine;

namespace _Scripts {
    public class ShellPrediction : MonoBehaviour {
        public static ShellPrediction Instance { get; private set; }
        private Shell _shell;
        private Rigidbody _shellRB;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
            }
            else {
                Instance = this;
            }
        }
        
    }
}