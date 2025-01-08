using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePanelController : MonoBehaviour
{
    public Animator fadePanelAnim;
    public Animator gameInfoAnim;
    public static FadePanelController instance;

    public void Play(){
        if(fadePanelAnim != null && gameInfoAnim != null){
            fadePanelAnim.SetBool("Out", true);
            gameInfoAnim.SetBool("Out", true);
        }
    }
}
