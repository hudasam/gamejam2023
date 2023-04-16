using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using UnityEngine;
using UnityEngine.Scripting;

public class StarrtMusic : MonoBehaviour
{
   private static readonly int s_play = Animator.StringToHash("Play");
   [Preserve]
   public void StartMusic()
   {
      GetComponent<AudioSource>().Play();
      GetComponent<Animator>().SetBool(s_play, true);
   }
}
