using UnityEngine;
public class InteractuarObjetos : MonoBehaviour
{
    [SerializeField] private Transform controladorInteractuar;
    [SerializeField] private Vector2 dimensionesCaja;
    [SerializeField] private LayerMask capasInteractuables;
    private void Update()
    {
        if (Input.GetButtonDown("Interactuar"))
        {
            Interactuar();
        }
    }
    private void Interactuar()
    {
        Collider2D[] objetosTocados = Physics2D.OverlapBoxAll(controladorInteractuar.position, dimensionesCaja, 0f, capasInteractuables);
        foreach (Collider2D objeto in objetosTocados)
        {
            if (objeto.TryGetComponent(out Vendas vendas)) //añadir más como estos si queremos tener llaves o mas objetos
            {
                vendas.Interactuar();
            }
        }
    }
}
    /*void OnDrawGizmos()//esto es para ver el area azul, se puede borrar
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(controladorInteractuar.position, dimensionesCaja);
    }
}
    */