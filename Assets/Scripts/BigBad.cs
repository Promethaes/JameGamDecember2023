using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBad : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.gameObject.GetComponentInParent<Player>();
        if (player == null)
            return;
        //play jumpscare animation
        GameManager.instance.OnKilled.Invoke();
    }
}
