using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Infection
{
    public class PostProcessing : MonoBehaviour
    {
        PostProcessVolume m_Volume;
        Vignette m_Vignette;
        Grain m_Grain;
        ChromaticAberration m_ChromaticAberration;

        void Start()
        {
            m_Volume = GetComponent<PostProcessVolume>();
            m_Volume.profile.TryGetSettings(out m_Vignette);
            m_Volume.profile.TryGetSettings(out m_Grain);
            m_Volume.profile.TryGetSettings(out m_ChromaticAberration);
        }

        private void LateUpdate()
        {
            if (Player.localPlayer)
            {
                // Survivor Post-Processing
                if (Player.localPlayer.team == Player.Team.Survivor)
                {
                    float parameter = Mathf.InverseLerp(Player.localPlayer.health, Player.localPlayer.healthMax, Player.localPlayer.healthMax - Player.localPlayer.health);

                    m_Vignette.color.Override(Color.red);
                    m_Vignette.intensity.Override(parameter * 0.5f);
                    m_Grain.intensity.Override(0f);
                    m_ChromaticAberration.intensity.Override(0f);

                }
                // Infected Post-Processing
                else if (Player.localPlayer.team == Player.Team.Infected)
                {
                    m_Vignette.color.Override(Color.green);
                    m_Vignette.intensity.Override(0.25f);
                    m_Grain.intensity.Override(0.2f);
                    m_ChromaticAberration.intensity.Override(0.2f);
                }
            }
        }
    }
}
