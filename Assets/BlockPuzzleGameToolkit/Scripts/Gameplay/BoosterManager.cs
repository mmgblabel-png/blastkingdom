using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.System;
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.LevelsData;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public enum EBoosterType
    {
        None,
        Hammer,
        Bomb,
        ColorClear,
        Shuffle,
        SuperBomb
    }

    public class BoosterManager : MonoBehaviour
    {
        public static BoosterManager instance;

        private LevelManager levelManager;
        private FieldManager fieldManager;
        private Canvas parentCanvas;
        private RectTransform boosterPanel;

        private EBoosterType activeBooster = EBoosterType.None;
        private GameObject boosterHintText;
        private TextMeshProUGUI hintTMP;

        // Custom Booster button states
        private Dictionary<EBoosterType, int> boosterCounts = new()
        {
            { EBoosterType.Hammer, 3 },
            { EBoosterType.Bomb, 2 },
            { EBoosterType.ColorClear, 1 },
            { EBoosterType.Shuffle, 3 },
            { EBoosterType.SuperBomb, 0 }
        };

        private Dictionary<EBoosterType, int> boosterCosts = new()
        {
            { EBoosterType.Hammer, 150 },
            { EBoosterType.Bomb, 250 },
            { EBoosterType.ColorClear, 300 },
            { EBoosterType.Shuffle, 200 }
        };

        private Dictionary<EBoosterType, Button> boosterButtons = new();
        private Dictionary<EBoosterType, TextMeshProUGUI> countTexts = new();

        private int superBombComboRequirement = 3;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void OnEnable()
        {
            StartCoroutine(InitWithDelay());
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Subscribe(OnShapePlaced);
        }

        private void OnDisable()
        {
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Unsubscribe(OnShapePlaced);
        }

        private IEnumerator InitWithDelay()
        {
            yield return new WaitForSeconds(0.1f);
            levelManager = FindObjectOfType<LevelManager>();
            fieldManager = FindObjectOfType<FieldManager>();

            if (levelManager != null)
            {
                parentCanvas = levelManager.GetComponentInParent<Canvas>();
                CreateBoosterUI();
            }
        }

        private void CreateBoosterUI()
        {
            if (levelManager == null || parentCanvas == null) return;

            // Find the canvas to attach to
            Transform canvasTransform = parentCanvas.transform;
            
            // Create a main parent panel for the Booster Bar
            GameObject panelGo = new GameObject("BoosterBar", typeof(RectTransform));
            panelGo.transform.SetParent(canvasTransform, false);
            boosterPanel = panelGo.GetComponent<RectTransform>();

            // Position at bottom of screen, slightly above the bottom boundary
            boosterPanel.anchorMin = new Vector2(0.5f, 0f);
            boosterPanel.anchorMax = new Vector2(0.5f, 0f);
            boosterPanel.pivot = new Vector2(0.5f, 0.5f);
            boosterPanel.anchoredPosition = new Vector2(0f, 150f);
            boosterPanel.sizeDelta = new Vector2(700f, 120f);

            // Add background image with rounded panel style
            Image bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.2f, 0.85f);

            // Add clean horizontal layout
            HorizontalLayoutGroup layout = panelGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Create buttons for each booster
            CreateBoosterButton(EBoosterType.Hammer, "🔨\nHammer", "150");
            CreateBoosterButton(EBoosterType.Bomb, "💣\nBomb", "250");
            CreateBoosterButton(EBoosterType.ColorClear, "🎨\nColor", "300");
            CreateBoosterButton(EBoosterType.Shuffle, "🔀\nShuffle", "200");
            CreateBoosterButton(EBoosterType.SuperBomb, "🔥\nS-Bomb", "FREE");

            // Create hint text at the top of the booster bar
            GameObject hintGo = new GameObject("BoosterHint", typeof(RectTransform));
            hintGo.transform.SetParent(canvasTransform, false);
            RectTransform hintRt = hintGo.GetComponent<RectTransform>();
            hintRt.anchorMin = new Vector2(0.5f, 0f);
            hintRt.anchorMax = new Vector2(0.5f, 0f);
            hintRt.pivot = new Vector2(0.5f, 0.5f);
            hintRt.anchoredPosition = new Vector2(0f, 230f);
            hintRt.sizeDelta = new Vector2(600f, 50f);

            Image hintBg = hintGo.AddComponent<Image>();
            hintBg.color = new Color(0f, 0f, 0f, 0.75f);

            GameObject textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(hintGo.transform, false);
            hintTMP = textGo.AddComponent<TextMeshProUGUI>();
            hintTMP.font = Resources.Load<TMP_FontAsset>("Font/FredokaOne-Regular SDF");
            hintTMP.fontSize = 24f;
            hintTMP.alignment = TextAlignmentOptions.Center;
            hintTMP.color = Color.white;
            hintTMP.text = "Select a Booster to clear the board!";

            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            boosterHintText = hintGo;
            boosterHintText.SetActive(false);

            UpdateSuperBombButton();
        }

        private void CreateBoosterButton(EBoosterType type, string label, string costText)
        {
            GameObject btnGo = new GameObject("Booster_" + type.ToString(), typeof(RectTransform));
            btnGo.transform.SetParent(boosterPanel, false);
            RectTransform rt = btnGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(110f, 100f);

            Image btnBg = btnGo.AddComponent<Image>();
            btnBg.color = new Color(0.25f, 0.35f, 0.65f, 1f);

            Button btn = btnGo.AddComponent<Button>();
            boosterButtons[type] = btn;

            // Create text label
            GameObject txtGo = new GameObject("Text", typeof(RectTransform));
            txtGo.transform.SetParent(btnGo.transform, false);
            TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.font = Resources.Load<TMP_FontAsset>("Font/FredokaOne-Regular SDF");
            tmp.fontSize = 18f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.text = label;

            RectTransform txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0f, 0.3f);
            txtRt.anchorMax = new Vector2(1f, 1f);
            txtRt.sizeDelta = Vector2.zero;

            // Create count/cost badge
            GameObject badgeGo = new GameObject("Badge", typeof(RectTransform));
            badgeGo.transform.SetParent(btnGo.transform, false);
            RectTransform badgeRt = badgeGo.GetComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0f, 0f);
            badgeRt.anchorMax = new Vector2(1f, 0.3f);
            badgeRt.sizeDelta = Vector2.zero;

            Image badgeBg = badgeGo.AddComponent<Image>();
            badgeBg.color = new Color(0.15f, 0.15f, 0.3f, 1f);

            GameObject badgeTxtGo = new GameObject("BadgeText", typeof(RectTransform));
            badgeTxtGo.transform.SetParent(badgeGo.transform, false);
            TextMeshProUGUI badgeTmp = badgeTxtGo.AddComponent<TextMeshProUGUI>();
            badgeTmp.font = Resources.Load<TMP_FontAsset>("Font/FredokaOne-Regular SDF");
            badgeTmp.fontSize = 14f;
            badgeTmp.alignment = TextAlignmentOptions.Center;
            badgeTmp.color = new Color(1f, 0.85f, 0f, 1f); // Golden color

            int count = boosterCounts[type];
            badgeTmp.text = count > 0 ? "Qty: " + count : "$" + costText;

            RectTransform badgeTxtRt = badgeTxtGo.GetComponent<RectTransform>();
            badgeTxtRt.anchorMin = Vector2.zero;
            badgeTxtRt.anchorMax = Vector2.one;
            badgeTxtRt.sizeDelta = Vector2.zero;

            countTexts[type] = badgeTmp;

            btn.onClick.AddListener(() => OnBoosterClicked(type));
        }

        private void OnBoosterClicked(EBoosterType type)
        {
            if (activeBooster == type)
            {
                // De-select
                CancelBoosterMode();
                return;
            }

            // If we have 0 count, attempt purchase with Coins
            if (boosterCounts[type] <= 0 && type != EBoosterType.SuperBomb)
            {
                int cost = boosterCosts[type];
                var coinResource = ResourceManager.instance.GetResource("Coins");
                if (coinResource != null)
                {
                    if (coinResource.GetValue() >= cost)
                    {
                        coinResource.Consume(cost);
                        boosterCounts[type] += 1;
                        UpdateBadgeText(type);
                        SoundBase.instance.PlaySound(SoundBase.instance.coins);
                        FloatingText("Purchased Booster!", boosterButtons[type].transform.position);
                    }
                    else
                    {
                        // Trigger shop popup
                        SoundBase.instance.PlaySound(SoundBase.instance.click);
                        MenuManager.instance.ShowPopup<CoinsShop>();
                        return;
                    }
                }
            }

            // Shuffle executes immediately
            if (type == EBoosterType.Shuffle)
            {
                ExecuteShuffleBooster();
                return;
            }

            // Enter targeting mode
            activeBooster = type;
            boosterHintText.SetActive(true);
            hintTMP.text = "Target a tile for " + type.ToString() + "!";
            
            // Highlight selected button
            ResetButtonColors();
            boosterButtons[type].GetComponent<Image>().color = new Color(1f, 0.65f, 0f, 1f); // Vibrant orange
            SoundBase.instance.PlaySound(SoundBase.instance.click);
        }

        private void CancelBoosterMode()
        {
            activeBooster = EBoosterType.None;
            if (boosterHintText != null) boosterHintText.SetActive(false);
            ResetButtonColors();
        }

        private void ResetButtonColors()
        {
            foreach (var kvp in boosterButtons)
            {
                kvp.Value.GetComponent<Image>().color = new Color(0.25f, 0.35f, 0.65f, 1f);
            }
            UpdateSuperBombButton();
        }

        private void UpdateBadgeText(EBoosterType type)
        {
            if (countTexts.ContainsKey(type))
            {
                int count = boosterCounts[type];
                if (type == EBoosterType.SuperBomb)
                {
                    countTexts[type].text = count > 0 ? "READY" : "LOCKED";
                }
                else
                {
                    countTexts[type].text = count > 0 ? "Qty: " + count : "$" + boosterCosts[type];
                }
            }
        }

        private void UpdateSuperBombButton()
        {
            if (boosterButtons.ContainsKey(EBoosterType.SuperBomb))
            {
                bool isReady = boosterCounts[EBoosterType.SuperBomb] > 0;
                boosterButtons[EBoosterType.SuperBomb].GetComponent<Image>().color = isReady ? new Color(1f, 0.15f, 0.15f, 1f) : new Color(0.2f, 0.2f, 0.2f, 1f);
                UpdateBadgeText(EBoosterType.SuperBomb);
            }
        }

        private void Update()
        {
            if (activeBooster == EBoosterType.None || fieldManager == null) return;

            // Handle selection tap/click on cells
            bool pressed = false;
            Vector2 screenPos = Vector2.zero;

            if (Touchscreen.current != null)
            {
                for (int i = 0; i < Touchscreen.current.touches.Count; i++)
                {
                    var touch = Touchscreen.current.touches[i];
                    if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        screenPos = touch.position.ReadValue();
                        pressed = true;
                        break;
                    }
                }
            }

            if (!pressed && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPos = Mouse.current.position.ReadValue();
                pressed = true;
            }

            if (pressed)
            {
                // Check if clicking inside booster buttons or other UI elements
                if (RectTransformUtility.RectangleContainsScreenPoint(boosterPanel, screenPos, Camera.main))
                {
                    return; // Ignore clicking booster bar panel itself
                }

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 100f);
                if (hit.collider != null && hit.collider.CompareTag("Cell"))
                {
                    Cell clickedCell = hit.collider.GetComponent<Cell>();
                    if (clickedCell != null)
                    {
                        ExecuteBooster(activeBooster, clickedCell);
                    }
                }
            }
        }

        private void ExecuteBooster(EBoosterType type, Cell targetCell)
        {
            if (fieldManager == null || targetCell == null) return;

            // Find clicked row & col
            int targetR = -1;
            int targetC = -1;
            int rows = fieldManager.cells.GetLength(0);
            int cols = fieldManager.cells.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (fieldManager.cells[r, c] == targetCell)
                    {
                        targetR = r;
                        targetC = c;
                        break;
                    }
                }
            }

            if (targetR == -1 || targetC == -1) return;

            bool success = false;

            switch (type)
            {
                case EBoosterType.Hammer:
                    if (!targetCell.IsEmpty())
                    {
                        targetCell.DestroyCell();
                        SpawnBoosterFX(targetCell.transform.position, Color.yellow);
                        success = true;
                    }
                    break;

                case EBoosterType.Bomb:
                    // Clear 3x3
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            int nr = targetR + dr;
                            int nc = targetC + dc;
                            if (nr >= 0 && nr < rows && nc >= 0 && nc < cols)
                            {
                                var cell = fieldManager.cells[nr, nc];
                                if (!cell.IsEmpty())
                                {
                                    cell.DestroyCell();
                                }
                            }
                        }
                    }
                    SpawnBoosterFX(targetCell.transform.position, Color.red, 1.5f);
                    success = true;
                    break;

                case EBoosterType.ColorClear:
                    if (!targetCell.IsEmpty() && targetCell.item != null && targetCell.item.itemTemplate != null)
                    {
                        var targetColor = targetCell.item.itemTemplate.backgroundColor;
                        for (int r = 0; r < rows; r++)
                        {
                            for (int c = 0; c < cols; c++)
                            {
                                var cell = fieldManager.cells[r, c];
                                if (!cell.IsEmpty() && cell.item != null && cell.item.itemTemplate != null && cell.item.itemTemplate.backgroundColor == targetColor)
                                {
                                    cell.DestroyCell();
                                    SpawnBoosterFX(cell.transform.position, Color.cyan);
                                }
                            }
                        }
                        success = true;
                    }
                    break;

                case EBoosterType.SuperBomb:
                    // Clear 5x5
                    for (int dr = -2; dr <= 2; dr++)
                    {
                        for (int dc = -2; dc <= 2; dc++)
                        {
                            int nr = targetR + dr;
                            int nc = targetC + dc;
                            if (nr >= 0 && nr < rows && nc >= 0 && nc < cols)
                            {
                                var cell = fieldManager.cells[nr, nc];
                                if (!cell.IsEmpty())
                                {
                                    cell.DestroyCell();
                                }
                            }
                        }
                    }
                    SpawnBoosterFX(targetCell.transform.position, Color.magenta, 2.5f);
                    success = true;
                    break;
            }

            if (success)
            {
                // Consume booster quantity
                if (boosterCounts[type] > 0)
                {
                    boosterCounts[type]--;
                }
                UpdateBadgeText(type);

                // Play satisfying effects
                if (SoundBase.instance.combo != null && SoundBase.instance.combo.Length > 0)
                {
                    SoundBase.instance.PlaySoundsRandom(SoundBase.instance.combo);
                }
                FloatingText(type.ToString() + " Active!", targetCell.transform.position);

                // Force LevelManager to evaluate checklines to trigger target collection & combos!
                EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Invoke(null);

                CancelBoosterMode();
            }
        }

        private void ExecuteShuffleBooster()
        {
            if (fieldManager == null) return;

            int rows = fieldManager.cells.GetLength(0);
            int cols = fieldManager.cells.GetLength(1);

            List<BlockPuzzleGameToolkit.Scripts.LevelsData.ItemTemplate> templates = new();
            List<Cell> filledCells = new();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var cell = fieldManager.cells[r, c];
                    if (!cell.IsEmpty() && cell.item != null && cell.item.itemTemplate != null)
                    {
                        templates.Add(cell.item.itemTemplate);
                        filledCells.Add(cell);
                    }
                }
            }

            if (filledCells.Count == 0)
            {
                FloatingText("Board Empty!", boosterPanel.position);
                return;
            }

            // Shuffle templates
            for (int i = 0; i < templates.Count; i++)
            {
                var temp = templates[i];
                int randomIndex = UnityEngine.Random.Range(i, templates.Count);
                templates[i] = templates[randomIndex];
                templates[randomIndex] = temp;
            }

            // Re-apply to cells
            for (int i = 0; i < filledCells.Count; i++)
            {
                filledCells[i].FillCell(templates[i]);
                filledCells[i].AnimateFill();
                SpawnBoosterFX(filledCells[i].transform.position, Color.green, 0.5f);
            }

            // Consume shuffle booster
            if (boosterCounts[EBoosterType.Shuffle] > 0)
            {
                boosterCounts[EBoosterType.Shuffle]--;
            }
            UpdateBadgeText(EBoosterType.Shuffle);

            if (SoundBase.instance.combo != null && SoundBase.instance.combo.Length > 0)
            {
                SoundBase.instance.PlaySoundsRandom(SoundBase.instance.combo);
            }
            FloatingText("SHUFFLE!", boosterPanel.position);
            CancelBoosterMode();
        }

        private void OnShapePlaced(Shape shape)
        {
            // Award free Super Bomb on Combo Streak
            var levelMgr = FindObjectOfType<LevelManager>();
            if (levelMgr != null)
            {
                if (levelMgr.comboCounter >= superBombComboRequirement)
                {
                    boosterCounts[EBoosterType.SuperBomb] += 1;
                    UpdateSuperBombButton();
                    FloatingText("🔥 SUPER BOMB EARNED! 🔥", levelMgr.transform.position);
                }
            }
        }

        private void SpawnBoosterFX(Vector3 position, Color fxColor, float scale = 1f)
        {
            // Spawn standard particle visual inside parent canvas
            GameObject fxGo = new GameObject("BoosterExplosion", typeof(RectTransform));
            fxGo.transform.SetParent(parentCanvas.transform, false);
            fxGo.transform.position = position;

            Image img = fxGo.AddComponent<Image>();
            img.color = fxColor;

            RectTransform rt = fxGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100f * scale, 100f * scale);

            // Pop scaling animation
            rt.localScale = Vector3.zero;
            rt.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                img.DOFade(0f, 0.25f).OnComplete(() =>
                {
                    Destroy(fxGo);
                });
            });
        }

        private void FloatingText(string text, Vector3 position)
        {
            GameObject txtGo = new GameObject("FloatingText", typeof(RectTransform));
            txtGo.transform.SetParent(parentCanvas.transform, false);
            txtGo.transform.position = position + Vector3.up * 50f;

            TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.font = Resources.Load<TMP_FontAsset>("Font/FredokaOne-Regular SDF");
            tmp.fontSize = 32f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.85f, 0f, 1f); // Vibrant gold
            tmp.text = text;

            RectTransform rt = txtGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400f, 60f);

            rt.DOMoveY(rt.position.y + 100f, 1f).SetEase(Ease.OutQuad);
            tmp.DOFade(0f, 1f).OnComplete(() =>
            {
                Destroy(txtGo);
            });
        }
    }
}
