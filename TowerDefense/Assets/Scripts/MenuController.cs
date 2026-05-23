using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] Transform dedicatedTowerSpot, creepOrganizer;
    [SerializeField] GameObject basicTower;
    [SerializeField] GameObject basicCreep;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject obj = Instantiate(basicTower);
        obj.transform.SetParent(dedicatedTowerSpot, false);
        obj.GetComponent<Tower>().PlaceTower(dedicatedTowerSpot.position);
        StartCoroutine(CreepLoop());
    }

    IEnumerator CreepLoop()
    {
        WaitForSeconds delay = new(1.05f);
        while (true)
        {
            GameObject newCreep = Instantiate(basicCreep);
            newCreep.GetComponent<Creep>().SetValves(new List<int>() { 0 }, 0);
            newCreep.transform.SetParent(creepOrganizer);
            yield return delay;
        }
    }

    public void OpenFirstLevel()
    {
        SceneManager.LoadScene(1);
    }
}
