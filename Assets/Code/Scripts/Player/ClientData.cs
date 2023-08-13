using Mirror;
using System;

[Serializable]
public class ClientData {
    public int connectionId = 0;
    public string displayName = "Unknown Player";
    public string playerColor = "White";
    public int score = 0;

    public ClientData(int connectionId) {
        this.connectionId = connectionId;
    }
}
