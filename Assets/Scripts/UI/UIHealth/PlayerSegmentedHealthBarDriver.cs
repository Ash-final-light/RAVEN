using TMPro;
using UnityEngine;

namespace UI.UIHealth
{
    /// <summary>
    /// Drives the player HP bar and numeric HP text.
    /// Attach this to FightUI/PlayerStatus.
    /// </summary>
    public class PlayerSegmentedHealthBarDriver : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private SegmentedShieldHpBar hpBar;
        [SerializeField] private TMP_Text currentHpText;
        [SerializeField] private TMP_Text maxHpText;

        [Header("Initial Values")]
        [Min(1f)]
        [SerializeField] private float originalHp = 6854f;
        [Min(0f)]
        [SerializeField] private float initialHp = 6854f;
        [Min(0f)]
        [SerializeField] private float initialShield = 0f;

        [Header("Runtime")]
        [SerializeField] private bool previewInEditMode = true;

        private float currentHp;
        private float currentShield;

        public float OriginalHp => originalHp;
        public float CurrentHp => currentHp;
        public float CurrentShield => currentShield;

        private void Reset()
        {
            AutoBindUi();
        }

        private void Awake()
        {
            AutoBindUi();
        }

        private void OnEnable()
        {
            AutoBindUi();
            ApplyInitialValues(true);
        }

        private void OnValidate()
        {
            originalHp = Mathf.Max(1f, originalHp);
            initialHp = Mathf.Clamp(initialHp, 0f, originalHp);
            initialShield = Mathf.Max(0f, initialShield);
            AutoBindUi();

            if (!Application.isPlaying && previewInEditMode)
            {
                SetOriginalHp(originalHp, false);
                SetHpAndShield(initialHp, initialShield, true);
            }
        }

        public void SetOriginalHp(float value, bool refill)
        {
            originalHp = Mathf.Max(1f, value);

            if (hpBar != null)
            {
                hpBar.SetOriginalHp(originalHp, refill);
            }

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

            UpdateHpText();
        }

        public void SetHp(float hp, bool instant = false)
        {
            SetHpAndShield(hp, currentShield, instant);
        }

        public void SetShield(float shield, bool instant = false)
        {
            SetHpAndShield(currentHp, shield, instant);
        }

        public void SetHpAndShield(float hp, float shield, bool instant = false)
        {
            currentHp = Mathf.Clamp(hp, 0f, originalHp);
            currentShield = Mathf.Max(0f, shield);

            if (hpBar != null)
            {
                hpBar.SetOriginalHp(originalHp, false);
                hpBar.SetHpAndShield(currentHp, currentShield);
            }

            UpdateHpText();
        }

        public void TakeDamage(float damage)
        {
            damage = Mathf.Max(0f, damage);

            float shieldDamage = Mathf.Min(currentShield, damage);
            currentShield -= shieldDamage;
            damage -= shieldDamage;
            currentHp = Mathf.Clamp(currentHp - damage, 0f, originalHp);

            if (hpBar != null)
            {
                hpBar.SetHpAndShield(currentHp, currentShield);
            }

            UpdateHpText();
        }

        public void Refill()
        {
            SetOriginalHp(originalHp, true);
        }

        private void ApplyInitialValues(bool instant)
        {
            currentHp = Mathf.Clamp(initialHp, 0f, originalHp);
            currentShield = Mathf.Max(0f, initialShield);
            SetHpAndShield(currentHp, currentShield, instant);
        }

        private void UpdateHpText()
        {
            if (currentHpText != null)
            {
                currentHpText.text = Mathf.RoundToInt(currentHp).ToString();
            }

            if (maxHpText != null)
            {
                maxHpText.text = $"{Mathf.RoundToInt(originalHp)}";
            }
        }

        private void AutoBindUi()
        {
            if (hpBar == null)
            {
                Transform hpBarBack = transform.Find("HpBarBack");
                if (hpBarBack != null)
                {
                    hpBar = hpBarBack.GetComponent<SegmentedShieldHpBar>();
                }
            }

            Transform hpObject = transform.Find("HpObject");
            if (hpObject == null)
            {
                return;
            }

            if (currentHpText == null)
            {
                Transform currentText = hpObject.Find("CurHpText");
                if (currentText != null)
                {
                    currentHpText = currentText.GetComponent<TMP_Text>();
                }
            }

            if (maxHpText == null)
            {
                Transform maxText = hpObject.Find("CurHpText (1)");
                if (maxText != null)
                {
                    maxHpText = maxText.GetComponent<TMP_Text>();
                }
            }
        }
    }
}
