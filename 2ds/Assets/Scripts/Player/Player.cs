using Gitmanik.FOV2D;
using Gitmanik.Multiplayer.Inventory;
using Mirror;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerInventory))]
public class Player : NetworkBehaviour, Target
{
    public static List<Player> allPlayers = new List<Player>();
    public static Player Local;

    public bool LockMovement { get => IngameHUDManager.Instance.GunSelectorActive || IngameHUDManager.Instance.OptionsActive; }

    [Header("Component references")]
    [SerializeField] private FOVMesh fovmesh;
    [SerializeField] private TMP_Text nickText;
    [SerializeField] private SpriteRenderer skinRenderer;
    [SerializeField] public PlayerInventory inventory;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private AudioSource gunAudioSource;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private Transform rotateTransform;
    [SerializeField] private Transform muzzleTransform;

    [Header("Server-owned variables")]
    [SyncVar(hook = nameof(OnUpdateSkinIndex))]  public int PlayerSkinIndex;
    [SyncVar(hook = nameof(OnSetNickname))]      public string Nickname;
    [SyncVar(hook = nameof(OnUpdatePlayerInfo))] public int killCount;
    [SyncVar(hook = nameof(OnUpdatePlayerInfo))] public int deathCount;
    [SyncVar(hook = nameof(OnChangedHealth))]    public float health = -1f;
    [SyncVar(hook = nameof(OnUpdatePlayerInfo))] public int ping = -1;
    [SyncVar] public bool isAlive = true;
    [SyncVar] public bool isReloading = false;
    [SyncVar] public float reloadingState;
    [SyncVar] public float speed;
    private float S_crouchdelay;
    private float S_footstepCtr;

    [Header("Transforms to modify on events")]
    [SerializeField] private Transform[] destroyOnNonLocal;
    [SerializeField] private Transform[] DisableOnDead;

    [Header("Client-owned variables")]
    private float C_shootDelay;
    private float C_pingDelay;

    private Vector3 oldMove;

    #region MonoBehaviour

    private void Start()
    {
        allPlayers.Add(this);

        skinRenderer.sprite = SkinManager.Instance.GetSprite(PlayerSkinIndex, inventory.CurrentGun.SkinIndex);

        if (!isLocalPlayer)
        {
            foreach (Transform b in destroyOnNonLocal)
            {
                DestroyImmediate(b.gameObject);
            }
            return;
        }

        inventory.OnSelectedSlot += OnSelectedSlot;
        inventory.OnSlotUpdate += IngameHUDManager.Instance.UpdateAmmo;
        IngameHUDManager.Instance.SetupPlayer(this);
        IngameHUDManager.Instance.ToggleAlive(true);
        IngameHUDManager.Instance.ToggleDebug(true);
        IngameHUDManager.Instance.OnGunSelectorSelected += inventory.CmdSelectSlot;
        GameCamera.instance.targetTransform = transform;

        oldMove = transform.position;

        Local = this;

        OnSelectedSlot(); //Force Fovmesh generation
    }

    private void OnDestroy()
    {
        IngameHUDManager.Instance.OnGunSelectorSelected -= inventory.CmdSelectSlot;
        allPlayers.Remove(this);
    }

    private void Update()
    {
        if (NetworkServer.active)
        {
            #region Footstep Handling
            S_footstepCtr += (transform.position - oldMove).magnitude;
            oldMove = transform.position;
            if (S_footstepCtr > 1.5f)
            {
                RpcPlayEvent(EventType.FOOTSTEP);
                S_footstepCtr = 0;
            }
            #endregion

            if (isReloading) // Reload Handling
            {
                if (reloadingState >= 0f)
                    reloadingState -= Time.deltaTime;
                else
                    ServerGunReloaded();
            }
        }

        if (!isLocalPlayer)
            return;

        IngameHUDManager.Instance.UpdateDebug();

        #region RTT Synchronization
        C_pingDelay += Time.unscaledDeltaTime;

        if (C_pingDelay >= .5f)
        {
            CmdSetPlayerPing((int)(NetworkTime.rtt * 1000));
            C_pingDelay = 0;
        }
        #endregion

        C_shootDelay -= Time.deltaTime;

        if (LockMovement)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                IngameHUDManager.Instance.ToggleOptions();

            rb.velocity = Vector2.zero;
            return;
        }

        #region PlayerList
        if (Input.GetKeyDown(KeyCode.Tab))
            IngameHUDManager.Instance.ToggleList(true);

