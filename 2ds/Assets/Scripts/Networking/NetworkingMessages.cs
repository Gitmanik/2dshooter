using Mirror;

public struct PlayerInformation : NetworkMessage
{
    public string Nickname;
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