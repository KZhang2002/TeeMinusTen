using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts {
    public class LevelManager : MonoBehaviour {
        public void ReloadLevel() {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}