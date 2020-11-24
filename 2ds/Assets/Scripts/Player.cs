using Gitmanik.FOV2D;
using Mirror;
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
    [HideInInspector] public NetworkIdentity identity;
    [HideInInspector] public PlayerInventory i;
    private Rigidbody2D rb;
    private AudioSource source;
    private Transform rotateTransform;

    [Header("Server-owned variables")]
    [SyncVar(hook = nameof(OnSetInfo))] public PlayerInformation info;
    [SyncVar(hook = nameof(OnChangedHealth))] public float health = -1f;
    [SyncVar] public bool isAlive = true;

    [Header("Transforms to modify on events")]
    [SerializeField] private Transform[] destroyOnNonLocal;
    [SerializeField] private Transform[] DisableOnDead;

    [Header("Client-owned variables")]
    public float speed;
    private float shootDelay;
    private Vector3 change = Vector3.zero;

    #region MonoBehaviour

    private void Start()
    {
        identity = GetComponent<NetworkIdentity>();
        source = GetComponent<AudioSource>();
        i = GetComponent<PlayerInventory>();
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

        Local = this;
        IngameHUDManager.Instance.SetupPlayer(this);
        IngameHUDManager.Instance.ToggleAlive(true);
        IngameHUDManager.Instance.OnGunSelectorSelected += OnGunSelected;

        CameraFollow.instance.targetTransform = transform;
    }

    private void OnDestroy()
    {
        IngameHUDManager.Instance.OnGunSelectorSelected -= OnGunSelected;
        allPlayers.Remove(this);
    }

    private void Update()
    {
        if (!hasAuthority)
            return;

        shootDelay -= Time.deltaTime;

        if (LockMovement)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                IngameHUDManager.Instance.Escape();

            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
            IngameHUDManager.Instance.ToggleList(true);

        if (Input.GetKeyUp(KeyCode.Tab))
            IngameHUDManager.Instance.ToggleList(false);

        if (!isAlive)
            return;

        if (i.HasAnyGun && shootDelay <= 0f && Input.GetKey(KeyCode.Mouse0))
        {
            shootDelay = 1f / i.CurrentGun.firerate;
            if (i.CurrentGunData.currentAmmo <= 0f)
            {
                PlaySound(GameManager.Instance.noAmmoSound);
            }
            else
            {
                CmdShoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && i.CurrentGunData.totalAmmo > 0)
            CmdReload();

        if (Input.GetKeyDown(KeyCode.T))
            IngameHUDManager.Instance.ToggleGunSelector(true);

        if (Input.GetKeyDown(KeyCode.Escape))
            IngameHUDManager.Instance.ToggleOptionsMenu(true);

        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        if (!hasAuthority || LockMovement)
            return;

        Vector3 newPos = change.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + newPos);

        if (Rotate() || newPos.x != 0 || newPos.y != 0)
            fovmesh.UpdateMesh();
    }

    #endregion

    #region Client

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
    #endregion

    private void OnGunSelected(int idx)
    {
        i.CmdSelectSlot(idx);
    }

    #region TargetRPCs


    private void OnSelectedSlot()
    {
        shootDelay = 0f;

        fovmesh.fov.viewAngle = i.CurrentGun.viewAngle;
        fovmesh.fov.viewRadius = i.CurrentGun.viewRadius;
        fovmesh.Setup();
        fovmesh.UpdateMesh();

        IngameHUDManager.Instance.UpdateAmmo();
    }
    #endregion

    #region ClientRPCs

    [ClientRpc]
    private void RpcReload()
    {
        PlaySound(GameManager.Instance.reloadSound);
    }

    [ClientRpc]
    private void RpcAfterShoot()
    {
        ParticleManager.Spawn(EParticleType.SHOOT, rotateTransform);
        PlaySound(GameManager.Instance.gunshot);
    }

    [ClientRpc]
    internal void RpcOnPlayerShot(Vector3 pos)
    {
        PlaySound(GameManager.Instance.hurtSound);
        ParticleManager.Spawn(EParticleType.BLOOD, pos);
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

    [TargetRpc]
    public void TargetTeleport(Vector3 newPos)
    {
        transform.position = newPos;
    }

    #endregion

    private void PlaySound(AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }
    private bool Rotate()
    {
        Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion r = rotateTransform.rotation;
        rotateTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        return r != rotateTransform.rotation;
    }

    #endregion

    #region Server

    public void Setup(AuthRequestMessage data)
    {
        info = new PlayerInformation() { Nickname = data.nick };
        name = $"Player {data.nick}";
    }

    [Server]
    internal void Respawn()
    {
        isAlive = true;
        RpcOnPlayerRespawned();
    }

    [Command]
    private void CmdReload()
    {
        int zaladowane = Mathf.Min(i.CurrentGun.magazineCapacity - i.CurrentGunData.currentAmmo, i.CurrentGunData.totalAmmo);

        GunData gd = i.CurrentGunData;

        gd.totalAmmo -= zaladowane;
        gd.currentAmmo += zaladowane;

        i.CurrentGunData = gd;

        RpcReload();
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

        RpcAfterShoot();

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
        RpcOnPlayerShot(transform.position);
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

public struct PlayerInformation : NetworkMessage
{
    public string Nickname;
    public int killCount;
    public int deathCount;
}