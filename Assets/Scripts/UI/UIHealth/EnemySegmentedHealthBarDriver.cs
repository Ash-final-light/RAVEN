using TMPro;
using Tools.Battle.Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIHealth
{
    /// <summary>
    /// Drives the enemy status HP UI with a single visible segmented bar and an xN count.
    /// Attach this to FightUI/EnemyStatus.
    /// </summary>
    public class EnemySegmentedHealthBarDriver : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private SegmentedShieldHpBar hpBar;
        [SerializeField] private TMP_Text barCountText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image barBackImage;

        [Header("Segment Settings")]
        [Min(1f)]
        [SerializeField] private float segmentHp = 100f;
        [SerializeField] private Color multiSegmentBackColor = new Color(0.82f, 0.84f, 0.88f, 0.45f);

        [Header("Initial Values")]
        [Min(0f)]
        [SerializeField] private float initialHp = 500f;
        [Min(0f)]
        [SerializeField] private float initialShield = 0f;

        [Header("Runtime")]
        [SerializeField] private bool previewInEditMode = true;

        private CharacterProperty target;
        private float currentHp;
        private float currentShield;
        private Color transparentBackColor;

        public CharacterProperty Target => target;
        public float SegmentHp => segmentHp;
        public float CurrentHp => currentHp;
        public float CurrentShield => currentShield;

        private void Reset()
        {
            AutoBindUi();
        }

        private void Awake()
        {
            AutoBindUi();
            CaptureTransparentColor();
        }

        private void OnEnable()
        {
            AutoBindUi();
            CaptureTransparentColor();

            if (target != null)
            {
                RefreshFromTarget(true);
            }
            else
            {
                ApplyInitialValues(true);
            }
        }

        private void OnValidate()
        {
            segmentHp = Mathf.Max(1f, segmentHp);
            initialHp = Mathf.Max(0f, initialHp);
            initialShield = Mathf.Max(0f, initialShield);
            AutoBindUi();

            if (!Application.isPlaying && previewInEditMode)
            {
                ApplyHealth(initialHp, initialShield, true);
            }
        }

        public void Bind(CharacterProperty characterProperty)
        {
            target = characterProperty;
            SetName(GetDisplayName(target));
            RefreshFromTarget(true);
        }

        public void RefreshFromTarget(bool instant = false)
        {
            if (target == null)
            {
                return;
            }

            SetHpAndShield(target.CurrentHealth, target.CurrentShield, instant);
        }
        public void SetName(string name)
        {
            if (nameText != null)
            {
                nameText.text = name;
            }
        }

        public void SetSegmentHp(float value, bool refresh = true)
        {
            segmentHp = Mathf.Max(1f, value);

            if (refresh)
            {
                ApplyHealth(currentHp, currentShield, true);
            }
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
            ApplyHealth(hp, shield, instant);
        }

        public void ApplyHealth(float hp, float shield, bool instant = false)
        {
            currentHp = Mathf.Max(0f, hp);
            currentShield = Mathf.Max(0f, shield);

            int reserveSegmentCount = CalculateReserveSegmentCount(currentHp);
            float currentSegmentHp = CalculateCurrentSegmentHp(currentHp);

            if (hpBar != null)
            {
                hpBar.SetOriginalHp(segmentHp, false);
                hpBar.SetHpAndShield(currentSegmentHp, currentShield);
            }

            if (barCountText != null)
            {
                bool showReserveSegmentCount = reserveSegmentCount > 0;
                barCountText.gameObject.SetActive(showReserveSegmentCount);
                barCountText.text = showReserveSegmentCount ? $"x{reserveSegmentCount}" : string.Empty;
            }

            ApplyBackColor(reserveSegmentCount);
        }

        private void ApplyInitialValues(bool instant)
        {
            ApplyHealth(initialHp, initialShield, instant);
        }

        private int CalculateReserveSegmentCount(float currentHp)
        {
            if (currentHp <= 0f)
            {
                return 0;
            }

            return Mathf.Max(0, Mathf.FloorToInt(currentHp / segmentHp));
        }

        private float CalculateCurrentSegmentHp(float currentHp)
        {
            if (currentHp <= 0f)
            {
                return 0f;
            }

            return currentHp % segmentHp;
        }

        private void ApplyBackColor(int reserveSegmentCount)
        {
            if (barBackImage == null)
            {
                return;
            }

            barBackImage.color = reserveSegmentCount > 0 ? multiSegmentBackColor : transparentBackColor;
        }

        private void CaptureTransparentColor()
        {
            Color source = barBackImage != null ? barBackImage.color : multiSegmentBackColor;
            transparentBackColor = new Color(source.r, source.g, source.b, 0f);
        }

        private void SetVisible(bool visible)
        {
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(visible);
            }

            if (barCountText != null)
            {
                barCountText.gameObject.SetActive(visible);
            }
        }

        private void AutoBindUi()
        {
            if (hpBar == null)
            {
                Transform enemyHpBarBack = transform.Find("EnemyHpBarBack");
                if (enemyHpBarBack != null)
                {
                    hpBar = enemyHpBarBack.GetComponent<SegmentedShieldHpBar>();
                }
            }

            if (barCountText == null)
            {
                Transform countText = transform.Find("EnemyBarCountText");
                if (countText != null)
                {
                    barCountText = countText.GetComponent<TMP_Text>();
                }
            }

            if (nameText == null)
            {
                Transform enemyNameText = transform.Find("EnemyNameText");
                if (enemyNameText != null)
                {
                    nameText = enemyNameText.GetComponent<TMP_Text>();
                }
            }

            if (barBackImage == null && hpBar != null)
            {
                barBackImage = hpBar.GetComponent<Image>();
            }
        }

    private string GetDisplayName(CharacterProperty characterProperty)
    {
        if (!string.IsNullOrEmpty(characterProperty?.gameObjId))
        {
            return characterProperty.gameObjId;
        }

        return characterProperty?.Info?.Name ?? string.Empty;
    }
}
}
