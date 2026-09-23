using UnityEngine;

public class PasarNivel : MonoBehaviour
{
    [SerializeField] GameObject pasarNivel;
    [SerializeField] GameObject controles;
    [SerializeField] RevealPainter reveal;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        pasarNivel.SetActive(true);
        controles.SetActive(false);
        reveal.ResetMask();
        Destroy(gameObject);
    }
}
