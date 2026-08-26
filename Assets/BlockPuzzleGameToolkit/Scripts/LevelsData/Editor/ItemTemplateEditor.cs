// // ©2015 - 2026 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using System.IO;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData.Editor
{
    [CustomEditor(typeof(ItemTemplate))]
    [CanEditMultipleObjects]
    public class ItemTemplateEditor : UnityEditor.Editor
    {
        private ItemTemplate[] itemTemplates;
        private int currentIndex;
        private ItemTemplate _target;

        private void OnEnable()
        {
            _target = (ItemTemplate)target;
            LoadAvailableTemplates();
            FindCurrentIndex();
        }

        private void LoadAvailableTemplates()
        {
            itemTemplates = Resources.LoadAll<ItemTemplate>("");
            Array.Sort(itemTemplates, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        }

        private void FindCurrentIndex()
        {
            for (var i = 0; i < itemTemplates.Length; i++)
            {
                if (itemTemplates[i] == _target)
                {
                    currentIndex = i;
                    break;
                }
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Create navigation container with arrows
            var navContainer = new VisualElement();
            navContainer.style.flexDirection = FlexDirection.Row;
            navContainer.style.marginBottom = 10;
            navContainer.style.justifyContent = Justify.Center;

            var prevButton = new Button(() => NavigateTemplates(-1)) { text = "<<" };
            prevButton.style.width = 40;
            
            var label = new Label($"{currentIndex + 1}/{itemTemplates.Length}");
            label.style.paddingLeft = 10;
            label.style.paddingRight = 10;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            var nextButton = new Button(() => NavigateTemplates(1)) { text = ">>" };
            nextButton.style.width = 40;
            
            var addButton = new Button(AddTemplate) { text = "+" };
            addButton.style.width = 30;
            
            var removeButton = new Button(RemoveTemplate) { text = "-" };
            removeButton.style.width = 30;

            navContainer.Add(prevButton);
            navContainer.Add(label);
            navContainer.Add(nextButton);
            navContainer.Add(addButton);
            navContainer.Add(removeButton);

            root.Add(navContainer);
            // Add the prefab field that will use the IconDrawer
            var prefabProperty = serializedObject.FindProperty("prefab");
            if (prefabProperty != null)
            {
                var iconDrawer = new BlockPuzzleGameToolkit.Scripts.Editor.Drawers.IconDrawer();
                var prefabField = iconDrawer.CreatePropertyGUI(prefabProperty);
                root.Add(prefabField);
                
                // Add ScriptableInspector buttons if prefab is assigned
                var prefab = prefabProperty.objectReferenceValue as Item;
                if (prefab != null)
                {
                    var buttonContainer = new VisualElement();
                    buttonContainer.style.flexDirection = FlexDirection.Row;
                    buttonContainer.style.marginTop = 5;
                    buttonContainer.style.marginBottom = 10;
                    
                    var editPrefabButton = new Button(() => {
                        if (prefabProperty.objectReferenceValue != null)
                        {
                            AssetDatabase.OpenAsset(prefabProperty.objectReferenceValue);
                        }
                        else
                        {
                            Debug.LogError("Prefab not found!");
                        }
                    }) { text = "Edit Prefab" };
                    editPrefabButton.style.flexGrow = 1;
                    editPrefabButton.style.marginRight = 5;
                    

                    buttonContainer.Add(editPrefabButton);
                    root.Add(buttonContainer);
                }
            }
            // Add the custom item prefab field at the top
            var customPrefabContainer = new VisualElement();
            customPrefabContainer.style.flexDirection = FlexDirection.Row;
            

            // Add color-sprite pairs
            var colorProperties = new[] {
                ("backgroundColor", "backgroundSprite"),
                ("underlayColor", "underlaySprite"),
                ("bottomColor", "bottomSprite"),
                ("topColor", "topSprite"),
                ("leftColor", "leftSprite"),
                ("rightColor", "rightSprite"),
                ("overlayColor", "overlaySprite")
            };

            for (var index = 0; index < colorProperties.Length; index++)
            {
                var (colorProp, spriteProp) = colorProperties[index];
                var pairContainer = new VisualElement();
                pairContainer.style.flexDirection = FlexDirection.Row;
                pairContainer.style.marginBottom = 5;
                pairContainer.style.marginTop = 5;

                var l = new Label(ObjectNames.NicifyVariableName(spriteProp));
                l.style.width = 150;
                l.style.unityTextAlign = TextAnchor.MiddleLeft;
                pairContainer.Add(l);

                var spriteField = new PropertyField(serializedObject.FindProperty(spriteProp));
                spriteField.style.flexGrow = 1;
                spriteField.label = "";
                spriteField.style.marginRight = 5;
                spriteField.Bind(serializedObject);

                var colorContainer = new VisualElement();
                colorContainer.style.flexDirection = FlexDirection.Row;
                colorContainer.style.flexGrow = 1;

                var colorField = new PropertyField(serializedObject.FindProperty(colorProp));
                colorField.style.flexGrow = 1;
                colorField.label = "";
                colorField.Bind(serializedObject);

                var toggleContainer = new VisualElement();
                toggleContainer.style.width = 20;
                toggleContainer.style.marginLeft = 5;
                var enableProperty = serializedObject.FindProperty("colorEnable").GetArrayElementAtIndex(index);
                var toggle = new PropertyField(enableProperty) { label = "" };
                toggle.Bind(serializedObject);

                toggleContainer.Add(toggle);

                colorContainer.Add(colorField);

                pairContainer.Add(toggleContainer);
                pairContainer.Add(spriteField);
                pairContainer.Add(colorContainer);
                root.Add(pairContainer);

            }

            var customItemPrefab = serializedObject.FindProperty("customItemPrefab");
            if (customItemPrefab != null && customItemPrefab.objectReferenceValue != null)
            {
                var customPrefabField = new PropertyField(customItemPrefab);
                customPrefabField.style.flexGrow = 1;
                customPrefabField.Bind(serializedObject);

                var createButton = new Button(() => CreateCustomPrefab(_target)) { text = "Create Prefab" };
                createButton.style.width = 100;

                customPrefabContainer.Add(customPrefabField);
                // customPrefabContainer.Add(createButton);
                root.Add(customPrefabContainer);
            }

            return root;
        }

        private void CreateCustomPrefab(ItemTemplate itemTemplate)
        {
            // Load the base item prefab
            string basePrefabPath = "Assets/BlockPuzzleGameToolkit/Prefabs/Game/Item.prefab";
            var baseItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
            if (baseItemPrefab == null)
            {
                Debug.LogError("Could not find base Item prefab at: " + basePrefabPath);
                return;
            }

            // Create the prefab
            string prefabPath = "Assets/BlockPuzzleGameToolkit/Resources/CustomItems";

            // Ensure directory exists
            if (!Directory.Exists(prefabPath))
            {
                Directory.CreateDirectory(prefabPath);
            }

            // Create unique name
            string prefabName = "CustomItem";
            int counter = 0;
            while (File.Exists($"{prefabPath}/{prefabName}{(counter == 0 ? "" : counter.ToString())}.prefab"))
            {
                counter++;
            }
            prefabName = $"{prefabName}{(counter == 0 ? "" : counter.ToString())}";

            // Clone the prefab
            string fullPath = $"{prefabPath}/{prefabName}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(PrefabUtility.InstantiatePrefab(baseItemPrefab) as GameObject, fullPath);
            prefab.GetComponent<Item>().itemTemplate = itemTemplate;
            // Assign the prefab to our customItemPrefab field
            serializedObject.Update();
            var customItemPrefabProperty = serializedObject.FindProperty("customItemPrefab");
            customItemPrefabProperty.objectReferenceValue = prefab.GetComponent<Gameplay.Item>();
            serializedObject.ApplyModifiedProperties();

            // Ping the prefab in project window
            EditorGUIUtility.PingObject(prefab);
        }

        private void NavigateTemplates(int direction)
        {
            if (itemTemplates.Length <= 1)
                return;

            currentIndex += direction;
            
            // Wrap around
            if (currentIndex < 0)
                currentIndex = itemTemplates.Length - 1;
            else if (currentIndex >= itemTemplates.Length)
                currentIndex = 0;

            // Select the new template
            Selection.activeObject = itemTemplates[currentIndex];
        }

        private void AddTemplate()
        {
            // Find a unique name for the new template
            string newName = "ItemTemplate New";
            int counter = 0;
            while (AssetDatabase.FindAssets($"t:ItemTemplate {newName}{(counter == 0 ? "" : " " + counter)}").Length > 0)
            {
                counter++;
            }
            newName = $"{newName}{(counter == 0 ? "" : " " + counter)}";

            // Create new template by duplicating the current one
            string path = AssetDatabase.GetAssetPath(_target);
            string directory = Path.GetDirectoryName(path);
            string newPath = $"{directory}/{newName}.asset";

            AssetDatabase.CopyAsset(path, newPath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the new template
            var newTemplate = AssetDatabase.LoadAssetAtPath<ItemTemplate>(newPath);
            Selection.activeObject = newTemplate;
        }

        private void RemoveTemplate()
        {
            if (itemTemplates.Length <= 1)
            {
                EditorUtility.DisplayDialog("Cannot Remove", "Cannot remove the last item template.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Remove Template", 
                $"Are you sure you want to remove '{_target.name}'?", "Remove", "Cancel"))
            {
                string path = AssetDatabase.GetAssetPath(_target);
                int newIndex = currentIndex > 0 ? currentIndex - 1 : 0;
                
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                // Reload available templates and select the new one
                LoadAvailableTemplates();
                Selection.activeObject = itemTemplates[newIndex];
            }
        }
    }
}

