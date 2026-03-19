using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using HotToMark.Core;
using HotToMark.Scoring;

namespace HotToMark.UI
{
    /// <summary>
    /// Career mode scene selection screen.
    /// Shows a scrollable list of scenes with star ratings and lock states.
    /// Keyboard: Up/Down to navigate, Enter/Space to select, Escape to go back.
    /// </summary>
    public class CareerMenuUI : MonoBehaviour
    {
        private GameObject careerPanel;
        private Transform sceneListContainer;
        private TextMeshProUGUI starCountText;
        private int selectedIndex = 0;
        private int sceneCount = 0;
        private Image[] itemBackgrounds;

        void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            careerPanel = UIFactory.CreatePanel("CareerPanel", transform,
                new Color(0.03f, 0.03f, 0.03f, 0.94f));

            // Top bar
            var topBar = UIFactory.CreateImage("TopBar", careerPanel.transform,
                new Color(1f, 0.6f, 0, 0.9f));
            UIFactory.SetAnchors(topBar, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 8));

            // Title — 3x (36 -> 108)
            var title = UIFactory.CreateText("Title", careerPanel.transform,
                "CAREER MODE", 108, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(title, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.97f));

            // Star count — 3x (16 -> 48)
            var starObj = UIFactory.CreateText("Stars", careerPanel.transform,
                "0/36", 48, new Color(1f, 0.85f, 0.2f),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(starObj, new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.85f));
            starCountText = starObj.GetComponent<TextMeshProUGUI>();

            // Scene list area (scrollable)
            var scrollArea = new GameObject("ScrollArea");
            scrollArea.transform.SetParent(careerPanel.transform, false);
            var scrollRect = scrollArea.AddComponent<RectTransform>();
            UIFactory.SetAnchors(scrollArea, new Vector2(0.05f, 0.14f), new Vector2(0.95f, 0.76f));

            var mask = scrollArea.AddComponent<RectMask2D>();

            var content = new GameObject("Content");
            content.transform.SetParent(scrollArea.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 1600);

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 12;
            vlg.padding = new RectOffset(0, 0, 4, 4);
            vlg.childAlignment = TextAnchor.UpperCenter;

            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollArea.AddComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;

            sceneListContainer = content.transform;

