using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientCamera : NetworkBehaviour
{
    Vector2 mouseLook;
    Vector2 smoothedLook;

    [Header("Input")]
    [SerializeField]
    private float sensitivity = 1.0f;
    [SerializeField]
    private float smoothing = 2.0f;
    [Header("Settings")]
    [SerializeField]
    [Tooltip("How far up the camera should be from the root of the player.")]
    private Vector3 headOffset = new Vector3(0f, 0.8f, 0f);

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        Vector2 direction = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        direction = Vector2.Scale(direction, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
        smoothedLook.x = Mathf.Lerp(smoothedLook.x, direction.x, 1f / smoothing);
        smoothedLook.y = Mathf.Lerp(smoothedLook.y, direction.y, 1f / smoothing);

        mouseLook += smoothedLook;
        mouseLook.y = Mathf.Clamp(mouseLook.y, -90f, 90f);

        Camera.main.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, Vector3.up) * Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        Camera.main.transform.localPosition = gameObject.transform.position + headOffset;

        gameObject.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, gameObject.transform.up);
    }
}