        if (Input.GetKeyUp(KeyCode.Tab))
            IngameHUDManager.Instance.ToggleList(false);
        #endregion

        if (!isAlive)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (!isReloading && inventory.HasAnyGun && C_shootDelay <= 0f && ((inventory.CurrentGun.autofire && Input.GetKey(KeyCode.Mouse0)) || Input.GetKeyDown(KeyCode.Mouse0)))
        {
            C_shootDelay = 1f / inventory.CurrentGun.firerate;
            CmdShoot(Input.GetKeyDown(KeyCode.Mouse0));
        }

        if (Input.GetKeyDown(KeyCode.R) && inventory.CurrentGunData.totalAmmo > 0 && inventory.CurrentGunData.currentAmmo != inventory.CurrentGun.magazineCapacity)
            CmdStartReload();

        if (Input.GetKeyDown(KeyCode.T))
            IngameHUDManager.Instance.ToggleGunSelector(true);

        if (Input.GetKeyDown(KeyCode.Escape))
            IngameHUDManager.Instance.ToggleOptionsMenu(true);

        if (Input.GetKeyDown(KeyCode.LeftControl))
            CmdToggleRunning();

        #region Player Movement
        Vector2 change;
        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");

        Vector2 newPos = change.normalized * speed * Time.deltaTime * 60f;
        rb.velocity = newPos;
        if (RotateTowardsCamera() || newPos.x != 0 || newPos.y != 0)
            fovmesh.UpdateMesh();

        #endregion
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

    [Command] private void CmdPlayEvent(EventType s) => RpcPlayEvent(s);

