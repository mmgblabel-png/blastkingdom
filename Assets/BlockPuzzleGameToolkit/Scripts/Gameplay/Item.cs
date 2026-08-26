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

using BlockPuzzleGameToolkit.Scripts.LevelsData;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Item : FillAndPreview
    {
        public ItemTemplate itemTemplate;
        public Image backgroundColor;
        public Image underlayColor;
        public Image bottomColor;
        public Image topColor;
        public Image leftColor;
        public Image rightColor;
        public Image overlayColor;
        private Vector2Int position;
        public Bonus bonus;
        public BonusItemTemplate bonusItemTemplate;

        private void Awake()
        {
            bonus?.gameObject.SetActive(false);
            if (itemTemplate != null)
            {
                UpdateColor(itemTemplate);
            }
        }

        public void UpdateColor(ItemTemplate itemTemplate)
        {
            if(itemTemplate.HasCustomPrefab())
                return;
            this.itemTemplate = itemTemplate;
            backgroundColor.color = itemTemplate.backgroundColor;
            underlayColor.color = itemTemplate.underlayColor;
            bottomColor.color = itemTemplate.bottomColor;
            topColor.color = itemTemplate.topColor;
            leftColor.color = itemTemplate.leftColor;
            rightColor.color = itemTemplate.rightColor;
            overlayColor.color = itemTemplate.overlayColor;

            UpdateEnableColors(itemTemplate);

            backgroundColor.sprite = itemTemplate.backgroundSprite;
            underlayColor.sprite = itemTemplate.underlaySprite;
            bottomColor.sprite = itemTemplate.bottomSprite;
            topColor.sprite = itemTemplate.topSprite;
            leftColor.sprite = itemTemplate.leftSprite;
            rightColor.sprite = itemTemplate.rightSprite;
            overlayColor.sprite = itemTemplate.overlaySprite;
        }

        private void UpdateEnableColors(ItemTemplate itemTemplate)
        {
            backgroundColor.color = new Color(backgroundColor.color.r, backgroundColor.color.g, backgroundColor.color.b, itemTemplate.colorEnable[0] ? 1f : 0f);
            underlayColor.color = new Color(underlayColor.color.r, underlayColor.color.g, underlayColor.color.b, itemTemplate.colorEnable[1] ? 1f : 0f);
            bottomColor.color = new Color(bottomColor.color.r, bottomColor.color.g, bottomColor.color.b, itemTemplate.colorEnable[2] ? 1f : 0f);
            topColor.color = new Color(topColor.color.r, topColor.color.g, topColor.color.b, itemTemplate.colorEnable[3] ? 1f : 0f);
            leftColor.color = new Color(leftColor.color.r, leftColor.color.g, leftColor.color.b, itemTemplate.colorEnable[4] ? 1f : 0f);
            rightColor.color = new Color(rightColor.color.r, rightColor.color.g, rightColor.color.b, itemTemplate.colorEnable[5] ? 1f : 0f);
            overlayColor.color = new Color(overlayColor.color.r, overlayColor.color.g, overlayColor.color.b, itemTemplate.colorEnable[6] ? 1f : 0f);
        }

        public void SetBonus(BonusItemTemplate template)
        {
            bonusItemTemplate = template;
            bonus.gameObject.SetActive(true);
            bonus.FillIcon(template);
        }

        public override void FillIcon(ScriptableData iconScriptable)
        {
            UpdateColor((ItemTemplate)iconScriptable);
        }

        public void SetPosition(Vector2Int vector2Int)
        {
            position = vector2Int;
        }

        public Vector2Int GetPosition()
        {
            return position;
        }

        public bool HasBonusItem()
        {
            return bonusItemTemplate != null;
        }

        public void ClearBonus()
        {
            bonusItemTemplate = null;
            bonus.gameObject.SetActive(false);
        }

        public void SetTransparency(float alpha)
        {
            UpdateEnableColors(itemTemplate);

            var color = backgroundColor.color;
            color.a = alpha;
            if(backgroundColor.color.a != 0f)
                backgroundColor.color = color;

            color = underlayColor.color;
            color.a = alpha;
            if(underlayColor.color.a != 0f)
                underlayColor.color = color;

            color = bottomColor.color;
            color.a = alpha;
            if(bottomColor.color.a != 0f)
                bottomColor.color = color;

            color = topColor.color;
            color.a = alpha;
            if(topColor.color.a != 0f)
                topColor.color = color;

            color = leftColor.color;
            color.a = alpha;
            if(leftColor.color.a != 0f)
                leftColor.color = color;

            color = rightColor.color;
            color.a = alpha;
            if(rightColor.color.a != 0f)
                rightColor.color = color;

            color = overlayColor.color;
            color.a = alpha;
            if(overlayColor.color.a != 0f)
                overlayColor.color = color;

            bonus?.SetTransparency(alpha);
        }
    }
}