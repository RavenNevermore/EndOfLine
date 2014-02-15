using UnityEngine;
using System.Collections;

public class FakeItemBoxBehavior : MonoBehaviour
{
    public int playerIndex = 0;   // Player index of driver who spawned fake item box

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        stream.Serialize(ref this.playerIndex);
    }
}
