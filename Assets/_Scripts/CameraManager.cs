using System.Collections.Generic;
using UnityEngine;

namespace _Scripts {
    public enum CameraType {
        none,
        projectile,
    }
    
    public class GameCam : MonoBehaviour {
        
    }
    
    public class CameraManager : MonoBehaviour {
        public static CameraManager Instance;
        private List<GameCam> _camList = new();
    
        private void Awake() {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;
        }

        public void RegisterCamera(GameCam cam) {
            _camList.Add(cam);
        }
    }
}
