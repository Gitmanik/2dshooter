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
    [SerializeField] private SpriteRenderer skin;
    [HideInInspector] public PlayerInventory i;
    private Rigidbody2D rb;
    private AudioSource source;
    private Transform rotateTransform;

    [Header("Server-owned variables")]
    [SyncVar(hook = nameof(OnSetInfo))] public PlayerInformation info;
    [SyncVar(hook = nameof(OnChangedHealth))] public float health = -1f;
    [SyncVar(hook = nameof(OnUpdatePing))] public int ping = -1;
    [SyncVar] public bool isAlive = true;
    [SyncVar] public bool isReloading = false;
    [SyncVar] public float reloadingState;
    [SyncVar] public float speed;

    [Header("Transforms to modify on events")]
    [SerializeField] private Transform[] destroyOnNonLocal;
    [SerializeField] private Transform[] DisableOnDead;

    [Header("Client-owned variables")]
    private float shootDelay;
    private Vector2 change = Vector2.zero;
    private float pingCtr = 0;

    #region MonoBehaviour

    private void Start()
    {
        source = GetComponent<AudioSource>();
        i = GetComponent<PlayerInventory>();
        i.parent = this;
        rb = GetComponent<Rigidbody2D>();
        rotateTransform = transform.GetChild(0);
        allPlayers.Add(this);

        if (!hasAuthority)
        {
            foreach (Transform b in destroyOnNonLocal)
            {
                DestroyImmediate(b.gameObject);
            }
            return;
        }

        i.OnSelectedSlot += OnSelectedSlot;
        i.OnSlotUpdate += IngameHUDManager.Instance.UpdateAmmo;
        IngameHUDManager.Instance.SetupPlayer(this);
        IngameHUDManager.Instance.ToggleAlive(true);
        IngameHUDManager.Instance.ToggleDebug(true);
        IngameHUDManager.Instance.OnGunSelectorSelected += i.CmdSelectSlot;
        CameraFollow.instance.targetTransform = transform;

        Local = this;

        OnSelectedSlot(); //Force Fovmesh generation
    }

    private void OnDestroy()
    {
        IngameHUDManager.Instance.OnGunSelectorSelected -= i.CmdSelectSlot;
        allPlayers.Remove(this);
    }

    private void Update()
    {
        if (isReloading && NetworkServer.active)
        {
            if (reloadingState >= 0f)
                reloadingState -= Time.deltaTime;
            else
                ServerGunReloaded();
        }

        if (!hasAuthority)
            return;

        IngameHUDManager.Instance.UpdateDebug();

        pingCtr += Time.unscaledDeltaTime;

        if (pingCtr >= .5f)
        {
            NetworkClient.Send(new PlayerPingMessage { ping = (int)(NetworkTime.rtt * 1000) });
            pingCtr = 0;
        }

        shootDelay -= Time.deltaTime;

        if (LockMovement)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                IngameHUDManager.Instance.ToggleOptions();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
            IngameHUDManager.Instance.ToggleList(true);

        if (Input.GetKeyUp(KeyCode.Tab))
            IngameHUDManager.Instance.ToggleList(false);

        if (!isAlive || LockMovement)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (i.HasAnyGun && shootDelay <= 0f && (i.CurrentGun.autofire && Input.GetKey(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse0)))
        {
            shootDelay = 1f / i.CurrentGun.firerate;
            if (i.CurrentGunData.currentAmmo <= 0f)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                    CmdPlayEvent(SoundType.NO_AMMO);
            }
            else
            {
                CmdShoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && i.CurrentGunData.totalAmmo > 0)
            CmdStartReload();

        if (Input.GetKeyDown(KeyCode.T))
            IngameHUDManager.Instance.ToggleGunSelector(true);

        if (Input.GetKeyDown(KeyCode.Escape))
            IngameHUDManager.Instance.ToggleOptionsMenu(true);

        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");

        Vector2 newPos = change.normalized * speed * Time.deltaTime * 60f;
        rb.velocity = newPos;

        if (RotateTowardsCamera() || newPos.x != 0 || newPos.y != 0)
            fovmesh.UpdateMesh();
    }

    #endregion

    #region EventPlayer

    private enum SoundType
    {
        NO_AMMO,
        RELOAD,
        SHOOT,
        DAMAGED
    }

    [Command]
    private void CmdPlayEvent(SoundType s) => RpcPlayEvent(s);

    [ClientRpc]
    private void RpcPlayEvent(SoundType s)
    {
        switch (s)
        {
            case SoundType.NO_AMMO:
                PlaySound(GameManager.Instance.noAmmoSound);
                break;
            case SoundType.RELOAD:
                PlaySound(GameManager.Instance.reloadSound);
                break;
            case SoundType.SHOOT:
                ParticleManager.Spawn(EParticleType.SHOOT, rotateTransform);
                PlaySound(i.CurrentGun.shootSount);
                break;
            case SoundType.DAMAGED:
                PlaySound(GameManager.Instance.hurtSound);
                ParticleManager.Spawn(EParticleType.BLOOD, transform.position);
                break;
        }
    }

    [ClientRpc]
    internal void RpcOnPlayerDied(GameObject x)
    {
        Player playerKiller = x.GetComponent<Player>();
        string killer = x.name;
        if (playerKiller != null)
            killer = playerKiller.info.Nickname;

        foreach (Transform toDisable in DisableOnDead)
        {
            if (toDisable != null)
                toDisable.gameObject.SetActive(false);
        }
        if (hasAuthority)
        {
            CameraFollow.instance.smooth = false;
            IngameHUDManager.Instance.ToggleAlive(false);
            IngameHUDManager.Instance.UpdateKilledBy(killer);
        }
    }

    [ClientRpc]
    private void RpcOnPlayerRespawned()
    {
        CameraFollow.instance.smooth = true;
        shootDelay = 0f;
        foreach (Transform toDisable in DisableOnDead)
        {
            if (toDisable != null)
                toDisable.gameObject.SetActive(true);
        }

        if (hasAuthority)
        {
            IngameHUDManager.Instance.ToggleAlive(true);
        }
    }
    #endregion

    #region SyncVar events
    private void OnSetInfo(PlayerInformation oldinfo, PlayerInformation newinfo)
    {
        if (oldinfo.Nickname != newinfo.Nickname)
            nickText.text = info.Nickname;

        if (hasAuthority)
            IngameHUDManager.Instance.UpdatePlayerList();
    }
    private void OnChangedHealth(float _, float __)
    {
        if (hasAuthority)
            IngameHUDManager.Instance.UpdateHealth();
    }
    private void OnUpdatePing(int a, int b)
    {
        IngameHUDManager.Instance.UpdatePlayerList();
    }

    #endregion

    private void OnSelectedSlot()
    {
        if (!i.HasAnyGun)
        {
            print("spawned with no gun");
            return;
        }

        shootDelay = 0f;

        fovmesh.fov.viewAngle = i.CurrentGun.viewAngle;
        fovmesh.fov.viewRadius = i.CurrentGun.viewRadius;
        fovmesh.Setup();
        fovmesh.UpdateMesh();

        IngameHUDManager.Instance.UpdateAmmo();
    }

    [TargetRpc]
    public void TargetTeleport(Vector3 newPos)
    {
        transform.position = newPos;
    }

    private void PlaySound(AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }

    private bool RotateTowardsCamera()
    {
        Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion r = rotateTransform.rotation;
        rotateTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        return r != rotateTransform.rotation;
    }

    #region Server

    public void Setup(AuthRequestMessage data)
    {
        info = new PlayerInformation() { Nickname = data.nick, SkinIndex = data.skinindex };
        skin.sprite = GameManager.Instance.PlayerSkins[data.skinindex];
        name = $"Player {data.nick}";
    }

    [Server]
    private void ServerGunReloaded()
    {
        int zaladowane = Mathf.Min(i.CurrentGun.magazineCapacity - i.CurrentGunData.currentAmmo, i.CurrentGunData.totalAmmo);

        GunData gd = i.CurrentGunData;

        gd.totalAmmo -= zaladowane;
        gd.currentAmmo += zaladowane;

        i.CurrentGunData = gd;
        reloadingState = 0f;
        isReloading = false;
    }

    [Server]
    internal void Respawn()
    {
        isAlive = true;
        RpcOnPlayerRespawned();
    }

    [Command]
    private void CmdStartReload()
    {
        if (reloadingState > 0f)
            return;

        reloadingState = i.CurrentGun.reloadTime;
        isReloading = true;

        RpcPlayEvent(SoundType.RELOAD);
    }

    [Command]
    private void CmdShoot()
    {
        GunData gd = i.CurrentGunData;
        if (gd.currentAmmo <= 0)
        {
            Debug.LogWarning("Tried to shoot with 0 ammo.");
            return;
        }

        gd.currentAmmo--;
        i.CurrentGunData = gd;

        RpcPlayEvent(SoundType.SHOOT);

        RaycastHit2D hit = Physics2D.Raycast(rotateTransform.position, rotateTransform.right, 99f);
        Target t;
        if (hit.collider != null)
        {
            t = hit.transform.GetComponent<Target>();
            t?.Damage(gameObject, i.CurrentGun.damageCurve.Evaluate(hit.distance) * i.CurrentGun.damage);
        }
    }

    [Server]
    public void Damage(GameObject from, float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            isAlive = false;
            RpcOnPlayerDied(from);

            PlayerInformation xx = info;
            xx.deathCount++;
            info = xx;

            Player playerKiller = from.GetComponent<Player>();
            if (playerKiller != null)
            {
                PlayerInformation asaa = playerKiller.info;
                asaa.killCount++;
                playerKiller.info = asaa;
            }

            Level.Instance.PlayerDied(this, from);
        }
    }
    #endregion

}