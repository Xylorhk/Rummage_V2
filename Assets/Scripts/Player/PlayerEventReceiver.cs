using UnityEngine;

// sends messages from animated model back to player controller
public class PlayerEventReceiver : MonoBehaviour
{
    public PlayerController playerController;

    public void OnThrowComplete()
    {
        playerController.OnThrowComplete();
    }

    public void OnThrowSpawnGrenade()
    {
        //playerController.OnThrowSpawnGrenade();
    }
}
