using UnityEngine;
using UnityEngine.UI;

namespace UI.UIHealth
{
    /// <summary>
    /// Drives an HP bar with an optional shield overlay.
    /// Expected child structure:
    /// HpBarRoot/BarBack, HpBarRoot/HpFill, HpBarRoot/ShieldFill, HpBarRoot/BarFrame.
    /// </summary>
    [ExecuteAlways]
    public class SegmentedShieldHpBar : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private RectTransform hpBarRoot;
        [SerializeField] private RectTransform hpFill;
        [SerializeField] private RectTransform shieldFill;

        [Header("Initial Values")]
        [Min(1f)]
        [SerializeField] private float originalHp = 1000f;
        [Min(0f)]
        [SerializeField] private float initialHp = 1000f;
        [Min(0f)]
        [SerializeField] private float initialShield = 0f;

        [Header("Runtime")]
        [SerializeField] private bool previewInEditMode = true;
        [SerializeField] private bool animateChanges = true;
        [Min(0.01f)]
        [SerializeField] private float animationSpeed = 12f;

        [Header("Optional Visuals")]
        [SerializeField] private Image shieldImage;
        [SerializeField] private Color shieldColor = new Color(0.42f, 0.88f, 1f, 0.82f);
        [Min(0f)]
        [SerializeField] private float shieldStartOverlapPixels = 0f;

        private float currentHp;
        private float currentShield;
        private float shownHpRatio;
        private float shownShieldStartRatio;
        private float shownShieldEndRatio;
        private float targetHpRatio;
        private float targetShieldStartRatio;
        private float targetShieldEndRatio;
        private bool initialized;

        public float OriginalHp => originalHp;
        public float CurrentHp => currentHp;
        public float CurrentShield => currentShield;
        public float HpRatio => targetHpRatio;
        public float ShieldStartRatio => targetShieldStartRatio;
        public float ShieldEndRatio => targetShieldEndRatio;

        private void Reset()
        {
            hpBarRoot = transform as RectTransform;
            AutoBindChildren();
            initialHp = originalHp;
            ApplyInitialValues(true);
        }

        private void Awake()
        {
            AutoBindChildren();
            ApplyInitialValues(true);
        }

        private void OnEnable()
        {
            AutoBindChildren();
            ApplyInitialValues(true);
        }

        private void OnValidate()
        {
            originalHp = Mathf.Max(1f, originalHp);
            initialHp = Mathf.Clamp(initialHp, 0f, originalHp);
            initialShield = Mathf.Max(0f, initialShield);

            AutoBindChildren();

            if (!Application.isPlaying && previewInEditMode)
            {
                ApplyInitialValues(true);
            }
        }

        private void Update()
        {
            if (!initialized)
            {
                ApplyInitialValues(true);
            }

            if (!Application.isPlaying && !previewInEditMode)
            {
                return;
            }

            if (!animateChanges || !Application.isPlaying)
            {
                ApplyRanges(targetHpRatio, targetShieldStartRatio, targetShieldEndRatio);
                return;
            }

            float t = 1f - Mathf.Exp(-animationSpeed * Time.unscaledDeltaTime);
            shownHpRatio = Mathf.Lerp(shownHpRatio, targetHpRatio, t);
            shownShieldStartRatio = Mathf.Lerp(shownShieldStartRatio, targetShieldStartRatio, t);
            shownShieldEndRatio = Mathf.Lerp(shownShieldEndRatio, targetShieldEndRatio, t);
            ApplyRanges(shownHpRatio, shownShieldStartRatio, shownShieldEndRatio);
        }

        public void Bind(RectTransform targetHpBarRoot)
        {
            hpBarRoot = targetHpBarRoot;
            AutoBindChildren();
            ApplyCurrentValues(true);
        }

        public void SetOriginalHp(float value, bool refill)
        {
            originalHp = Mathf.Max(1f, value);

            if (refill)
            {
                currentHp = originalHp;
                currentShield = 0f;
            }
            else
            {
                currentHp = Mathf.Clamp(currentHp, 0f, originalHp);
                currentShield = Mathf.Max(0f, currentShield);
            }

            ApplyCurrentValues(false);
        }