    [ClientRpc]
    private void RpcPlayEvent(EventType s)
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
                PlaySound(inventory.CurrentGun.shootSount);
                break;
            case EventType.DAMAGED:
                if (isLocalPlayer) GameCamera.ShakeOnce(0.2f, 5, new Vector3(0.2f, 0.2f, 0.0f));
                PlaySound(GameManager.Instance.hurtSound);
                ParticleManager.Spawn(EParticleType.BLOOD, transform.position);
                break;
            case EventType.FOOTSTEP:
                PlaySound(GameManager.Instance.footstep, footstepAudioSource, speed ==  5f ? 0.4f : 0.1f, 0.2f);
                break;
        }
    }
    #endregion

    [ClientRpc]
    internal void RpcDied(GameObject x)
    {
        Player playerKiller = x.GetComponent<Player>();
        string killer = x.name;
        if (playerKiller != null)
            killer = playerKiller.Nickname;

        foreach (Transform toDisable in DisableOnDead)
        {
            if (toDisable != null)
                toDisable.gameObject.SetActive(false);
        }

        skinRenderer.material.color = new Color(1f, 1f, 1f, 0.1f);

        if (isLocalPlayer)
        {
            GameCamera.instance.smooth = false;
            IngameHUDManager.Instance.ToggleAlive(false);
            IngameHUDManager.Instance.UpdateKilledBy(killer);
        }
    }

    [ClientRpc]
    private void RpcRespawn()
    {
        skinRenderer.material.color = Color.white;
        GameCamera.instance.smooth = true;
        C_shootDelay = 0f;
        isReloading = false;
        reloadingState = 0f;
        foreach (Transform toDisable in DisableOnDead)
        {
            if (toDisable != null)
                toDisable.gameObject.SetActive(true);
        }

        if (isLocalPlayer)
        {
            IngameHUDManager.Instance.ToggleAlive(true);
        }
    }


    #region SyncVar events

    private void OnUpdateSkinIndex(int _, int __)
    {
        if (inventory.CurrentGun != null)
            skinRenderer.sprite = SkinManager.Instance.GetSprite(PlayerSkinIndex, inventory.CurrentGun.SkinIndex);
    }

    private void OnSetNickname(string _, string __)
    {
        nickText.text = Nickname;
        name = $"{(isLocalPlayer ? "Local" : "")} Player: {Nickname}";
        if (isLocalPlayer)
            IngameHUDManager.Instance.UpdatePlayerList();
    }

    private void OnChangedHealth(float a, float b)
    {
        if (isLocalPlayer)
            IngameHUDManager.Instance.UpdateHealth();
    }
    private void OnUpdatePlayerInfo(int _, int __)
    {
        IngameHUDManager.Instance.UpdatePlayerList();
    }

    #endregion

    [Command]
    private void CmdToggleRunning()
    {
        speed = (speed == 5f) ? 2.5f : 5f;
        IngameHUDManager.Instance.UpdateRunning(speed == 5f ? "Running" : "Walking");
    }

    [Command] private void CmdSetPlayerPing(int v) => ping = v;
    [Command] private void CmdSetSkinIndex(int SkinIndex) => PlayerSkinIndex = SkinIndex;

    [ClientRpc] private void RpcSetSkin(SkinIndex s) => skinRenderer.sprite = SkinManager.Instance.GetSprite(PlayerSkinIndex, s);

    [TargetRpc] public void TargetTeleport(Vector3 newPos) => transform.position = newPos;

    private void OnSelectedSlot()
    {
        C_shootDelay = 0f;

        fovmesh.fov.viewAngle = inventory.CurrentGun.viewAngle;
        fovmesh.fov.viewRadius = inventory.CurrentGun.viewRadius;
        fovmesh.Setup();
        fovmesh.UpdateMesh();

        skinRenderer.sprite = SkinManager.Instance.GetSprite(PlayerSkinIndex, inventory.CurrentGun.SkinIndex);

        IngameHUDManager.Instance.UpdateAmmo();
    }

    private void PlaySound(AudioClip clip, AudioSource source = null, float volume = 1f, float pitchChange = 0f)
    {
        if (source == null)
            source = gunAudioSource;

        source.pitch = 1f + UnityEngine.Random.Range(-pitchChange, pitchChange);
        source.volume = volume;
        source.clip = clip;
        source.Play();
    }

    private bool RotateTowardsCamera()
    {
        Vector2 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        Quaternion r = rotateTransform.rotation;
        rotateTransform.rotation = Quaternion.AngleAxis(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, Vector3.forward);
        return r != rotateTransform.rotation;
    }

    #region Server

    public void Setup(AuthRequestMessage data)
    {
        Nickname = data.nick;
        PlayerSkinIndex = data.skinindex;
    }

    #region Reloading
    [Command]
    private void CmdStartReload()
    {
        if (reloadingState > 0f)
            return;

        reloadingState = inventory.CurrentGun.reloadTime;
        isReloading = true;

        RpcPlayEvent(EventType.RELOAD);
        RpcSetSkin(SkinIndex.HOLD);
    }

    [Server]
    private void ServerGunReloaded()
    {
        int zaladowane = Mathf.Min(inventory.CurrentGun.magazineCapacity - inventory.CurrentGunData.currentAmmo, inventory.CurrentGunData.totalAmmo);

        GunData gd = inventory.CurrentGunData;

        gd.totalAmmo -= zaladowane;
        gd.currentAmmo += zaladowane;

        inventory.CurrentGunData = gd;
        reloadingState = 0f;
        isReloading = false;
        RpcSetSkin(inventory.CurrentGun.SkinIndex);
    }
    #endregion

    [Server]
    internal void Respawn()
    {
        isAlive = true;
        RpcRespawn();
    }

    [Command]
    private void CmdShoot(bool mouseDown)
    {
        GunData gd = inventory.CurrentGunData;
        if (!inventory.CurrentGun.melee && gd.currentAmmo <= 0)
        {
            if (mouseDown)
                RpcPlayEvent(EventType.NO_AMMO);
            return;
        }

        gd.currentAmmo--;
        inventory.CurrentGunData = gd;

        RaycastHit2D hit = Physics2D.Raycast(rotateTransform.position, rotateTransform.right, 99f);
        if (inventory.CurrentGun.melee && hit.distance > 1.5f)
            return;

        RpcPlayEvent(EventType.SHOOT);
        if (hit.collider != null)
        {
            hit.transform.GetComponent<Target>()?.Damage(gameObject, inventory.CurrentGun.damageCurve.Evaluate(hit.distance) * inventory.CurrentGun.damage);
        }
    }

    [Server]
    public void Damage(GameObject killer, float damage)
    {
        if (!isAlive)
            return;

        RpcPlayEvent(EventType.DAMAGED);
        if ((health - damage) <= 0f)
        {
            health = 0f;
            isAlive = false;
            RpcDied(killer);

            deathCount++;

            Player killerPlayer = killer.GetComponent<Player>();
            killerPlayer.killCount++;

            Level.Instance.PlayerDied(this, killer);
        }
        else
        {
            health -= damage;
        }
        #endregion
    }
}