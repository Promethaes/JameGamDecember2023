using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureBox : MonoBehaviour
{
    [HideInInspector] public Player playerRef;
    private void OnCollisionEnter(Collision other)
    {
        playerRef.OnHitMonster(other.gameObject);
        Destroy(gameObject);
    }
}
