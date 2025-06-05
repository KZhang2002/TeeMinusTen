using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace _Scripts {
    public class Cursor : MonoBehaviour
    {
        public UIDocument uiDocument;

        private VisualElement cursor;

        void OnEnable()
        {
            var root = uiDocument.rootVisualElement;
            cursor = root.Q<VisualElement>("cursor");
            
            UnityEngine.Cursor.visible = false; // hide cursor
        }

        void Update()
        {
            Vector2 mousePos = Input.mousePosition;

            // Convert to UI Toolkit coordinates (bottom-left origin)
            var panel = uiDocument.rootVisualElement.panel;
            if (panel == null) return;
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(
                uiDocument.rootVisualElement.panel, mousePos);

            cursor.style.left = panelPos.x;
            cursor.style.top = panelPos.y;
        }
    }
}