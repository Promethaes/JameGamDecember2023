using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Lights Settings")]
    [SerializeField] AnimationCurve badLightCurve;

    [Header("Light Colours")]
    [SerializeField] Color badColor;
    [SerializeField] Color goodColor;

    [Header("Lights")]
    [SerializeField] List<Light> allLights = new();
    [SerializeField] Transform lightsParent;

    [Header("Events")]
    public UnityEvent OnAllCaptured;
    public UnityEvent<int> OnCaptured;
    public UnityEvent OnKilled;
    public UnityEvent OnBigBad;
    public UnityEvent OnReturnToNormal;
    public UnityEvent OnPlayerHide;
    public UnityEvent OnPlayerReturn;
    public static GameManager instance;

    public bool BigBadActive = false;

    public int numCaptured = 0;

    float badLightU = 0.0f;
    private void Awake()
    {
        instance = this;

        for (int i = 0; i < lightsParent.childCount; i++)
        {
            var light = lightsParent.GetChild(i).GetComponent<Light>();
            allLights.Add(light);
        }

        OnBigBad.AddListener(() =>
        {
            BigBadActive = true;
            allLights.ForEach(x => x.color = badColor);
        });
        OnReturnToNormal.AddListener(() =>
        {
            BigBadActive = false;
            allLights.ForEach(x =>
            {
                x.color = goodColor;
            });
        });
        OnAllCaptured.AddListener(() => SceneManager.LoadScene(0));
        OnKilled.AddListener(WaitThenDie);
    }

    private void WaitThenDie()
    {
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(1.0f);
            SceneManager.LoadScene(0);
        }
        StartCoroutine(Wait());
    }

    private void Update()
    {
        if (!BigBadActive)
            return;
        badLightU += Time.deltaTime;
        if (badLightU >= 1.0f)
            badLightU = 0.0f;
        allLights.ForEach(x =>
        {
            x.color = new Color(badLightCurve.Evaluate(badLightU), x.color.g, x.color.b, x.color.a);
        });
    }

    public void OnSuccessfulCapture()
    {
        numCaptured++;
        OnCaptured.Invoke(numCaptured);

        if (numCaptured == 8)
        {
            OnAllCaptured.Invoke();
        }
    }
}