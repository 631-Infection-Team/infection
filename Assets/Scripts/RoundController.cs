using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Infection
{
    public class RoundController : NetworkBehaviour
    {
        private float startTime;
        private float timePassed;
        private float roundLength = 10f;
        public void Start()
        {
            startTime = Time.timeSinceLevelLoad;    
        }
        public void Update()
        {
            timePassed = Time.timeSinceLevelLoad - startTime;

            Debug.Log(timePassed);
            if (timePassed >= roundLength)
            {
                Debug.Log("Round over!");
            }
        }

        public override void OnStartServer()
        {

        }

        public override void OnStartClient()
        {
            
        }
    }
}