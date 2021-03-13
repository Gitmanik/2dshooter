using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Gitmanik.Multiplayer.Inventory
{
    public class PlayerInventory : NetworkBehaviour
    {
        public readonly SyncDictionary<int, GunData> inventory = new SyncDictionary<int, GunData>();

        public UnityAction OnSelectedSlot;
        public UnityAction OnSlotUpdate;

        [SyncVar(hook = nameof(SlotChanged))] public int CurrentSlot = 0;
        public GunData CurrentGunData
        {
            get
            {
                if (inventory.ContainsKey(CurrentSlot))
                    return inventory[CurrentSlot];
                else return new GunData { gunIndex = -1 };
            }
            set => inventory[CurrentSlot] = value;
        }
        public Gun CurrentGun
        {
            get
            {
                return CurrentGunData.gunIndex == -1 ? null : GameManager.Instance.Guns[CurrentGunData.gunIndex];
            }
        }

        public bool HasAnyGun => inventory.Count > 0 && inventory.ContainsKey(CurrentSlot);

        public override void OnStartServer()
        {
            inventory.Clear();
            inventory[0] = GameManager.Instance.Guns[0].GenerateGunData();
            inventory[1] = GameManager.Instance.Guns[1].GenerateGunData();

            CurrentSlot = 0;
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
            OnSelectedSlot?.Invoke();
        }

        public void ResetInventory()
        {
            CurrentSlot = 0;
            OnStartServer();
        }
    }

    public abstract class IIntentoryItem
    {
        string itemName;
        string itemDescription;

        void Use()
        {
            Debug.Log($"Used {itemName}");
        }
    }
}