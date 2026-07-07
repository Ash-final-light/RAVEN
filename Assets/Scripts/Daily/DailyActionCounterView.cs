using TMPro;
using UnityEngine;

namespace Daily
{
    public class DailyActionCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private string format = "每日行动次数    {0}/{1}";

        private void Awake()
        {
            if (text == null)
            {
                text = GetComponent<TMP_Text>();
            }
        }

        public void Refresh(int used, int limit)
        {
            if (text != null)
            {
                text.text = string.Format(format, used, limit);
            }
        }
    }
}
