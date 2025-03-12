using System;
using UnityEngine;

namespace _Scripts {
    public class GameManager : MonoBehaviour {
        public static GameManager instance { get; private set; }
        
        public MortarController mortar { get; private set; }
        public InputManager input { get; private set; }
        
        private void Awake() {
            if (instance != null && instance != this)
                Destroy(this);
            else
                instance = this;

            mortar = GameObject.FindWithTag("Mortar").GetComponent<MortarController>(); // todo replace with reference?
            input = GetComponent<InputManager>();
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