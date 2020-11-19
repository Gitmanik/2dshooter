using UnityEngine;
using UnityEngine.Events;

public class IngameHUDManager : MonoBehaviour
{
    [Header("Alive")]
    [SerializeField] private GameObject alivePanel;
    [SerializeField] private TMPro.TMP_Text healthText;
    [SerializeField] private TMPro.TMP_Text ammoText;

    [Header("Dead")]
    [SerializeField] private GameObject deadPanel;
    [SerializeField] private TMPro.TMP_Text killedBy;

    [Header("Player List")]
    [SerializeField] private GameObject listPanel;
    [SerializeField] private GameObject panelEntryPrefab;

    [Header("Inventory Selector")]
    [SerializeField] private GameObject gunSelectorPanel;
    [SerializeField] private GameObject gunSelectorEntryPrefab;
    public UnityEvent<int> OnGunSelectorSelected;

    private Player owner;

    public void Setup(Player o)
    {
        owner = o;
        UpdateHealth();
        UpdateAmmo();
    }

    public void ToggleAlive(bool v)
    {
        alivePanel.SetActive(v);
        deadPanel.SetActive(!v);
        ToggleGunSelector(false);
    }

    #region AliveHUD
    public void UpdateHealth()
    {
        healthText.text = $"{owner.health} HP";
    }

    public void UpdateAmmo()
    {
        if (!owner.i.HasAnyGun)
        {
            ammoText.text = "--";
        }
        ammoText.text = $"{owner.i.CurrentGunData.currentAmmo}/{owner.i.CurrentGun.magazineCapacity} ({owner.i.CurrentGunData.totalAmmo})";
    }
    #endregion

    #region Dead
    public void UpdateKilledBy(string by)
    {
        killedBy.text = $"You were killed by: {by}";
    }
    #endregion

    #region Player List

    public void ToggleList(bool v)
    {
        listPanel.SetActive(v);
        if (v)
            UpdatePlayerList();
    }


    public void UpdatePlayerList()
    {
        if (!listPanel.activeSelf)
            return;

        foreach (Transform child in listPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Player p in Player.allPlayers)
        {
            Instantiate(panelEntryPrefab, listPanel.transform).GetComponent<PlayerListEntry>().Setup(p.info);
        }
    }

    #endregion

    #region GunSelector

    public void ToggleGunSelector(bool v)
    {
        gunSelectorPanel.SetActive(v);

        if (v)
        {
            foreach (Transform child in gunSelectorPanel.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var gun in Player.Local.i.inventory)
            {
                Instantiate(gunSelectorEntryPrefab, gunSelectorPanel.transform).GetComponent<GunSelectorElement>().Setup(this, GameManager.Instance.Guns[gun.Value.gunIndex], gun.Key);
            }
        }
    }

    internal void GunSelectorSelected(int idx)
    {
        OnGunSelectorSelected?.Invoke(idx);
        ToggleGunSelector(false);
    }

    #endregion
}
