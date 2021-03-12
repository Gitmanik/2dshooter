using UnityEngine;
using UnityEngine.Events;

public class IngameHUDManager : MonoBehaviour
{
    public static IngameHUDManager Instance;

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
    public bool GunSelectorActive { get => gunSelectorPanel.activeSelf; }

    [Header("Ingame Options")]
    [SerializeField] private GameObject optionsPanel;
    public bool OptionsActive { get => optionsPanel.activeSelf; }

    internal void Disable()
    {
        if (alivePanel != null) alivePanel.SetActive(false);
        if (deadPanel != null) deadPanel.SetActive(false);
        if (gunSelectorPanel != null) gunSelectorPanel.SetActive(false);
        if (listPanel != null) listPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public UnityAction<int> OnGunSelectorSelected;

    private Player owner;

    private void Awake()
    {
        Instance = this;
    }

    public void Escape()
    {
        if (GunSelectorActive)
        {
            ToggleGunSelector(false);
            return;
        }
        if (OptionsActive)
        {
            ToggleOptionsMenu(false);
            return;
        }
    }

    public void SetupPlayer(Player o)
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
        healthText.color = new Color(1f - owner.health * 0.01f, owner.health * 0.01f, 0f);
    }

    public void UpdateAmmo()
    {
        if (!owner.i.HasAnyGun)
        {
            ammoText.text = "--";
            return;
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

    #region Options
    public void ToggleOptionsMenu(bool v)
    {
        optionsPanel.SetActive(v);
    }
    #endregion
}
