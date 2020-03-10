using Mirror;

public class CameraController : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            gameObject.SetActive(false);
        }
    }
}
