using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCreep : Creep
{
    [SerializeField] protected bool isSummoner;
    [SerializeField] protected GameObject summonCreepPrefab;
    [SerializeField] protected int summonCount = 1;
    [SerializeField] protected float summonInterval = 3f;
    private Vector3 lastPosition;
    private float summonTimer = 0f;

    protected override void Update()
    {
        base.Update();

        lastPosition = transform.position;
        if (!isSummoner || summonCreepPrefab == null) return;
        summonTimer += Time.deltaTime;
        if (summonTimer >= summonInterval)
        {
            summonTimer = 0f;
            StartCoroutine(SpawnSummons());
        }
    }

    private IEnumerator SpawnSummons()
    {
        for (int i = 0; i < summonCount; i++)
        {
            GameObject summon = Instantiate(summonCreepPrefab);
            summon.transform.parent = MasterController.Instance.CreepParent;

            Creep summonCreep = summon.GetComponent<Creep>();
            summonCreep.SetValves(pathToFollow, splineCompletion);
            summonCreep.pathProgress = pathProgress;
            summonCreep.pathIndex = pathIndex;
            summon.transform.position = lastPosition;

            MasterController.Instance.IncrementEnemies();
            yield return new WaitForSeconds(0.2f);
        }
    }
}
