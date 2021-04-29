using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace myTest
{
    
    public class PlayerCamera : MonoBehaviourPun
    {
        private float verticalLook;
        private float horizontalLook;
        private Camera camera;


        bool isFollowing;
        private void Awake() {
            if (!photonView.IsMine) return;
            camera = GetComponent<Camera>();
            isFollowing = true;
        }
        private void Update()
        {
            if (!photonView.IsMine) return;

            float lookY = Input.GetAxis("Look Y");
            float lookX = Input.GetAxis("Look X");
            camera = GetComponent<Camera>();

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