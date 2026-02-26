using System;
using UnityEngine;
public class InteractuarObjetos : MonoBehaviour
{
    [SerializeField] private Transform controladorInteractuar;
    [SerializeField] private Vector2 dimensionesCaja;
    [SerializeField] private LayerMask capasInteractuables;

    //private bool _picked = false;
    void Update()
    {
        if (Input.GetButtonDown("Interactuar"))
        {
            Interactuar();
        }
    }
    
    public void Interactuar()
    // public bool Interactuar() lo cambié de void a bool porque sino no me dejaba ponerlo como condición de recogida
    {
        Collider2D[] objetosTocados = Physics2D.OverlapBoxAll(controladorInteractuar.position, dimensionesCaja, 0f, capasInteractuables);
        foreach (Collider2D objeto in objetosTocados)
        {
            if (objeto.TryGetComponent(out Vendas vendas)) //añadir más como estos si queremos tener llaves o mas objetos
            {
                vendas.Interactuar();
                //_picked = true;
            }
        }

        //return _picked;
    }

}
    /*void OnDrawGizmos()//esto es para ver el area azul, se puede borrar
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(controladorInteractuar.position, dimensionesCaja);
    }
}
    */