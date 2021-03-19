using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gitmanik.Notification
{
    public class NotificationObject : MonoBehaviour
    {
        [SerializeField] private TMP_Text p_text;
        [SerializeField] private Image panel;

        public void Setup(string text, Color color, float alive)
        {
            panel.color = color;
            p_text.text = text;
            LeanTween.delayedCall(alive, () => { LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.5f).setOnComplete(() => Destroy(gameObject)); });
        }
    }
}