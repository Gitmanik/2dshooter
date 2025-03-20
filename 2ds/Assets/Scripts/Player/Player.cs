using Gitmanik.FOV2D;
using Gitmanik.Notification;
using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviourPun, Target
{
    public static List<Player> allPlayers = new List<Player>();
    public static Player Local;

    [Header("Component references")]
    [SerializeField] private FOVMesh fovmesh;
    [SerializeField] private TMP_Text nickText;
    [SerializeField] private SpriteRenderer skinRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private AudioSource gunAudioSource;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private Transform rotateTransform;
    [SerializeField] private Transform muzzleTransform;

    [Header("Transforms to modify on events")]
    [SerializeField] private Transform[] destroyOnNonLocal;
    [SerializeField] private Transform[] DisableOnDead;

    [Header("Inventory")]
    public GunHolder[] Inventory;
    private int InventoryIndex;
    public GunHolder CurrentGun => Inventory[InventoryIndex];
    public Gun CurrentGunSO => GameManager.Instance.Guns[CurrentGunIndex];

    [Header("Synced variables")]
    public int PlayerSkinIndex;
    private int CurrentGunIndex;
    public string Nickname => photonView.Owner.NickName;
    public int Ping => photonView.IsMine ? PhotonNetwork.GetPing() : _ping; public int _ping;
    public int Health { get => _health; set => photonView.RPC("RPC_SetHealth", RpcTarget.AllBuffered, value); } [SerializeField] private int _health;
    public bool Running
    {
        get => _running;
        set
        {
            _running = value;
            IngameHUDManager.Instance.UpdateRunning();
        }
    } [SerializeField] private bool _running;

    public bool IsAlive = true;

    [Header("Local variables")]
    public bool IsReloading = false;
    public bool LockMovement { get => IngameHUDManager.Instance.GunSelectorActive || IngameHUDManager.Instance.OptionsActive; }
    private float reloadingState;

    private float ctr_crouch;
    private float ctr_footstep;
    private float ctr_ping;
    private float ctr_shoot;
    private Vector3 oldMove;

    #region MonoBehaviour and Movement

    private void Start()
    {
        allPlayers.Add(this);
        name = Nickname;
        nickText.text = Nickname;

        PlayerSkinIndex = (int)photonView.InstantiationData[0];
        skinRenderer.sprite = SkinManager.Instance.GetSprite(PlayerSkinIndex, SkinIndex.HOLD);

        if (!photonView.IsMine)
        {
            foreach (Transform b in destroyOnNonLocal)
            {
                DestroyImmediate(b.gameObject);
            }
            return;
        }

        Hashtable hash = new Hashtable
        {
            ["Deaths"] = 0,
            ["Kills"] = 0
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        Inventory = new GunHolder[GameManager.Instance.Guns.Count];
        for (int i = 0; i < GameManager.Instance.Guns.Count; i++)
        {
            Inventory[i] = GameManager.Instance.Guns[i].GetGunHolder();
        }

        IngameHUDManager.Instance.SetupPlayer(this);
        IngameHUDManager.Instance.ToggleAlive(true);
        IngameHUDManager.Instance.ToggleDebug(true);
        IngameHUDManager.Instance.OnGunSelectorSelected += SelectInventorySlot;

        SelectInventorySlot(0);
        Health = 100;
        Running = true;
        GameCamera.instance.targetTransform = transform;

        oldMove = transform.position;

        Local = this;
    }

    private void OnDestroy()
    {
        allPlayers.Remove(this);
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        ctr_crouch -= Time.deltaTime;
        ctr_shoot -= Time.deltaTime;

        // Ping Update Handling
        ctr_ping += Time.deltaTime;
        if (ctr_ping > 2.5f)
        {
            ctr_ping = 0f;
            photonView.RPC("RPC_UpdatePing", RpcTarget.All, PhotonNetwork.GetPing());
        }
        // ---------------------

        // Footstep Handling
        ctr_footstep += (transform.position - oldMove).magnitude;
        oldMove = transform.position;
        if (ctr_footstep > 1.5f)
        {
            photonView.RPC("PlayEvent", RpcTarget.AllViaServer, EventType.FOOTSTEP);
            ctr_footstep = 0;
        }
        // ---------------------

        if (IsReloading) // Reload Handling
        {
            if (reloadingState >= 0f)
                reloadingState -= Time.deltaTime;
            else
                GunReloaded();
        }

        IngameHUDManager.Instance.UpdateDebug();
        IngameHUDManager.Instance.UpdateHealth();

        if (LockMovement)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                IngameHUDManager.Instance.ToggleOptions();

            rb.linearVelocity = Vector2.zero;
            return;
        }

        #region PlayerList
        if (Input.GetKeyDown(KeyCode.Tab))
            IngameHUDManager.Instance.ToggleList(true);

        if (Input.GetKeyUp(KeyCode.Tab))
            IngameHUDManager.Instance.ToggleList(false);
        #endregion

        if (!IsAlive)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (!IsReloading && CurrentGun != null && ctr_shoot <= 0f && ((CurrentGunSO.autofire && Input.GetKey(KeyCode.Mouse0)) || Input.GetKeyDown(KeyCode.Mouse0)))
        {
            ctr_shoot = 1f / CurrentGunSO.firerate;
            Shoot(Input.GetKeyDown(KeyCode.Mouse0));
        }

        if (Input.GetKeyDown(KeyCode.R) && CurrentGun.totalAmmo > 0 && CurrentGun.currentAmmo != CurrentGunSO.magazineCapacity)
            StartReload();

        if (Input.GetKeyDown(KeyCode.T))
            IngameHUDManager.Instance.ToggleGunSelector(true);

        if (Input.GetKeyDown(KeyCode.Escape))
            IngameHUDManager.Instance.ToggleOptionsMenu(true);

        if (Input.GetKeyDown(KeyCode.LeftControl))
            Running = !Running;
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine || LockMovement)
            return;

        #region Player Movement
        Vector2 change;
        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");

        Vector2 newPos = change.normalized * (Running ? 5f : 2.5f) * Time.fixedDeltaTime * 50f;
        rb.linearVelocity = newPos;
        if (RotateTowardsCamera() || newPos.x != 0 || newPos.y != 0)
            fovmesh.UpdateMesh();

        #endregion
    }
    private bool RotateTowardsCamera()
    {
        Vector2 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        Quaternion r = rotateTransform.rotation;
        rotateTransform.rotation = Quaternion.AngleAxis(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, Vector3.forward);
        return r != rotateTransform.rotation;
    }

    #endregion


    #region EventPlayer

    private enum EventType
    {
        NO_AMMO,
        RELOAD,
        SHOOT,
        DAMAGED,
        FOOTSTEP
    }

    [PunRPC]
    private void PlayEvent(EventType s)
    {
        switch (s)
        {
            case EventType.NO_AMMO:
                PlaySound(GameManager.Instance.noAmmoSound);
                break;
            case EventType.RELOAD:
                PlaySound(GameManager.Instance.reloadSound);
                break;
            case EventType.SHOOT:
                ParticleManager.Spawn(EParticleType.SHOOT, muzzleTransform);
                PlaySound(CurrentGunSO.shootSount);
                break;
            case EventType.DAMAGED:
                if (photonView.IsMine) GameCamera.ShakeOnce(0.2f, 5, new Vector3(0.2f, 0.2f, 0.0f));
                PlaySound(GameManager.Instance.hurtSound);
                ParticleManager.Spawn(EParticleType.BLOOD, transform.position);
                break;
            case EventType.FOOTSTEP:
                PlaySound(GameManager.Instance.footstep, footstepAudioSource, Running ? 0.4f : 0.1f, 0.2f);
                break;
        }
    }

    private void PlaySound(AudioClip clip, AudioSource source = null, float volume = 1f, float pitchChange = 0f)
    {
        if (source == null)
            source = gunAudioSource;

        source.pitch = 1f + Random.Range(-pitchChange, pitchChange);
        source.volume = volume;
        source.clip = clip;
        source.Play();
    }
    #endregion
    #region Inventory and Guns
    [PunRPC]
    public void SetCurrentGunIndex(byte gunindex)
    {
        CurrentGunIndex = gunindex;
        RPC_SetSubSkin(CurrentGunSO.SkinIndex);
    }

    public void SelectInventorySlot(int index)
    {
        InventoryIndex = index;

        if (!photonView.IsMine)
            return;

        photonView.RPC("SetCurrentGunIndex", RpcTarget.All, CurrentGun.gunIndex);

        ctr_shoot = 0f;

        fovmesh.fov.viewAngle = CurrentGunSO.viewAngle;
        fovmesh.fov.viewRadius = CurrentGunSO.viewRadius;
        fovmesh.Setup();
        fovmesh.UpdateMesh();

        IngameHUDManager.Instance.UpdateAmmo();
    }
    #endregion

    #region PunRPCs
    [PunRPC] public void RPC_UpdatePing(int newPing) => _ping = newPing;
    [PunRPC] public void RPC_SetHealth(int amount) => _health = amount;
    [PunRPC] public void RPC_SetSubSkin(SkinIndex subs) => skinRenderer.sprite = SkinManager.Instance.GetSprite(PlayerSkinIndex, subs);

    [PunRPC]
    private void RPC_Respawn()
    {
        IsAlive = true;
        skinRenderer.material.color = Color.white;
        foreach (Transform toDisable in DisableOnDead)
        {
            if (toDisable != null)
                toDisable.gameObject.SetActive(true);
        }
    }

    [PunRPC]
    public void RPC_Died(int killerID)
    {
        foreach (Transform toDisable in DisableOnDead)
        {
            if (toDisable != null)
                toDisable.gameObject.SetActive(false);
        }

        NotificationManager.Instance.LocalSpawn($"{PhotonView.Find(killerID).Owner.NickName} > {Nickname}", new Color(0, 0, 0, 0.8f), 5f);

        if (killerID == Local.photonView.ViewID)
        {
            SetCP("Kills", (int)GetCP("Kills") + 1);
        }

        IsAlive = false;

        skinRenderer.material.color = new Color(1f, 1f, 1f, 0.1f);
        if (photonView.IsMine)
        {
            SetCP("Deaths", (int)GetCP("Deaths") + 1);

            GameCamera.instance.smooth = false;
            IngameHUDManager.Instance.ToggleAlive(false);
            IngameHUDManager.Instance.UpdateKilledBy(PhotonView.Find(killerID).Owner.NickName);

            LeanTween.delayedCall(2.5f, Respawn);
        }
    }

    [PunRPC]
    public void RPC_Damage(int id, float damage)
    {
        if (!IsAlive)
            return;

        PlayEvent(EventType.DAMAGED);

        if (!photonView.IsMine)
            return;

        Health -= Mathf.CeilToInt(damage);

        IngameHUDManager.Instance.UpdateHealth();

        if (Health <= 0)
        {
            Health = 0;
            photonView.RPC("RPC_Died", RpcTarget.All, id);
        }
    }
    #endregion

    #region Reloading
    private void StartReload()
    {
        if (reloadingState > 0f)
            return;

        reloadingState = CurrentGunSO.reloadTime;
        IsReloading = true;

        photonView.RPC("PlayEvent", RpcTarget.All, EventType.RELOAD);
        photonView.RPC("RPC_SetSubSkin", RpcTarget.All, SkinIndex.HOLD);
    }

    private void GunReloaded()
    {
        int zaladowane = Mathf.Min(CurrentGunSO.magazineCapacity - CurrentGun.currentAmmo, CurrentGun.totalAmmo);

        CurrentGun.totalAmmo -= zaladowane;
        CurrentGun.currentAmmo += zaladowane;

        reloadingState = 0f;
        IsReloading = false;

        photonView.RPC("RPC_SetSubSkin", RpcTarget.All, CurrentGunSO.SkinIndex);
        IngameHUDManager.Instance.UpdateAmmo();
    }
    #endregion

    public void Damage(int id, float damage) => photonView.RPC("RPC_Damage", RpcTarget.All, new object[] { id, damage });
    private void Shoot(bool mouseDown)
    {
        if (!photonView.IsMine)
        {
            Debug.LogError("Shoot called on non-owned Player!");
            return;
        }
        if (!CurrentGunSO.melee && CurrentGun.currentAmmo <= 0)
        {
            if (mouseDown)
                photonView.RPC("PlayEvent", RpcTarget.All, EventType.NO_AMMO);
            return;
        }

        CurrentGun.currentAmmo--;

        RaycastHit2D hit = Physics2D.Raycast(rotateTransform.position, rotateTransform.right, 99f);
        if (CurrentGunSO.melee && hit.distance > 1.5f)
            return;

        photonView.RPC("PlayEvent", RpcTarget.All, EventType.SHOOT);
        if (hit.collider != null)
        {
            hit.transform.GetComponent<Target>()?.Damage(photonView.ViewID, CurrentGunSO.damageCurve.Evaluate(hit.distance) * CurrentGunSO.damage);
        }
        IngameHUDManager.Instance.UpdateAmmo();
    }
    private void Respawn()
    {
        transform.position = Level.Instance.GetStartPosition().position;
        ctr_shoot = 0f;
        IsReloading = false;
        reloadingState = 0f;
        GameCamera.instance.smooth = true;
        IngameHUDManager.Instance.ToggleAlive(true);
        Health = 100;
        NotificationManager.Instance.RemoteSpawn($"{Nickname} respawned!", new Color(105f / 255f, 181f / 255f, 120f / 255f, 0.4f), 1f);
        photonView.RPC("RPC_Respawn", RpcTarget.All);
    }

    #region CustomProperties
    public void SetCP(string key, object v)
    {
        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
        hash[key] = v;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
    public object GetCP(string key) => PhotonNetwork.LocalPlayer.CustomProperties[key];
    #endregion
}