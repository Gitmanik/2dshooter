using Photon.Pun;
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
    [SerializeField] private TMP_Text crouchText;

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

    [Header("Ingame Pause Panel")]
    [SerializeField] private GameObject ingamePausePanel;
    private CanvasGroup ingamePauseCanvasGroup;
    public bool OptionsActive { get => ingamePausePanel.activeSelf; }

    public void DisableAll()
    {
        if (alivePanel != null) alivePanel.SetActive(false);
        if (deadPanel != null) deadPanel.SetActive(false);
        if (gunSelectorPanel != null) gunSelectorPanel.SetActive(false);
        if (listPanel != null) listPanel.SetActive(false);
        if (ingamePausePanel != null) ingamePausePanel.SetActive(false);
        if (debugPanel != null) debugPanel.gameObject.SetActive(false);
    }

    public UnityAction<int> OnGunSelectorSelected;

    private Player owner;

    private void Awake()
    {
        Instance = this;
        ingamePauseCanvasGroup = ingamePausePanel.GetComponent<CanvasGroup>();
    }

    public void ToggleDebug(bool v)
    {
        debugPanel.gameObject.SetActive(v);
    }

    public void UpdateDebug()
    {
        debugPanel.text = string.Format("ping: {0}ms", PhotonNetwork.GetPing());
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
        healthText.text = $"{owner.Health} HP";
        healthText.color = new Color(1f - owner.Health * 0.01f, owner.Health * 0.01f, 0f);
    }

    public void UpdateRunning()
    {
        crouchText.text = owner.Running ? "Running" : "Walking";
    }

    public void UpdateAmmo()
    {
        if (owner.CurrentGun == null || owner.CurrentGunSO.melee)
            ammoText.text = "--";
        else
            ammoText.text = $"{owner.CurrentGun.currentAmmo}/{owner.CurrentGunSO.magazineCapacity} ({owner.CurrentGun.totalAmmo})";
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

            for (int i = 0; i < Player.Local.Inventory.Length; i++)
            {
                if (Player.Local.Inventory[i] == null)
                    continue;

                Instantiate(gunSelectorEntryPrefab, gunSelectorPanel.transform).GetComponent<GunSelectorElement>().Setup(this, Player.Local.Inventory[i], i);
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
        if (v)
        {
            ingamePausePanel.SetActive(true);
            ingamePauseCanvasGroup.alpha = 0f;
            LeanTween.alphaCanvas(ingamePauseCanvasGroup, 1f, 0.15f);
        }
        else
        {
            LeanTween.alphaCanvas(ingamePauseCanvasGroup, 0f, 0.15f).setOnComplete(() => ingamePausePanel.SetActive(false));
        }
    }
    #endregion
}
