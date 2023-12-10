using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Monster monsterPrefab;
    [SerializeField] List<Transform> monsterTravelLocations = new();
    List<Monster> monsters = new();
    private void Start()
    {
        SpawnMonsters();

        GameManager.instance.OnBigBad.AddListener(() =>
        {
            monsters.Where(x => !x.IsBigBad).ToList().ForEach(x => x.gameObject.SetActive(false));
        });
        GameManager.instance.OnReturnToNormal.AddListener(() =>
        {
            monsters.Where(x => !x.IsBigBad).ToList().ForEach(x => x.gameObject.SetActive(true));
        });
    }

    private void RemoveCheck()
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i] == null || monsters[i].RemoveMe)
            {
                monsters.Remove(monsters[i]);
                i--;
            }
        }
    }

    private void SpawnMonsters()
    {
        IEnumerator WaitThenSpawn()
        {
            yield return new WaitForSeconds(20.0f);
            for (int i = 0; i < 8; i++)
            {
                var monster = Instantiate(monsterPrefab);
                monsters.Add(monster);
                int index = Random.Range(0, monsterTravelLocations.Count);
                monster.transform.position = monsterTravelLocations[index].position;
            }
            StartUpdateMonsterDestinations();
            StartBigBadCheck();
            yield return null;
        }
        StartCoroutine(WaitThenSpawn());
    }

    private void StartUpdateMonsterDestinations()
    {
        IEnumerator UpdateMonsterDestinations()
        {
            bool first = true;
            while (true)
            {
                if (first)
                {
                    yield return null;
                    first = false;
                }
                else
                    yield return new WaitForSeconds(10.0f);

                RemoveCheck();
                monsters.Where(x => !x.IsCaptured).ToList().ForEach(x =>
                {
                    int index = Random.Range(0, monsterTravelLocations.Count);
                    x.navMeshAgent.SetDestination(monsterTravelLocations[index].position);
                });
            }
        }
        StartCoroutine(UpdateMonsterDestinations());
    }

    private void StartBigBadCheck()
    {
        IEnumerator BigBadCheck()
        {
            while (true)
            {
                yield return new WaitForSeconds(3.0f);

                RemoveCheck();

                if (GameManager.instance.BigBadActive)
                    continue;

                monsters.Where(x => !x.IsCaptured).ToList().ForEach(x => x.IsBigBad = false);
                int index = Random.Range(0, monsters.Count);
                monsters[index].IsBigBad = true;
            }
        }

        StartCoroutine(BigBadCheck());
    }
}