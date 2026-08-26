using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.Popups;
using System.Linq;
using System.Collections.Generic;

namespace BlockPuzzleGameToolkit.Scripts.Editor
{
    [CustomPropertyDrawer(typeof(PopupEventAdsSetting))]
    public class PopupEventAdsSettingDrawer : PropertyDrawer
    {
        private Popup[] popupPrefabs;
        private List<string> popupNames;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            LoadPopupPrefabs();

            var container = new VisualElement();
            
            var popupProperty = property.FindPropertyRelative("popup");
            var showOnOpenProperty = property.FindPropertyRelative("showOnOpen");
            var showOnCloseProperty = property.FindPropertyRelative("showOnClose");

            // Popup dropdown
            var popupDropdown = new DropdownField("Popup", popupNames, GetPopupIndex(popupProperty.objectReferenceValue as Popup));
            popupDropdown.RegisterValueChangedCallback(evt =>
            {
                int selectedIndex = popupNames.IndexOf(evt.newValue);
                if (selectedIndex == 0)
                {
                    popupProperty.objectReferenceValue = null;
                }
                else if (selectedIndex > 0)
                {
                    popupProperty.objectReferenceValue = popupPrefabs[selectedIndex - 1];
                }
                popupProperty.serializedObject.ApplyModifiedProperties();
            });
            container.Add(popupDropdown);

            // Show options
            var showOnOpenField = new PropertyField(showOnOpenProperty);
            container.Add(showOnOpenField);

            var showOnCloseField = new PropertyField(showOnCloseProperty);
            container.Add(showOnCloseField);

            return container;
        }

        private void LoadPopupPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            var popups = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
                .Where(go => go != null && go.GetComponent<Popup>() != null)
                .Select(go => go.GetComponent<Popup>())
                .OrderBy(popup => popup.name)
                .ToArray();

            popupPrefabs = popups;
            popupNames = new List<string> { "None (Popup)" };
            popupNames.AddRange(popups.Select(popup => popup.name));
        }

        private int GetPopupIndex(Popup popup)
        {
            if (popup == null) return 0;
            
            for (int i = 0; i < popupPrefabs.Length; i++)
            {
                if (popupPrefabs[i] == popup)
                    return i + 1;
            }
            return 0;
        }
    }
}