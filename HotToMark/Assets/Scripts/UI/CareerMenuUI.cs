using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HotToMark.Core;
using HotToMark.Scoring;

namespace HotToMark.UI
{
    /// <summary>
    /// F-3: Career mode scene selection screen.
    /// Shows a scrollable list of scenes with star ratings and lock states.
    /// </summary>
    public class CareerMenuUI : MonoBehaviour
    {
        private GameObject careerPanel;
        private Transform sceneListContainer;
        private TextMeshProUGUI starCountText;

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
                new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 6));

            // Title
            var title = UIFactory.CreateText("Title", careerPanel.transform,
                "CAREER MODE", 36, new Color(1f, 0.6f, 0),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(title, new Vector2(0.1f, 0.90f), new Vector2(0.9f, 0.97f));

            // Star count
            var starObj = UIFactory.CreateText("Stars", careerPanel.transform,
                "0/36", 16, new Color(1f, 0.85f, 0.2f),
                FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(starObj, new Vector2(0.1f, 0.86f), new Vector2(0.9f, 0.90f));
            starCountText = starObj.GetComponent<TextMeshProUGUI>();

            // Scene list area (scrollable)
            var scrollArea = new GameObject("ScrollArea");
            scrollArea.transform.SetParent(careerPanel.transform, false);
            var scrollRect = scrollArea.AddComponent<RectTransform>();
            UIFactory.SetAnchors(scrollArea, new Vector2(0.05f, 0.10f), new Vector2(0.95f, 0.85f));

            var mask = scrollArea.AddComponent<RectMask2D>();

            var content = new GameObject("Content");
            content.transform.SetParent(scrollArea.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 800);

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 8;
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

            // Back button
            var backBtn = UIFactory.CreateImage("BackBtn", careerPanel.transform,
                new Color(0.3f, 0.3f, 0.3f));
            UIFactory.SetAnchors(backBtn, new Vector2(0.3f, 0.02f), new Vector2(0.7f, 0.08f));
            var btn = backBtn.AddComponent<Button>();
            btn.onClick.AddListener(() => {
                Hide();
                if (GameManager.Instance != null)
                    GameManager.Instance.ShowMenu();
            });
            var label = UIFactory.CreateText("Label", backBtn.transform,
                "BACK", 16, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UIFactory.SetAnchors(label, new Vector2(0, 0), new Vector2(1, 1));

            careerPanel.SetActive(false);
        }

        public void Show()
        {
            careerPanel.SetActive(true);
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

            starCountText.text = $"{career.TotalStars()}/{career.MaxStars()}";

            for (int i = 0; i < career.scenes.Length; i++)
            {
                CreateSceneItem(career.scenes[i], i);
            }
        }

        private void CreateSceneItem(CareerScene scene, int index)
        {
            var item = new GameObject($"Scene_{index}");
            item.transform.SetParent(sceneListContainer, false);

            var le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 70;
            le.flexibleWidth = 1;

            Color bgColor = scene.unlocked
                ? new Color(0.1f, 0.1f, 0.12f)
                : new Color(0.06f, 0.06f, 0.06f);
            var bg = item.AddComponent<Image>();
            bg.color = bgColor;

            // Scene number
            var numObj = UIFactory.CreateText("Num", item.transform,
                $"{index + 1}", 24, scene.unlocked
                    ? new Color(1f, 0.6f, 0) : new Color(0.3f, 0.3f, 0.3f),
                FontStyles.Bold, TextAlignmentOptions.Center);
            var numRect = numObj.GetComponent<RectTransform>();
            numRect.anchorMin = new Vector2(0, 0);
            numRect.anchorMax = new Vector2(0.12f, 1);
            numRect.offsetMin = Vector2.zero;
            numRect.offsetMax = Vector2.zero;

            // Name
            Color textColor = scene.unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f);
            var nameObj = UIFactory.CreateText("Name", item.transform,
                scene.unlocked ? scene.name : "LOCKED",
                16, textColor, FontStyles.Bold, TextAlignmentOptions.Left);
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.14f, 0.5f);
            nameRect.anchorMax = new Vector2(0.75f, 1);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Description
            var descObj = UIFactory.CreateText("Desc", item.transform,
                scene.unlocked ? scene.description : "",
                10, new Color(0.5f, 0.5f, 0.5f),
                FontStyles.Italic, TextAlignmentOptions.Left);
            var descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.14f, 0);
            descRect.anchorMax = new Vector2(0.75f, 0.5f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;

            // Stars
            string starStr = "";
            for (int s = 0; s < 3; s++)
                starStr += s < scene.bestStars ? "<color=#FFD700>*</color>" : "<color=#333>*</color>";
            var starObj = UIFactory.CreateText("Stars", item.transform,
                starStr, 20, Color.white, FontStyles.Normal, TextAlignmentOptions.Center);
            starObj.GetComponent<TextMeshProUGUI>().richText = true;
            var starRect = starObj.GetComponent<RectTransform>();
            starRect.anchorMin = new Vector2(0.78f, 0.3f);
            starRect.anchorMax = new Vector2(0.98f, 0.7f);
            starRect.offsetMin = Vector2.zero;
            starRect.offsetMax = Vector2.zero;

            // Best score
            if (scene.bestScore > 0)
            {
                var scoreObj = UIFactory.CreateText("Best", item.transform,
                    $"Best: {scene.bestScore}", 9, new Color(0.5f, 0.5f, 0.5f),
                    FontStyles.Normal, TextAlignmentOptions.Center);
                var scoreRect = scoreObj.GetComponent<RectTransform>();
                scoreRect.anchorMin = new Vector2(0.78f, 0);
                scoreRect.anchorMax = new Vector2(0.98f, 0.3f);
                scoreRect.offsetMin = Vector2.zero;
                scoreRect.offsetMax = Vector2.zero;
            }

            // Button
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
