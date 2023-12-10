using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    [SerializeField] AudioSource footstep;

    public void PlayFootstep()
    {
        footstep.Play();
    }

}
