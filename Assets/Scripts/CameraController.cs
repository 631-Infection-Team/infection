using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    Vector2 mouseLook;
    Vector2 smoothedLook;

    [Header("Input")]
    public float sensitivity = 1.0f;
    public float smoothing = 2.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer) { return; }

        Vector2 direction = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        direction = Vector2.Scale(direction, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
        smoothedLook.x = Mathf.Lerp(smoothedLook.x, direction.x, 1f / smoothing);
        smoothedLook.y = Mathf.Lerp(smoothedLook.y, direction.y, 1f / smoothing);

        mouseLook += smoothedLook;
        mouseLook.y = Mathf.Clamp(mouseLook.y, -90f, 90f);

        Camera.main.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, Vector3.up) * Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        Camera.main.transform.localPosition = gameObject.transform.position;

        gameObject.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, gameObject.transform.up);
    }
}
