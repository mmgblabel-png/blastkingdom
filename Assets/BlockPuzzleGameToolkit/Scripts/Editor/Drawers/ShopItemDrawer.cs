using BlockPuzzleGameToolkit.Scripts.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine.UIElements;

namespace BlockPuzzleGameToolkit.Scripts.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ShopItem))]
    public class ShopItemDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;

            var productIDProp = property.FindPropertyRelative("productID");
            var countProp = property.FindPropertyRelative("count");
            var prefabProp = property.FindPropertyRelative("prefab");
            var priceProp = property.FindPropertyRelative("price");

            var productIDField = new PropertyField(productIDProp);
            productIDField.label = "";
            productIDField.style.flexGrow = 1;
            productIDField.style.marginRight = 2;
            productIDField.tooltip = "Product ID reference";

            var countField = new PropertyField(countProp);
            countField.label = "";
            countField.style.flexGrow = 1;
            countField.style.marginRight = 2;
            countField.tooltip = "Coin count";

            var priceField = new PropertyField(priceProp);
            priceField.label = "";
            priceField.style.flexGrow = 1;
            priceField.style.marginRight = 2;
            priceField.tooltip = "Price (editor only)";

            var prefabField = new PropertyField(prefabProp);
            prefabField.label = "";
            prefabField.style.flexGrow = 1;
            prefabField.tooltip = "Custom prefab (optional)";

            if (prefabProp.objectReferenceValue == null)
            {
                var settingsProp = property.serializedObject.FindProperty("defaultPrefab");
                if (settingsProp != null && settingsProp.objectReferenceValue != null)
                {
                    prefabProp.objectReferenceValue = settingsProp.objectReferenceValue;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            prefabField.RegisterValueChangeCallback(evt =>
            {
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            });

            container.Add(productIDField);
            container.Add(countField);
            container.Add(priceField);
            container.Add(prefabField);

            return container;
        }
    }
}
