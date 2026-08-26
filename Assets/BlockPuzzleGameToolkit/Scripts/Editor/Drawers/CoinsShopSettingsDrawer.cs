using BlockPuzzleGameToolkit.Scripts.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlockPuzzleGameToolkit.Scripts.Editor.Drawers
{
    [CustomEditor(typeof(CoinsShopSettings))]
    public class CoinsShopSettingsDrawer : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            
            // Draw default properties
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            
            // Add space
            var spacer = new VisualElement();
            spacer.style.height = 10;
            root.Add(spacer);
            
            // Add help box
            var helpBox = new HelpBox(
                "Note: Prices are only for testing purposes in the editor. Real prices will be taken from the store for builds.",
                HelpBoxMessageType.Info);
            
            root.Add(helpBox);
            
            return root;
        }
    }
}