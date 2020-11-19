﻿using Gitmanik.FOV2D;
using Gitmanik.Notification;
using Mirror;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerInventory))]
public class Player : NetworkBehaviour, Target
{
    public static Player Local;

    [Header("Component references")]
    [SerializeField] private IngameHUDManager hudman;
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

    [Header("Client-owned variables")]
    public float speed;
    [SerializeField] private Transform[] nonLocal;
    [SerializeField] private Transform[] onDied;
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

        if (!hasAuthority)
        {
            foreach (Transform b in nonLocal)
            {
                DestroyImmediate(b.gameObject);
            }
            return;
        }

        i.OnSelectedSlot += OnSelectedSlot;
        i.OnSlotUpdate += OnSlotUpdate;

        Local = this;
        hudman.Setup(this);

        CameraFollow.instance.targetTransform = transform;
    }


    private void Update()
    {
        if (!hasAuthority)
            return;

        shootDelay -= Time.deltaTime;

        if (GameManager.Instance.LockInput || !isAlive)
            return;

        if (shootDelay <= 0f && Input.GetKey(KeyCode.Mouse0) && i.CurrentGunData.currentAmmo > 0)
        {
            shootDelay = 1f / i.CurrentGun.firerate;
            CmdShoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && i.CurrentGunData.totalAmmo > 0)
            CmdReload();

        if (Input.GetKeyDown(KeyCode.T))
            GunSelector.Instance.Enable();

        if (Input.GetKeyDown(KeyCode.Escape))
            GameManager.Instance.ToggleOptionsMenu(true);

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
    private void OnSetInfo(PlayerInformation oldinfo, PlayerInformation newinfo)
    {
        nickText.text = info.Nickname;
    }
    private void OnChangedHealth(float _, float __)
    {
        if (hasAuthority)
            hudman.UpdateHealth();
    }
    #endregion

    #region TargetRPCs


    private void OnSelectedSlot()
    {
        shootDelay = 0f;

        fovmesh.fov.viewAngle = i.CurrentGun.viewAngle;
        fovmesh.fov.viewRadius = i.CurrentGun.viewRadius;
        fovmesh.Setup();
        fovmesh.UpdateMesh();

        hudman.UpdateAmmo();
    }

    private void OnSlotUpdate()
    {
        if (i.inventory.Count == 0)
            return;

        hudman.UpdateAmmo();
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
        CameraFollow.instance.smooth = false;
        string killer = x.name;
        if (playerKiller != null)
            killer = playerKiller.info.Nickname;
        NotificationManager.Spawn($"{killer} > {info.Nickname}", new Color(0, 0, 0, 0.8f), 5f);
        foreach(Transform toDisable in onDied)
        {
            toDisable.gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void RpcOnPlayerRespawned()
    {
        NotificationManager.Spawn($"{info.Nickname} respawned!", new Color(0, 1, 0, 0.8f), 2f);
        CameraFollow.instance.smooth = true;
        shootDelay = 0f;
        foreach (Transform toDisable in onDied)
        {
            toDisable.gameObject.SetActive(true);
        }
    }

    [TargetRpc]
    private void TargetTeleport(Vector3 newPos)
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
    public void Damage(GameObject x, float damage)
    {
        RpcOnPlayerShot(transform.position);
        health -= damage;
        if (health <= 0f)
        {
            isAlive = false;
            RpcOnPlayerDied(x);
            Invoke(nameof(Server_Respawn), 2.5f);
        }
    }

    [Server]
    private void Server_Respawn()
    {
        health = 100;
        TargetTeleport(NetworkManager.singleton.GetStartPosition().position);
        i.ResetInventory();
        isAlive = true;
        RpcOnPlayerRespawned();
    }
    #endregion

}