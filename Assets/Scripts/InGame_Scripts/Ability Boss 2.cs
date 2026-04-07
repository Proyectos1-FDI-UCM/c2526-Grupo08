using UnityEngine;
using UnityEngine.Rendering;

public class BossSummonAbility : MonoBehaviour
{
    [SerializeField] private GameObject minionPrefab; 
    [SerializeField] private Transform spawnPointL; 
    [SerializeField] private Transform spawnPointR;
    [SerializeField] private float spawnTime = 15;

    private float nextSpawnTime = 0f;

    public void SummonMinions()
    {
        if (minionPrefab != null)
        {
            Instantiate(minionPrefab, spawnPointL.position, Quaternion.identity);
            Instantiate(minionPrefab, spawnPointR.position, Quaternion.identity);
            Debug.Log("¡Esbirros invocados!");
        }
    }

   
    public void CallMinions()
    {
        if (Time.time >= nextSpawnTime)
        {
            SummonMinions();
            nextSpawnTime = Time.time + spawnTime;
        }
    }
   
    
    private void Update()
    {
        CallMinions();
    }
    
}