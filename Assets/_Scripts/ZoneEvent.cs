using System;
using UnityEngine;

namespace _Scripts {
    public class ZoneEvent : MonoBehaviour {
        public static event Action<int> OnZoneRegistered;
        
        public static void ZoneRegistered(int zoneID) {
            OnZoneRegistered?.Invoke(zoneID);
        }
        
        public static event Action OnAllTargetsCompleted;
        
        public static void AllZonesCompleted() {
            OnAllTargetsCompleted?.Invoke();
        }
        
        // regular gameplay
        // public static event Action OnShellLanded;
        // public static event Action OnShellFired;
        // public static event Action OnShellLoaded;
        //
        // public static void ShellLanded() {
        //     OnShellLanded?.Invoke();
        // }
        //
        // public static void ShellFired() {
        //     OnShellFired?.Invoke();
        // }
        //
        // public static void ShellLoaded() {
        //     OnShellLoaded?.Invoke();
        // }
    }
}