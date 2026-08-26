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

using System.Collections;
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Popups.Reward;
using BlockPuzzleGameToolkit.Scripts.Services.Ads.AdUnits;
using BlockPuzzleGameToolkit.Scripts.System;
using BlockPuzzleGameToolkit.Scripts.System.Haptic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.GUI
{
    [RequireComponent(typeof(Animator))]
    public class CustomButton : Button
    {
        public AudioClip overrideClickSound;
        public RuntimeAnimatorController overrideAnimatorController;
        private bool isClicked;
        private readonly float cooldownTime = .5f;
        public new ButtonClickedEvent onClick;
        private new Animator animator;
        public bool isRewarded;
        public AdReference adReference;
        private RewardedButtonHandler handler;

        private static bool blockInput;

        protected override void OnEnable()
        {
            base.OnEnable();
            animator = GetComponent<Animator>();
            if (overrideAnimatorController != null)
            {
                animator.runtimeAnimatorController = overrideAnimatorController;
            }
            isClicked = false;
            if (isRewarded && !GetComponent<RewardedButtonHandler>() && Application.isPlaying)
            {
                handler = gameObject.AddComponent<RewardedButtonHandler>();
                handler.adReference = adReference;
                handler.onRewardedAdComplete = new UnityEngine.Events.UnityEvent();
                handler.onRewardedAdComplete.AddListener(ExecuteEvent);
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (blockInput || isClicked)
            {
                return;
            }
            if (isRewarded)
            {
                if (handler != null)
                {
                    handler.ShowRewardedAd();
                    return;
                }
            }
            if (transition != Transition.Animation)
            {
                Pressed();
            }

            isClicked = true;
            SoundBase.instance.PlaySound(overrideClickSound ? overrideClickSound : SoundBase.instance.click);
            HapticFeedback.TriggerHapticFeedback(HapticFeedback.HapticForce.Light);
            // Start cooldown
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(Cooldown());
            }

            base.OnPointerClick(eventData);
        }

        public void Pressed()
        {
            if (blockInput)
            {
                return;
            }
            if (isRewarded)
            {
                if (handler != null)
                {
                    handler.ShowRewardedAd();
                    return;
                }
            }

            ExecuteEvent();
        }

        private void ExecuteEvent()
        {
            onClick?.Invoke();
            base.onClick?.Invoke();
        }

        private IEnumerator Cooldown()
        {
            yield return new WaitForSeconds(cooldownTime);
            isClicked = false;
        }


        private bool IsAnimationPlaying()
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.loop || stateInfo.normalizedTime < 1;
        }

        public static void BlockInput(bool block)
        {
            blockInput = block;
        }
    }
}