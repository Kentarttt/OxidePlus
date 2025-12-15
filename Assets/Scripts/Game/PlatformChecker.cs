using UnityEngine;
using UnityEngine.Events;

class PlatformChecker : MonoBehaviour
{
    [SerializeField] UnityEvent isPC, isMobile;
    private void Awake()
    {
        if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
            isPC.Invoke();
        else if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            isMobile.Invoke();
    }
}