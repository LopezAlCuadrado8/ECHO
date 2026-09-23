using UnityEngine;

public class BotonMenu : MonoBehaviour
{
    [SerializeField] GameObject parent;
    [SerializeField] GameObject controles;
    [SerializeField] GameObject playerFab;
    public void onClick()
    {
        Instantiate(playerFab);
        controles.SetActive(true);
        Destroy(parent);
    }

}
