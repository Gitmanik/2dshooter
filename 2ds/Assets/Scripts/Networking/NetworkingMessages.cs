using Mirror;

public class InventoryMessage : NetworkMessage
{
    public GunData slot1;
}

public struct GunData
{
    public int gunIndex;
    public int totalAmmo;
    public int currentAmmo;
}

public class AuthRequestMessage : NetworkMessage
{
    public string nick;
    public int version;
}

public class AuthResponseMessage : NetworkMessage
{
    public bool success;
    public string message;
}