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

using UnityEngine;
using UnityEditor;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using System;

namespace BlockPuzzleGameToolkit.Scripts.Editor
{
    public class AssetVersionChecker : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var guids = AssetDatabase.FindAssets("t:ItemTemplate");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var itemTemplate = AssetDatabase.LoadAssetAtPath<ItemTemplate>(assetPath);
                if (itemTemplate != null && itemTemplate.HasCustomPrefab())
                {
                    EditorUtility.DisplayDialog(
                        "Warning",
                        $"ItemTemplate '{itemTemplate.name}' uses custom prefab. Please replace sprites instead of using custom prefab.",
                        "OK"
                    );
                    Selection.activeObject = itemTemplate;
                }
            }
        }
    }
}
