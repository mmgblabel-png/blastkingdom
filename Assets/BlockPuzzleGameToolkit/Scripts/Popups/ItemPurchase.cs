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

using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.Services.IAP;
using BlockPuzzleGameToolkit.Scripts.Settings;
using TMPro;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class ItemPurchase : MonoBehaviour
    {
        public CustomButton BuyItemButton;
        public TextMeshProUGUI price;
        public TextMeshProUGUI count;
        public TextMeshProUGUI discountPercent;

        [HideInInspector]
        public ShopItem settingsShopItem;

        public ProductID productID;

        [SerializeField]
        public ResourceObject resource;

        private void OnEnable()
        {
            BuyItemButton?.onClick.RemoveListener(BuyCoins);
            BuyItemButton?.onClick.AddListener(BuyCoins);
            RefreshLocalizedPrice();
        }

        private void OnDisable()
        {
            BuyItemButton?.onClick.RemoveListener(BuyCoins);
        }

        public void Initialize(ShopItem shopItem)
        {
            settingsShopItem = shopItem;
            productID = shopItem.productID;

            if (count != null)
            {
                count.text = shopItem.count.ToString();
            }

#if UNITY_EDITOR
            if (price != null && !string.IsNullOrEmpty(shopItem.price))
            {
                price.text = shopItem.price;
            }
#endif

            RefreshLocalizedPrice();
        }

        private void RefreshLocalizedPrice()
        {
            if (productID != null && price != null && IAPManager.instance != null)
            {
                var priceValue = IAPManager.instance.GetProductLocalizedPrice(productID.ID);
                if (priceValue > 0.01m)
                {
                    price.text = IAPManager.instance.GetProductLocalizedPriceString(productID.ID);
                }
            }
        }

        private void BuyCoins()
        {
            if (productID != null)
            {
                GetComponentInParent<CoinsShop>().BuyCoins(productID.ID);
            }
        }
    }

    internal class NoAdsItemPurchase : ItemPurchase
    {
    }
}
