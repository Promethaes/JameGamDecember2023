using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] AnimationCurve scaleBounceCurve;
    [SerializeField] AnimationCurve rotationJiggleCurve;
    [SerializeField] float _bigBadSpeed = 3.0f;

    [Header("References")]
    public NavMeshAgent navMeshAgent;
    public List<Mesh> possibleMeshes = new();
    [Header("Box")]
    public Mesh boxMesh;
    public Material boxMaterial;

    [Header("Big Bad")]
    public GameObject bigBadObject;
    public Material bigBadMaterial;

    [Header("Audio")]
    [SerializeField] List<AudioSource> cuteSounds = new();
    [SerializeField] AudioSource badSound;
    [SerializeField] AudioSource captureSound;

    public bool IsBigBad = false;
    public bool IsCaptured = false;

    public bool RemoveMe = false;

    float scaleBounceU = 0.0f;
    bool animate = true;

    Mesh regularSharedMesh;

    bool BigBad => IsBigBad && GameManager.instance.BigBadActive;
    // Start is called before the first frame update
    void OnEnable()
    {
        int index = Random.Range(0, possibleMeshes.Count);
        GetComponent<MeshFilter>().sharedMesh = possibleMeshes[index];
        regularSharedMesh = possibleMeshes[index];
        StartPlayMonsterSounds();
    }

    private void StartPlayMonsterSounds()
    {
        IEnumerator PlayMonsterSounds()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(1.0f, 3.5f));
                if (BigBad)
                    badSound.Play();
                else
                {
                    int index = Random.Range(0, cuteSounds.Count);
                    cuteSounds[index].Play();
                }
            }
        }

        StartCoroutine(PlayMonsterSounds());
    }

    // Update is called once per frame
    void Update()
    {
        Animate();

        if (BigBad)
        {
            navMeshAgent.SetDestination(FindObjectOfType<Player>().transform.position);
        }

    }

    private void Animate()
    {
        scaleBounceU += Time.deltaTime;
        if (scaleBounceU >= 1.0f)
            scaleBounceU = 0.0f;

        if (!IsCaptured && animate)
            transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.75f, 1.25f, 0.75f), scaleBounceCurve.Evaluate(scaleBounceU));
        else if (IsCaptured)
            transform.rotation = Quaternion.Lerp(Quaternion.AngleAxis(15, transform.right), Quaternion.AngleAxis(-15, transform.right), rotationJiggleCurve.Evaluate(scaleBounceU));
    }

    public void OnCapture()
    {
        IsCaptured = true;
        navMeshAgent.isStopped = true;
        GetComponent<MeshFilter>().sharedMesh = boxMesh;
        GetComponent<MeshRenderer>().sharedMaterial = boxMaterial;
        captureSound.Play();

        IEnumerator WaitThenSurprise()
        {
            yield return new WaitForSeconds(3.0f);
            if (IsBigBad)
            {
                IsCaptured = false;
                bigBadObject.SetActive(true);
                GetComponent<MeshRenderer>().enabled = false;
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = _bigBadSpeed;
                navMeshAgent.acceleration = 10.0f;
                GameManager.instance.OnBigBad.Invoke();
                animate = false;
                transform.localScale = Vector3.one;
            }
            else
            {
                RemoveMe = true;
                Remove();
            }
        }
        StartCoroutine(WaitThenSurprise());
        GameManager.instance.OnSuccessfulCapture();

    }

    public void Remove()
    {
        //play dust vfx
        IEnumerator WaitThenDie()
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }
        StartCoroutine(WaitThenDie());
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!IsBigBad)
            return;
        var player = other.gameObject.GetComponent<Player>();
        if (player == null)
            return;
        //play jumpscare animation
        GameManager.instance.OnKilled.Invoke();
    }
}
