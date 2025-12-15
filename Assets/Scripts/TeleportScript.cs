using UnityEngine;

public class TeleportScript : MonoBehaviour
{
    private GameObject teleportDestination;

    public void Teleport()
    {
        teleportDestination = GameObject.FindGameObjectWithTag("Respawn");
        if (teleportDestination != null)
            gameObject.transform.position = teleportDestination.transform.position;
    }
}