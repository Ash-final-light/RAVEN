using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIHealth
{
    /// <summary>
    /// Display-only driver for the player's ultimate energy bar.
    /// </summary>
    public class PlayerUltimateEnergyBarDriver : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TMP_Text energyText;
        [SerializeField] private GameObject fullGlow;
        [SerializeField] private GameObject ultimateButtonRoot;

        [Header("Display")]
        [SerializeField] private bool forceFilledImage = true;
        [SerializeField] private bool stretchFillRectToTrack = true;
        [SerializeField] private bool showNumberText = false;

        private int currentEnergy;
        private int maxEnergy = 1;

        public int CurrentEnergy => currentEnergy;
        public int MaxEnergy => maxEnergy;
        public float Ratio => maxEnergy > 0 ? (float)currentEnergy / maxEnergy : 0f;
        public bool IsFull => maxEnergy > 0 && currentEnergy >= maxEnergy;

        private void Reset()
        {
            AutoBindUi();
        }

        private void Awake()
        {
            AutoBindUi();
            ApplyView();
        }

        private void OnValidate()
        {
            AutoBindUi();
            maxEnergy = Mathf.Max(1, maxEnergy);
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            ApplyView();
        }

        public void SetEnergy(int current, int max)
        {
            maxEnergy = Mathf.Max(1, max);
            currentEnergy = Mathf.Clamp(current, 0, maxEnergy);
            ApplyView();
        }

        private void ApplyView()
        {
            if (fillImage != null)
            {
                if (stretchFillRectToTrack)
                {
                    StretchFillRectToParent(fillImage.rectTransform);
                }

                if (forceFilledImage)
                {
                    fillImage.type = Image.Type.Filled;
                    fillImage.fillMethod = Image.FillMethod.Horizontal;
                    fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                }

                fillImage.fillAmount = Ratio;
            }

            if (energyText != null)
            {
                energyText.gameObject.SetActive(showNumberText);
                energyText.text = $"{currentEnergy}/{maxEnergy}";
            }

            if (fullGlow != null)
            {
                fullGlow.SetActive(IsFull);
            }

            if (ultimateButtonRoot != null)
            {
                ultimateButtonRoot.SetActive(IsFull);
            }
        }

        private static void StretchFillRectToParent(RectTransform fillRect)
        {
            if (fillRect == null || fillRect.parent == null)
            {
                return;
            }

            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = Vector2.zero;
        }

        private void AutoBindUi()
        {
            if (fillImage == null)
            {
                Transform fill = FindChildRecursive(transform, "EnergyBarFill_Ratio_0_70");
                if (fill != null)
                {
                    fillImage = fill.GetComponent<Image>();
                }
            }

            if (fullGlow == null)
            {
                Transform glow = FindChildRecursive(transform, "EnergyFullGlow_Disabled");
                if (glow != null)
                {
                    fullGlow = glow.gameObject;
                }
            }

            if (ultimateButtonRoot == null)
            {
                ultimateButtonRoot = FindSceneObject("Canvas_CombatOperationHUD/CombatOperationPanel/RightActionCluster/UltimateButton_L");
            }
        }

        private static GameObject FindSceneObject(string path)
        {
            GameObject activeObject = GameObject.Find(path);
            if (activeObject != null)
            {
                return activeObject;
            }

            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null || candidate.hideFlags != HideFlags.None || !candidate.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (GetHierarchyPath(candidate) == path)
                {
                    return candidate.gameObject;
                }
            }

            return null;
        }

        private static string GetHierarchyPath(Transform target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            string path = target.name;
            Transform parent = target.parent;
            while (parent != null)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }

            return path;
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                Transform nested = FindChildRecursive(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }
    }
}
