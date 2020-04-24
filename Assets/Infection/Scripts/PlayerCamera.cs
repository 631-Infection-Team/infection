using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Infection
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Player))]
    public class PlayerCamera : NetworkBehaviour
    {
        private float verticalLook;
        private float horizontalLook;

        private void Update()
        {
            if (!isLocalPlayer) return;

            float lookY = Input.GetAxis("Look Y");
            float lookX = Input.GetAxis("Look X");
            Camera camera = GetComponent<Player>().camera;

            verticalLook -= lookY;
            if (verticalLook > 90f) verticalLook = 90f;
            if (verticalLook < -90f) verticalLook = -90f;

            Vector3 currentAngles = camera.transform.localEulerAngles;
            currentAngles.x = verticalLook;

            camera.transform.localEulerAngles = currentAngles;

            horizontalLook += lookX;
            if (horizontalLook > 360) horizontalLook -= 360.0f;
            if (horizontalLook < 0) horizontalLook += 360.0f;

            currentAngles = transform.localEulerAngles;
            currentAngles.y = horizontalLook;

            transform.localEulerAngles = currentAngles;
        }
    }
}