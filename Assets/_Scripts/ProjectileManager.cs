using System;
using UnityEngine;

namespace _Scripts {
    public class ProjectileManager : MonoBehaviour {
        public static ProjectileManager Instance { get; private set; }
        private Shell _shell;

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