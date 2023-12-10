using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource _goodAmbience;
    [SerializeField] AudioSource _badAmbience;
    // Start is called before the first frame update
    void Start()
    {
        _goodAmbience.Play();
        GameManager.instance.OnBigBad.AddListener(OnBigBad);
        GameManager.instance.OnReturnToNormal.AddListener(OnReturnToNormal);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnBigBad()
    {
        _goodAmbience.Stop();
        _badAmbience.Play();
    }

    public void OnReturnToNormal()
    {
        _goodAmbience.Play();
        _badAmbience.Stop();
    }
}
