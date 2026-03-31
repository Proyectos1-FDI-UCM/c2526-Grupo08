using UnityEngine;

public class BossSummonAbility : MonoBehaviour
{
    public GameObject minionPrefab; 
    public Transform spawnPointL; 
    public Transform spawnPointR; 

    
    public void SummonMinions()
    {
        if (minionPrefab != null)
        {
            Instantiate(minionPrefab, spawnPointL.position, Quaternion.identity);
            Instantiate(minionPrefab, spawnPointR.position, Quaternion.identity);
            Debug.Log("¡Esbirros invocados!");
        }
    }

   
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            SummonMinions();
        }
    }
}