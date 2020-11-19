using Mirror;
using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    public void OnDisconnectClick()
    {
        NetworkManager manager = NetworkManager.singleton;
        if (manager == null)
            return;

        switch (manager.mode)
        {
            case NetworkManagerMode.ServerOnly:
                manager.StopServer();
                break;
            case NetworkManagerMode.Host:
                manager.StopHost();
                break;
            case NetworkManagerMode.ClientOnly:
                manager.StopClient();
                break;
        }
        GameManager.Instance.ToggleOptionsMenu(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            GameManager.Instance.ToggleOptionsMenu(false);
    }
}
