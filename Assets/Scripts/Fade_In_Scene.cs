/*
*   This script is used between 01_Menu and 02_Prototype
*   Fade In from white to imitate Scene transition. 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade_In_Scene : MonoBehaviour{
    public Image White_Fade_In;

    // Start is called before the first frame update
    void Start(){
        White_Fade_In.canvasRenderer.SetAlpha(1.0f);
        fade_in_scene();
        
    }

    // Update is called once per frame
    void fade_in_scene(){
        White_Fade_In.CrossFadeAlpha(0, 3, false);
    }
}
