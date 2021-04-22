using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomEntry : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text descText;
    [SerializeField] private Button button;

    public RoomInfo info;

    public void Setup(RoomInfo info)
    {
        UpdateInfo(info);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    public void UpdateInfo(RoomInfo info)
    {
        this.info = info;
        descText.text = info.ToString();
    }

    private void OnClick()
    {
        Menu.Instance.RoomSelected(info);
    }
}
