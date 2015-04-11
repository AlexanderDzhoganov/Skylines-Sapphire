using System;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{

    public class DebugRenderer : MonoBehaviour
    {

        public bool drawDebugInfo = false;

        private GUIStyle normalRectStyle;
        private GUIStyle hoveredRectStyle;
        private GUIStyle infoWindowStyle;

        private UIComponent hoveredComponent;

        void OnGUI()
        {
            if (!drawDebugInfo)
            {
                return;
            }

            if (normalRectStyle == null)
            {
                normalRectStyle = new GUIStyle(GUI.skin.box);
                var bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color(1.0f, 0.0f, 1.0f, 0.1f));
                bgTexture.Apply();
                normalRectStyle.normal.background = bgTexture;
                normalRectStyle.hover.background = bgTexture;
                normalRectStyle.active.background = bgTexture;
                normalRectStyle.focused.background = bgTexture;

                hoveredRectStyle = new GUIStyle(GUI.skin.box);
                bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color(0.0f, 1.0f, 0.0f, 0.3f));
                bgTexture.Apply();
                hoveredRectStyle.normal.background = bgTexture;
                hoveredRectStyle.hover.background = bgTexture;
                hoveredRectStyle.active.background = bgTexture;
                hoveredRectStyle.focused.background = bgTexture;

                infoWindowStyle = new GUIStyle(GUI.skin.box);
                infoWindowStyle.normal.background = null;
                infoWindowStyle.hover.background = null;
                infoWindowStyle.active.background = null;
                infoWindowStyle.focused.background = null;
            }

            var uiView = FindObjectOfType<UIView>();

            if (uiView == null)
            {
                return;
            }

            UIComponent[] components = GetComponentsInChildren<UIComponent>();
            Array.Sort(components, RenderSortFunc);

            var mouse = Input.mousePosition;
            mouse.y = Screen.height - mouse.y;

            hoveredComponent = null;
            for (int i = components.Length - 1; i > 0; i--)
            {
                var component = components[i];
              
                if (!component.isVisible)
                {
                    continue;
                }

                if (component.name == "FullScreenContainer")
                {
                    continue;
                }

                var position = component.absolutePosition;
                var size = component.size;
                var rect = CalculateRealComponentRect(position, size);

                if (rect.Contains(mouse))
                {
                    hoveredComponent = component;
                    break;
                }
            }

            foreach (var component in components)
            {
                if (!component.isVisible)
                {
                    continue;
                }

                var position = component.absolutePosition;
                var size = component.size;
                var rect = CalculateRealComponentRect(position, size);

                GUI.Box(rect, "", hoveredComponent == component ? hoveredRectStyle : normalRectStyle);
            }

            if (hoveredComponent != null)
            {
                var coords = mouse;

                var size = new Vector2(300.0f, 180.0f);

                if (coords.x + size.x >= Screen.width)
                {
                    coords.x = Screen.width - size.x;
                }

                if (coords.y + size.y >= Screen.height)
                {
                    coords.y = Screen.height - size.y;
                }

                GUI.Window(81871, new Rect(coords.x, coords.y, size.x, size.y), DoInfoWindow, "", infoWindowStyle);
            }
        }

        Rect CalculateRealComponentRect(Vector3 absolutePosition, Vector2 size)
        {
            var dx = Screen.width / 1920.0f;
            var dy = Screen.height / 1080.0f;
            absolutePosition.x *= dx;
            absolutePosition.y *= dy;
            size.x *= dx;
            size.y *= dy;
            return new Rect(absolutePosition.x, absolutePosition.y, size.x, size.y);
        }

        void DoInfoWindow(int i)
        {
            GUILayout.Label(String.Format("name: {0}", hoveredComponent.name));
            GUILayout.Label(String.Format("type: {0}", hoveredComponent.GetType().Name));

            if (hoveredComponent.parent != null)
            {
                GUILayout.Label(String.Format("parent: {0}", hoveredComponent.parent.name));
            }

            GUILayout.Label(String.Format("anchor: {0}", hoveredComponent.anchor));
            GUILayout.Label(String.Format("size: {0}", hoveredComponent.size));
            GUILayout.Label(String.Format("relativePosition: {0}", hoveredComponent.relativePosition));
            GUILayout.Label(String.Format("absolutePosition: {0}", hoveredComponent.relativePosition));
        }

        private int RenderSortFunc(UIComponent lhs, UIComponent rhs)
        {
            return lhs.renderOrder.CompareTo(rhs.renderOrder);
        }

    }

}
