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
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_PURCHASING
#endif

namespace BlockPuzzleGameToolkit.Scripts.Services.IAP
{
    public class IAPManager : MonoBehaviour
    {
        public static IAPManager instance;
        private const float InitializationTimeoutSeconds = 20f;

        private IIAPService iapController;

        public async Task InitializePurchasing(IEnumerable<(string productId, ProductTypeWrapper.ProductType productType)> products)
        {
            #if UNITY_PURCHASING
            iapController = new IAPController();
            iapController.InitializePurchasing(products);
            var initializationStartedAt = Time.realtimeSinceStartup;
            while (!iapController.IsInitialized())
            {
                if (iapController is IAPController controller && controller.HasInitializationFailed)
                {
                    Debug.LogError("IAP is unavailable: " + controller.InitializationError);
                    return;
                }

                if (Time.realtimeSinceStartup - initializationStartedAt >= InitializationTimeoutSeconds)
                {
                    Debug.LogError("IAP initialization timed out. Purchase buttons must remain unavailable until a later retry succeeds.");
                    return;
                }

                await Task.Delay(100);
            }
            #else
            iapController = new DummyIAPService();
            #endif
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void SubscribeToPurchaseEvent(Action<string> purchaseHandler)
        {
            #if UNITY_PURCHASING
            IAPController.OnSuccessfulPurchase += purchaseHandler;
            #endif
        }

        public static void UnsubscribeFromPurchaseEvent(Action<string> purchaseHandler)
        {
            #if UNITY_PURCHASING
            IAPController.OnSuccessfulPurchase -= purchaseHandler;
            #endif
        }

        public void BuyProduct(string productId)
        {
            if (iapController == null || !iapController.IsInitialized())
            {
                Debug.LogError("Cannot start purchase because Unity IAP is not initialized.");
                return;
            }

            iapController.BuyProduct(productId);
        }

        public decimal GetProductLocalizedPrice(string productId)
        {
            return iapController?.GetProductLocalizedPrice(productId) ?? 0m;
        }

        public string GetProductLocalizedPriceString(string productId)
        {
            return iapController?.GetProductLocalizedPriceString(productId) ?? string.Empty;
        }

        public bool IsProductPurchased(string productId)
        {
            #if UNITY_PURCHASING
            if (iapController is IAPController controller)
            {
                return controller.IsProductPurchased(productId);
            }
            #endif
            return false;
        }

        public void RestorePurchases(Action<bool, List<string>> action)
        {
#if UNITY_PURCHASING
            if (iapController is IAPController controller)
            {
                controller.Restore(action);
            }
#endif
        }
    }

    public class DummyIAPService : IIAPService
    {
        public void InitializePurchasing(IEnumerable<(string productId, ProductTypeWrapper.ProductType productType)> products)
        {
        }

        public void BuyProduct(string productId)
        {
        }

        public decimal GetProductLocalizedPrice(string productId)
        {
            return 0m;
        }

        public string GetProductLocalizedPriceString(string productId)
        {
            return string.Empty;
        }

        public bool IsInitialized()
        {
            return true;
        }
    }
}