            // Back button — 3x (16 -> 48)
            var backBtn = UIFactory.CreateImage("BackBtn", careerPanel.transform,
                new Color(0.3f, 0.3f, 0.3f));
            UIFactory.SetAnchors(backBtn, new Vector2(0.25f, 0.02f), new Vector2(0.75f, 0.11f));
            var btn = backBtn.AddComponent<Button>();
            btn.onClick.AddListener(() => {
                Hide();
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowMenu();
            });
            var label = UIFactory.CreateText("Label", backBtn.transform,
                "BACK", 48, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(label, new Vector2(0, 0), new Vector2(1, 1));

            // Footer hint
            var footer = UIFactory.CreateText("Footer", careerPanel.transform,
                "Arrow keys to browse, ENTER to play, ESC to go back", 24,
                new Color(0.45f, 0.45f, 0.45f),
                FontStyles.Normal, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(footer, new Vector2(0.05f, 0.115f), new Vector2(0.95f, 0.14f));

            careerPanel.SetActive(false);
        }

        void Update()
        {
            if (careerPanel == null || !careerPanel.activeSelf) return;
            if (sceneCount == 0) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)
            {
                selectedIndex = Mathf.Max(0, selectedIndex - 1);
                UpdateSelectionHighlight();
            }
            else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)
            {
                selectedIndex = Mathf.Min(sceneCount - 1, selectedIndex + 1);
                UpdateSelectionHighlight();
            }
            else if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame
                     || kb.spaceKey.wasPressedThisFrame)
            {
                var career = CareerManager.Instance;
                if (career != null && selectedIndex < career.scenes.Length
                    && career.scenes[selectedIndex].unlocked)
                {
                    Hide();
                    career.StartScene(selectedIndex);
                }
            }
            else if (kb.escapeKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame)
            {
                Hide();
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowMenu();
            }
        }

        private void UpdateSelectionHighlight()
        {
            if (itemBackgrounds == null) return;
            var career = CareerManager.Instance;
            if (career == null) return;

            for (int i = 0; i < itemBackgrounds.Length; i++)
            {
                if (itemBackgrounds[i] == null) continue;
                bool isSelected = (i == selectedIndex);
                bool unlocked = i < career.scenes.Length && career.scenes[i].unlocked;

                if (isSelected)
                    itemBackgrounds[i].color = unlocked
                        ? new Color(0.2f, 0.18f, 0.08f)
                        : new Color(0.12f, 0.10f, 0.06f);
                else
                    itemBackgrounds[i].color = unlocked
                        ? new Color(0.1f, 0.1f, 0.12f)
                        : new Color(0.06f, 0.06f, 0.06f);
            }
        }

        public void Show()
        {
            careerPanel.SetActive(true);
            selectedIndex = 0;
            RefreshSceneList();
        }

        public void Hide()
        {
            if (careerPanel != null)
                careerPanel.SetActive(false);
        }

        private void RefreshSceneList()
        {
            // Clear existing items
            foreach (Transform child in sceneListContainer)
                Destroy(child.gameObject);

            var career = CareerManager.Instance;
            if (career == null) return;

            sceneCount = career.scenes.Length;
            itemBackgrounds = new Image[sceneCount];
            starCountText.text = $"{career.TotalStars()}/{career.MaxStars()}";

            // Auto-select first unlocked scene
            selectedIndex = 0;
            for (int i = 0; i < career.scenes.Length; i++)
            {
                if (career.scenes[i].unlocked && career.scenes[i].bestStars == 0)
                {
                    selectedIndex = i;
                    break;
                }
                if (i == career.scenes.Length - 1)
                    selectedIndex = i; // last one
            }

            for (int i = 0; i < career.scenes.Length; i++)
            {
                CreateSceneItem(career.scenes[i], i);
            }

            UpdateSelectionHighlight();
        }

        private void CreateSceneItem(CareerScene scene, int index)
        {
            var item = new GameObject($"Scene_{index}");
            item.transform.SetParent(sceneListContainer, false);

            var le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 120;
            le.flexibleWidth = 1;

            Color bgColor = scene.unlocked
                ? new Color(0.1f, 0.1f, 0.12f)
                : new Color(0.06f, 0.06f, 0.06f);
            var bg = item.AddComponent<Image>();
            bg.color = bgColor;
            itemBackgrounds[index] = bg;

            // Scene number — 3x (24 -> 72)
            var numObj = UIFactory.CreateText("Num", item.transform,
                $"{index + 1}", 72, scene.unlocked
                    ? new Color(1f, 0.6f, 0) : new Color(0.3f, 0.3f, 0.3f),
                FontStyles.Bold, TextAlignmentOptions.Center);
            var numRect = numObj.GetComponent<RectTransform>();
            numRect.anchorMin = new Vector2(0, 0);
            numRect.anchorMax = new Vector2(0.12f, 1);
            numRect.offsetMin = Vector2.zero;
            numRect.offsetMax = Vector2.zero;

            // Name — 3x (16 -> 48)
            Color textColor = scene.unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f);
            var nameObj = UIFactory.CreateText("Name", item.transform,
                scene.unlocked ? scene.name : "LOCKED",
                48, textColor, FontStyles.Bold, TextAlignmentOptions.Left);
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.14f, 0.45f);
            nameRect.anchorMax = new Vector2(0.75f, 1);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Description — 3x (10 -> 30)
            var descObj = UIFactory.CreateText("Desc", item.transform,
                scene.unlocked ? scene.description : "",
                30, new Color(0.5f, 0.5f, 0.5f),
                FontStyles.Italic, TextAlignmentOptions.Left);
            var descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.14f, 0);
            descRect.anchorMax = new Vector2(0.75f, 0.45f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;

            // Stars — 3x (20 -> 60)
            string starStr = "";
            for (int s = 0; s < 3; s++)
                starStr += s < scene.bestStars ? "<color=#FFD700>*</color>" : "<color=#333>*</color>";
            var starObj = UIFactory.CreateText("Stars", item.transform,
                starStr, 60, Color.white, FontStyles.Normal, TextAlignmentOptions.Center);
            starObj.GetComponent<TextMeshProUGUI>().richText = true;
            var starRect = starObj.GetComponent<RectTransform>();
            starRect.anchorMin = new Vector2(0.78f, 0.3f);
            starRect.anchorMax = new Vector2(0.98f, 0.7f);
            starRect.offsetMin = Vector2.zero;
            starRect.offsetMax = Vector2.zero;

            // Best score — 3x (9 -> 27)
            if (scene.bestScore > 0)
            {
                var scoreObj = UIFactory.CreateText("Best", item.transform,
                    $"Best: {scene.bestScore}", 27, new Color(0.5f, 0.5f, 0.5f),
                    FontStyles.Normal, TextAlignmentOptions.Center);
                var scoreRect = scoreObj.GetComponent<RectTransform>();
                scoreRect.anchorMin = new Vector2(0.78f, 0);
                scoreRect.anchorMax = new Vector2(0.98f, 0.3f);
                scoreRect.offsetMin = Vector2.zero;
                scoreRect.offsetMax = Vector2.zero;
            }

            // Button (click to play)
            if (scene.unlocked)
            {
                var btn = item.AddComponent<Button>();
                int capturedIndex = index;
                btn.onClick.AddListener(() => {
                    Hide();
                    CareerManager.Instance.StartScene(capturedIndex);
                });
            }
        }
    }
}
