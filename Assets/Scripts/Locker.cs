using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locker : MonoBehaviour
{
    [SerializeField] Camera _camera;

    public void OnEnterLocker()
    {
        _camera.gameObject.SetActive(true);
    }

    public void OnExitLocker()
    {
        _camera.gameObject.SetActive(false);
    }
}
