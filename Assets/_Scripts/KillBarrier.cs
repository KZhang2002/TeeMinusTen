using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBarrier : MonoBehaviour {
    private Collider _col;
    
    // Start is called before the first frame update
    void Start() {
        _col = GetComponent<Collider>();
        
    }

    private void OnCollisionEnter(Collision other) {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
