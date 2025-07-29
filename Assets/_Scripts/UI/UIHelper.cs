using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts {
    public static class UIHelper {
        public static string RoundFloatToStr(float num) {
            var rounded = Mathf.Round(num * 10f) / 10f;
            return rounded.ToString("F1");
        }
        
        public static string IntToLetter(int num) {
            return ((char)('A' + num)).ToString();
        }
            
        public static void AssignLabel(ref Label target, string labelName, UIDocument doc) {
            target = doc.rootVisualElement.Q(labelName) as Label;
            if (target == null) Debug.LogError($"Missing Label: {labelName}");
        }
        
        public static void AssignVE(ref VisualElement target, string veName, UIDocument doc) {
            target = doc.rootVisualElement.Q(veName);
            if (target == null) Debug.LogError($"Missing Visual Element: {veName}");
        }
    }
}