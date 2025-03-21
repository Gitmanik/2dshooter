using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.UI;

public class RoomEntry : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text slotsText, nameText;
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
        slotsText.text = $"{info.PlayerCount}/{info.MaxPlayers}";
        nameText.text = info.Name;
    }

    private void OnClick()
    {
        Menu.Instance.RoomSelected(info);
    }

    internal void Titlebar()
    {
        button.interactable = false;
        slotsText.text = "-/-";
        nameText.text = "Room Name";
    }
}
