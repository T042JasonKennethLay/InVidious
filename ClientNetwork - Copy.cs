using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetwork : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
