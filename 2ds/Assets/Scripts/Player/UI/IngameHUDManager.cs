﻿using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class IngameHUDManager : MonoBehaviour
{
    public static IngameHUDManager Instance;

    [Header("Alive")]
    [SerializeField] private GameObject alivePanel;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text ammoText;

    [Header("Dead")]
    [SerializeField] private GameObject deadPanel;
    [SerializeField] private TMP_Text killedBy;

    [Header("Player List")]
    [SerializeField] private GameObject listPanel;
    [SerializeField] private GameObject panelEntryPrefab;

    [Header("Inventory Selector")]
    [SerializeField] private GameObject gunSelectorPanel;
    [SerializeField] private GameObject gunSelectorEntryPrefab;
    public bool GunSelectorActive { get => gunSelectorPanel.activeSelf; }

    [Header("Debug Info")]
    [SerializeField] private TMP_Text debugPanel;

    [Header("Ingame Options")]
    [SerializeField] private GameObject optionsPanel;
    public bool OptionsActive { get => optionsPanel.activeSelf; }

    internal void DisableAll()
    {
        if (alivePanel != null) alivePanel.SetActive(false);
        if (deadPanel != null) deadPanel.SetActive(false);
        if (gunSelectorPanel != null) gunSelectorPanel.SetActive(false);
        if (listPanel != null) listPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (debugPanel != null) debugPanel.gameObject.SetActive(false);
    }

    public UnityAction<int> OnGunSelectorSelected;

    private Player owner;

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleDebug(bool v)
    {
        debugPanel.gameObject.SetActive(v);
    }

    public void UpdateDebug()
    {
        debugPanel.text = string.Format("ping: {0}ms", (int)(NetworkTime.rtt * 1000));
    }

    public void ToggleOptions()
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
        if (!owner.inventory.HasAnyGun || owner.inventory.CurrentGun.melee)
            ammoText.text = "--";
        else
            ammoText.text = $"{owner.inventory.CurrentGunData.currentAmmo}/{owner.inventory.CurrentGun.magazineCapacity} ({owner.inventory.CurrentGunData.totalAmmo})";
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
            Instantiate(panelEntryPrefab, listPanel.transform).GetComponent<PlayerListEntry>().Setup(p);
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

            foreach (var gun in Player.Local.inventory.inventory)
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
