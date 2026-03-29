using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;
using Synthborn.Core.Skills;

namespace Synthborn.UI
{
    /// <summary>
    /// Skill tree UI: 4 branches displayed as columns, nodes as buttons.
    /// Unlocked = colored, locked but available = outlined, locked = dark.
    /// </summary>
    public class SkillTreeScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _treeContainer;
        [SerializeField] private Text _pointsText;
        [SerializeField] private Text _infoText;
        [SerializeField] private Button _resetButton;
        [SerializeField] private SkillTreeData _treeData;
        [SerializeField] private Font _font;
        [SerializeField] private int _resetCost = 50;

        private void Awake()
        {
            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnReset);
        }

        private void OnDestroy()
        {
            if (_resetButton != null)
                _resetButton.onClick.RemoveListener(OnReset);
        }

        public void Show()
        {
            if (_panel == null) return;
            if (_treeData != null) SkillTreeManager.SetTreeData(_treeData);
            _panel.SetActive(true);
            Refresh();
            PopupEscHandler.Register(_panel, Hide);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            PopupEscHandler.Unregister();
        }

        private void Refresh()
        {
            if (_treeContainer == null) return;
            foreach (Transform child in _treeContainer) Destroy(child.gameObject);

            var ch = SaveManager.Character;
            if (_pointsText != null)
                _pointsText.text = $"Skill Points: {ch?.unspentStatPoints ?? 0}";

            if (_infoText != null)
                _infoText.text = $"Unlocked: {SkillTreeManager.UnlockedCount}/{SkillTreeManager.TotalNodes}";

            // Create 4 branch columns
            var branches = new[] { SkillBranch.Might, SkillBranch.Vitality, SkillBranch.Agility, SkillBranch.Fortune };
            var branchColors = new[] {
                new Color(0.9f, 0.3f, 0.2f), // Might - red
                new Color(0.3f, 0.9f, 0.3f), // Vitality - green
                new Color(0.3f, 0.5f, 0.9f), // Agility - blue
                new Color(0.9f, 0.85f, 0.2f) // Fortune - gold
            };
            var branchNames = new[] { "MIGHT", "VITALITY", "AGILITY", "FORTUNE" };

            for (int b = 0; b < branches.Length; b++)
                CreateBranchColumn(branches[b], branchNames[b], branchColors[b], b, branches.Length);
        }

        private void CreateBranchColumn(SkillBranch branch, string name, Color color, int colIndex, int totalCols)
        {
            float colWidth = 1f / totalCols;
            float xMin = colIndex * colWidth + 0.01f;
            float xMax = (colIndex + 1) * colWidth - 0.01f;

            // Branch container
            var colGO = new GameObject($"Branch_{branch}");
            colGO.transform.SetParent(_treeContainer, false);
            var rect = colGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(xMin, 0f);
            rect.anchorMax = new Vector2(xMax, 1f);
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Branch header
            var headerGO = new GameObject("Header", typeof(RectTransform), typeof(Text));
            headerGO.transform.SetParent(colGO.transform, false);
            var hRect = headerGO.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0, 0.92f); hRect.anchorMax = new Vector2(1, 1f);
            hRect.sizeDelta = Vector2.zero; hRect.offsetMin = Vector2.zero; hRect.offsetMax = Vector2.zero;
            var hText = headerGO.GetComponent<Text>();
            hText.text = name; hText.fontSize = 14; hText.color = color;
            hText.alignment = TextAnchor.MiddleCenter; hText.font = _font; hText.raycastTarget = false;

            // Nodes
            var nodes = _treeData.GetBranch(branch);
            for (int i = 0; i < nodes.Length; i++)
            {
                float yMax = 0.90f - i * 0.09f;
                float yMin = yMax - 0.08f;
                CreateNodeButton(colGO.transform, nodes[i], color, yMin, yMax);
            }
        }

        private void CreateNodeButton(Transform parent, SkillNodeData node, Color branchColor, float yMin, float yMax)
        {
            bool unlocked = SkillTreeManager.IsUnlocked(node.id);
            bool canUnlock = SkillTreeManager.CanUnlock(node.id);

            var go = new GameObject(node.id, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, yMin);
            rect.anchorMax = new Vector2(0.9f, yMax);
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = go.GetComponent<Image>();
            if (unlocked)
                img.color = new Color(branchColor.r * 0.4f, branchColor.g * 0.4f, branchColor.b * 0.4f, 0.9f);
            else if (canUnlock)
                img.color = new Color(0.2f, 0.18f, 0.25f, 0.9f);
            else
                img.color = new Color(0.08f, 0.08f, 0.10f, 0.8f);

            // Node text
            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero; tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;
            tRect.offsetMin = new Vector2(4, 1); tRect.offsetMax = new Vector2(-4, -1);
            var text = textGO.GetComponent<Text>();
            text.text = unlocked
                ? $"<color=#{ColorUtility.ToHtmlStringRGB(branchColor)}>\u2713</color> {node.displayName}"
                : node.displayName;
            text.fontSize = 10;
            text.color = unlocked ? Color.white : (canUnlock ? new Color(0.8f, 0.8f, 0.8f) : Color.gray);
            text.alignment = TextAnchor.MiddleCenter;
            text.font = _font;
            text.supportRichText = true;
            text.raycastTarget = false;

            // Button interaction
            var btn = go.GetComponent<Button>();
            btn.interactable = canUnlock;
            if (canUnlock)
            {
                string capturedId = node.id;
                btn.onClick.AddListener(() => OnNodeClicked(capturedId));
            }

            // Hover effect
            if (go.GetComponent<ButtonHoverEffect>() == null)
                go.AddComponent<ButtonHoverEffect>();
        }

        private void OnNodeClicked(string nodeId)
        {
            if (SkillTreeManager.TryUnlock(nodeId))
                Refresh();
        }

        private void OnReset()
        {
            if (SkillTreeManager.TryReset(_resetCost))
                Refresh();
        }
    }
}
