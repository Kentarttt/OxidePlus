using Mirror;
using UnityEngine;
using UnityEngine.Events;

class LocalPlayer : NetworkBehaviour
{
    [SerializeField] private UnityEvent IsLocalPlayer, IsNotLocalPlayer;

    private void Start() => Check();
    
    private void Check()
    {
        if (isLocalPlayer) IsLocalPlayer.Invoke();
        else IsNotLocalPlayer.Invoke();
    }
}