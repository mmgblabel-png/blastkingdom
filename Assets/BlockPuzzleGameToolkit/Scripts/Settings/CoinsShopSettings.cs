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
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.Services.IAP;
using BlockPuzzleGameToolkit.Scripts.Utils;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Settings
{
    [Serializable]
    public class ShopItem
    {
        public ProductID productID;
        public int count;
        public ItemPurchase prefab;
        public string price;
    }

    public class CoinsShopSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        public ItemPurchase defaultPrefab;

        [SerializeField, HideInInspector]
        private SerializableDictionary<ProductID, int> coinsProducts = new();

        [SerializeField]
        public List<ShopItem> products = new();

        [SerializeField, HideInInspector]
        private bool migrated = false;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (migrated)
                return;

            if (coinsProducts != null && coinsProducts.Count > 0)
            {
                products = new List<ShopItem>();

                foreach (KeyValuePair<ProductID, int> kvp in coinsProducts)
                {
                    products.Add(new ShopItem
                    {
                        productID = kvp.Key,
                        count = kvp.Value,
                        prefab = defaultPrefab
                    });
                }

                migrated = true;
            }
        }
    }
}