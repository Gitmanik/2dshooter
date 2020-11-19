using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : NetworkBehaviour
{
    public readonly SyncDictionary<int, GunData> inventory = new SyncDictionary<int, GunData>();

    public UnityAction OnSelectedSlot;
    public UnityAction OnSlotUpdate;

    [SyncVar(hook = nameof(SlotChanged))] public int CurrentSlot = 0;
    public GunData CurrentGunData { get => inventory[CurrentSlot]; set => inventory[CurrentSlot] = value; }
    public Gun CurrentGun => GameManager.Instance.Guns[CurrentGunData.gunIndex];

    public bool HasAnyGun => inventory.Count > 0;

    public override void OnStartServer()
    {
        //inventory.Clear();
        inventory[0] = GameManager.Instance.Guns[0].GenerateGunData();
        inventory[1] = GameManager.Instance.Guns[1].GenerateGunData();
    }

    public override void OnStartClient()
    {
        inventory.Callback += OnInventoryUpdate;
    }

    private void OnInventoryUpdate(SyncIDictionary<int, GunData>.Operation op, int key, GunData item)
    {
        OnSlotUpdate?.Invoke();
    }

    [Command]
    public void CmdSelectSlot(int slot)
    {
        CurrentSlot = slot;
    }

    public void SlotChanged(int oldslot, int newslot)
    {
        Debug.Log($"{name} changed slot from {oldslot} to {newslot}");
        OnSelectedSlot?.Invoke();
    }

    public void ResetInventory()
    {
        CurrentSlot = 0;
        OnStartServer();
    }
}