        public void SetHp(float hp)
        {
            currentHp = Mathf.Clamp(hp, 0f, originalHp);
            ApplyCurrentValues(false);
        }

        public void SetShield(float shield)
        {
            currentShield = Mathf.Max(0f, shield);
            ApplyCurrentValues(false);
        }

        public void SetHpAndShield(float hp, float shield)
        {
            currentHp = Mathf.Clamp(hp, 0f, originalHp);
            currentShield = Mathf.Max(0f, shield);
            ApplyCurrentValues(false);
        }

        public void AddShield(float amount)
        {
            currentShield = Mathf.Max(0f, currentShield + amount);
            ApplyCurrentValues(false);
        }

        public void TakeDamage(float damage)
        {
            damage = Mathf.Max(0f, damage);

            float shieldDamage = Mathf.Min(currentShield, damage);
            currentShield -= shieldDamage;
            damage -= shieldDamage;

            currentHp = Mathf.Clamp(currentHp - damage, 0f, originalHp);
            ApplyCurrentValues(false);
        }

        public void Refill()
        {
            currentHp = originalHp;
            currentShield = 0f;
            ApplyCurrentValues(false);
        }

        private void ApplyInitialValues(bool instant)
        {
            currentHp = Mathf.Clamp(initialHp, 0f, originalHp);
            currentShield = Mathf.Max(0f, initialShield);
            ApplyCurrentValues(instant);
        }

        private void ApplyCurrentValues(bool instant)
        {
            float hpRatio = Mathf.Clamp01(currentHp / originalHp);
            float shieldEndRatio = Mathf.Clamp01((currentHp + currentShield) / originalHp);

            targetHpRatio = hpRatio;
            targetShieldStartRatio = hpRatio;
            targetShieldEndRatio = Mathf.Max(hpRatio, shieldEndRatio);

            if (instant || !Application.isPlaying || !animateChanges)
            {
                shownHpRatio = targetHpRatio;
                shownShieldStartRatio = targetShieldStartRatio;
                shownShieldEndRatio = targetShieldEndRatio;
                ApplyRanges(shownHpRatio, shownShieldStartRatio, shownShieldEndRatio);
            }

            initialized = true;
        }

        private void ApplyRanges(float hpStartToEnd, float shieldStart, float shieldEnd)
        {
            SetBarRange(hpFill, 0f, hpStartToEnd);
            SetBarRange(shieldFill, shieldStart, shieldEnd);
            ApplyShieldStartOverlap(shieldStart, shieldEnd);

            if (shieldImage != null)
            {
                shieldImage.color = shieldColor;
                shieldImage.enabled = shieldEnd > shieldStart + 0.001f;
            }
        }

        private static void SetBarRange(RectTransform rect, float start, float end)
        {
            if (rect == null)
            {
                return;
            }

            start = Mathf.Clamp01(start);
            end = Mathf.Clamp01(Mathf.Max(start, end));

            rect.anchorMin = new Vector2(start, 0f);
            rect.anchorMax = new Vector2(end, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void ApplyShieldStartOverlap(float shieldStart, float shieldEnd)
        {
            if (shieldFill == null || shieldStartOverlapPixels <= 0f || shieldEnd <= shieldStart + 0.001f)
            {
                return;
            }

            Vector2 offsetMin = shieldFill.offsetMin;
            offsetMin.x = -shieldStartOverlapPixels;
            shieldFill.offsetMin = offsetMin;
        }

        private void AutoBindChildren()
        {
            if (hpBarRoot == null)
            {
                hpBarRoot = transform as RectTransform;
            }

            if (hpBarRoot == null)
            {
                return;
            }

            if (hpFill == null)
            {
                hpFill = FindChildRect(hpBarRoot, "HpFill");
            }

            if (shieldFill == null)
            {
                shieldFill = FindChildRect(hpBarRoot, "ShieldFill");
            }

            if (shieldImage == null && shieldFill != null)
            {
                shieldImage = shieldFill.GetComponent<Image>();
            }
        }

        private static RectTransform FindChildRect(Transform root, string childName)
        {
            Transform direct = root.Find(childName);
            if (direct != null)
            {
                return direct as RectTransform;
            }

            foreach (RectTransform child in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
