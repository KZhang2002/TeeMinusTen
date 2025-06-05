using UnityEngine;

namespace _Scripts {
    public static class UIHelper {
        public static string RoundFloatToStr(float num) {
            var rounded = Mathf.Round(num * 10f) / 10f;
            return rounded.ToString("F1");
        }
        
        public static string IntToLetter(int num) {
            return ((char)('A' + num)).ToString();
        }
        
        
    }
}