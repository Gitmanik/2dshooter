using Gitmanik.FOV2D;
using Gitmanik.Notification;
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Player : NetworkBehaviour, Target
{
    public static Player Local;

    [Header("Component references")]
    [SerializeField] private IngameHUDManager hudman;
    [SerializeField] private FOVMesh fovmesh;
    [SerializeField] private TMP_Text nickText;
    [HideInInspector] public NetworkIdentity identity;
    private Rigidbody2D rb;
    private AudioSource source;
    private Transform rotateTransform;

    [Header("Server-owned variables")]
    [SyncVar(hook = nameof(UpdateNick))] public string nickname;
    [SyncVar(hook = nameof(OnChangedHealth))] public float health = -1f;
    private Dictionary<int, GunData> gunPlayerInventory = new Dictionary<int, GunData>();


    [Header("Client-owned variables")]
    [Tooltip("GameObjects to be deleted on remote cli ent.")]
    public float speed;
    [SerializeField] private Transform[] toRemove;
    private float shootDelay;
    private Vector3 change = Vector3.zero;

    private Gun gunInstance
    {
        get
        {
            if (NetworkServer.active)
                return GameManager.Instance.Guns[serverGunIndex];
            else
                return GameManager.Instance.Guns[localPlayerGunData.gunIndex];
        }
    }

    private int serverGunIndex;

    private GunData serverPlayerGunData { get => gunPlayerInventory[serverGunIndex]; set => gunPlayerInventory[serverGunIndex] = value; }
    private GunData localPlayerGunData;

    private int lastGunIndex = -1;

    #region MonoBehaviour

    private void Start()
    {
        identity = GetComponent<NetworkIdentity>();
        source = GetComponent<AudioSource>();
        rotateTransform = transform.GetChild(0);

        if (!hasAuthority)
        {
            foreach (Transform b in toRemove)
            {
                DestroyImmediate(b.gameObject);
            }
            return;
        }
        rb = GetComponent<Rigidbody2D>();

        Local = this;
        CameraFollow.instance.targetTransform = transform;
        CmdSetGun(0);
        hudman.UpdateHealth(health);
    }

    private void Update()
    {
        if (!hasAuthority)
            return;

        shootDelay -= Time.deltaTime;

        if (GameManager.Instance.LockInput)
            return;

        if (shootDelay <= 0f && Input.GetKey(KeyCode.Mouse0) && localPlayerGunData.currentAmmo > 0)
        {
            shootDelay = 1f / gunInstance.firerate;
            CmdShoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && localPlayerGunData.totalAmmo > 0)
            CmdReload();

        if (Input.GetKeyDown(KeyCode.T))
            GunSelector.Instance.Enable();

        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        if (!hasAuthority || GameManager.Instance.LockInput)
            return;

        Vector3 newPos = change.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + newPos);

        if (Rotate() || newPos.x != 0 || newPos.y != 0)
            fovmesh.UpdateMesh();
    }

    #endregion

    #region Client

    #region SyncVar events
    private void UpdateNick(string _, string __) => nickText.text = nickname;
    private void OnChangedHealth(float _, float __)
    {
        if (hasAuthority)
            hudman.UpdateHealth(health);
    }
    #endregion

    #region TargetRPCs
    [TargetRpc]
    private void TargetOnPlayerRespawned()
    {
    }
    [TargetRpc]
    private void TargetChangedGun(InventoryMessage newinv)
    {
        localPlayerGunData = newinv.slot1;
        if (lastGunIndex != newinv.slot1.gunIndex)
            shootDelay = 0f;

        lastGunIndex = newinv.slot1.gunIndex;
        fovmesh.fov.viewAngle = gunInstance.viewAngle;
        fovmesh.fov.viewRadius = gunInstance.viewRadius;
        fovmesh.UpdateMesh();

        hudman.UpdateAmmo(gunInstance, localPlayerGunData);

    }

    [TargetRpc]
    private void TargetAmmoUpdate(InventoryMessage newinv)
    {
        localPlayerGunData = newinv.slot1;
        hudman.UpdateAmmo(gunInstance, localPlayerGunData);
    }

    [TargetRpc]
    private void TargetTeleport(Vector3 position)
    {
        transform.position = position;
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
        ParticleManager.Spawn(EParticleType.BLOOD, pos);
    }

    [ClientRpc]
    internal void RpcOnPlayerDied(GameObject x)
    {
        Player playerKiller = x.GetComponent<Player>();
        string killer = x.name;
        if (playerKiller != null)
            killer = playerKiller.nickname;
        NotificationManager.Spawn($"{killer} > {nickname}", new Color(0, 0, 0, 0.8f));
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
        nickname = data.nick;
        name = $"Player {data.nick}";
    }

    [Command] public void CmdSetGun(int g) => SrvSetGun(g);

    [Server]
    public void SrvSetGun(int g)
    {
        serverGunIndex = g;
        if (!gunPlayerInventory.ContainsKey(g))
        {
            serverPlayerGunData = new GunData
            {
                totalAmmo = gunInstance.magazineCapacity * (gunInstance.magazineCount - 1),
                currentAmmo = gunInstance.magazineCapacity,
                gunIndex = g
            };
        }
        TargetChangedGun(new InventoryMessage() { slot1 = serverPlayerGunData });
    }

    [Command]
    private void CmdReload()
    {
        int zaladowane = Mathf.Min(gunInstance.magazineCapacity - serverPlayerGunData.currentAmmo, serverPlayerGunData.totalAmmo);

        GunData gd = serverPlayerGunData;

        gd.totalAmmo -= zaladowane;
        gd.currentAmmo += zaladowane;

        serverPlayerGunData = gd;

        TargetAmmoUpdate(new InventoryMessage() { slot1 = serverPlayerGunData });
        RpcReload();
    }

    [Command]
    private void CmdShoot()
    {
        GunData gd = serverPlayerGunData;
        gd.currentAmmo--;
        serverPlayerGunData = gd;

        RpcAfterShoot();
        TargetAmmoUpdate(new InventoryMessage() { slot1 = serverPlayerGunData });

        RaycastHit2D hit = Physics2D.Raycast(rotateTransform.position, rotateTransform.right, 99f);
        Target t;
        if (hit.collider != null)
        {
            t = hit.transform.GetComponent<Target>();
            t?.Damage(gameObject, gunInstance.damageCurve.Evaluate(hit.distance) * gunInstance.damage);
        }
    }

    [Server]
    public void Damage(GameObject x, float damage)
    {
        RpcOnPlayerShot(transform.position);
        health -= damage;
        if (health <= 0f)
        {
            RpcOnPlayerDied(x);
            Server_Respawn();
        }
    }

    [Server]
    private void Server_Respawn()
    {
        health = 100;
        transform.position = NetworkManager.singleton.GetStartPosition().position;
        gunPlayerInventory.Clear();
        SrvSetGun(0);
        TargetOnPlayerRespawned();
    }
    #endregion

}