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
    [SerializeField] public PlayerInventory i;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private AudioSource source;
    [SerializeField] private Transform rotateTransform;
    [SerializeField] private Transform muzzleTransform;

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
        allPlayers.Add(this);

        skin.sprite = SkinManager.Instance.GetSprite(info.SkinIndex, i.CurrentGun.SkinIndex);

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

            rb.velocity = Vector2.zero;
            return;
        }

        #region PlayerList
        if (Input.GetKeyDown(KeyCode.Tab))
            IngameHUDManager.Instance.ToggleList(true);

        if (Input.GetKeyUp(KeyCode.Tab))
            IngameHUDManager.Instance.ToggleList(false);
        #endregion

        if (Input.GetKeyDown(KeyCode.K))
            CmdSetSkinIndex((info.SkinIndex + 1) % SkinManager.Instance.AllSkins.Length);

        if (!isAlive)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (!isReloading && i.HasAnyGun && shootDelay <= 0f && (i.CurrentGun.autofire && Input.GetKey(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse0)))
        {
            shootDelay = 1f / i.CurrentGun.firerate;
            CmdShoot(Input.GetKeyDown(KeyCode.Mouse0));
        }

        if (Input.GetKeyDown(KeyCode.R) && i.CurrentGunData.totalAmmo > 0 && i.CurrentGunData.currentAmmo != i.CurrentGun.magazineCapacity)
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

    private enum EventType
    {
        NO_AMMO,
        RELOAD,
        SHOOT,
        DAMAGED
    }

    [Command]
    private void CmdPlayEvent(EventType s) => RpcPlayEvent(s);

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
                PlaySound(i.CurrentGun.shootSount);
                break;
            case EventType.DAMAGED:
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

        if (i.CurrentGun != null)
            skin.sprite = SkinManager.Instance.GetSprite(info.SkinIndex, i.CurrentGun.SkinIndex);

        name = $"Player {info.Nickname}";

        if (hasAuthority)
            IngameHUDManager.Instance.UpdatePlayerList();
    }
    private void OnChangedHealth(float a, float b)
    {
        if (hasAuthority)
            IngameHUDManager.Instance.UpdateHealth();
    }
    private void OnUpdatePing(int _, int __)
    {
        IngameHUDManager.Instance.UpdatePlayerList();
    }

    #endregion

    [Command]
    private void CmdSetSkinIndex(int SkinIndex){
        PlayerInformation x = info;
        x.SkinIndex = SkinIndex;
        info = x;
        print(SkinIndex);
    }

    [ClientRpc]
    private void RpcSetSkin(SkinIndex s)
    {
        skin.sprite = SkinManager.Instance.GetSprite(info.SkinIndex, s);
    }

    private void OnSelectedSlot()
    {
        shootDelay = 0f;

        fovmesh.fov.viewAngle = i.CurrentGun.viewAngle;
        fovmesh.fov.viewRadius = i.CurrentGun.viewRadius;
        fovmesh.Setup();
        fovmesh.UpdateMesh();

        skin.sprite = SkinManager.Instance.GetSprite(info.SkinIndex, i.CurrentGun.SkinIndex);

        IngameHUDManager.Instance.UpdateAmmo();
    }

    [TargetRpc]
    public void TargetTeleport(Vector3 newPos)
    {
        transform.position = newPos;
    }

    /// <summary>
    /// Sets sound to attached AudioSource and plays it.
    /// </summary>
    /// <param name="clip">Sound to play</param>
    private void PlaySound(AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }


    /// <summary>
    /// Rotates rotateTransform towards mouse pointer.
    /// </summary>
    private bool RotateTowardsCamera()
    {
        Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion r = rotateTransform.rotation;
        rotateTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        return r != rotateTransform.rotation;
    }

    #region Server

    /// <summary>
    /// Sets up client's representation.
    /// </summary>
    /// <param name="data">Client data</param>
    public void Setup(AuthRequestMessage data)
    {
        info = new PlayerInformation() { Nickname = data.nick, SkinIndex = data.skinindex };
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
        RpcSetSkin(i.CurrentGun.SkinIndex);
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

        RpcPlayEvent(EventType.RELOAD);
        RpcSetSkin(SkinIndex.HOLD);
    }

    [Command]
    private void CmdShoot(bool mouseDown)
    {
        GunData gd = i.CurrentGunData;
        if (!i.CurrentGun.melee && gd.currentAmmo <= 0)
        {
            if (mouseDown)
                RpcPlayEvent(EventType.NO_AMMO);
            return;
        }

        gd.currentAmmo--;
        i.CurrentGunData = gd;

        RaycastHit2D hit = Physics2D.Raycast(rotateTransform.position, rotateTransform.right, 99f);
        if (i.CurrentGun.melee && hit.distance > 1.5f)
            return;

        RpcPlayEvent(EventType.SHOOT);
        if (hit.collider != null)
        {
            hit.transform.GetComponent<Target>()?.Damage(gameObject, i.CurrentGun.damageCurve.Evaluate(hit.distance) * i.CurrentGun.damage);
        }
    }

    [Server]
    public void Damage(GameObject from, float damage)
    {
        RpcPlayEvent(EventType.DAMAGED);
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