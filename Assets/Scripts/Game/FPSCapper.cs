using UnityEngine;

public class FPSCapper : MonoBehaviour
{
    public int targetFrameRate = 120;

    private void Awake() 
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate; 
    }
}
