using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BigBad : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Animator badAnimator;
    public AudioSource jumpscareSound;

    private void Start()
    {
        IEnumerator Despawn()
        {
            yield return new WaitForSeconds(15.0f);
            gameObject.SetActive(false);
            GameManager.instance.OnReturnToNormal.Invoke();
        }
        StartCoroutine(Despawn());
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.gameObject.GetComponentInParent<Player>();
        if (player == null || player.IsHiding)
            return;

        //play jumpscare animation
        navMeshAgent.isStopped = true;
        badAnimator.SetTrigger("Jumpscare");
        jumpscareSound.Play();
        GameManager.instance.OnKilled.Invoke();
    }
}
