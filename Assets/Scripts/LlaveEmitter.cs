using UnityEngine;

public class LlaveEmitter : EchoEmitter
{

    [Header("Echo")]
    [SerializeField] private EchoEmitter echoEmitter;
    [SerializeField] private EchoEmitter puertaEmitter;
    [SerializeField] private GameObject pared;

    void Start()
    {
        // Ejecuta "MiFuncion" después de 2 segundos,
        // y luego la repite cada 1 segundo, para siempre
        InvokeRepeating(nameof(Destello), 4f, 3f);
    }

    void Destello()
    {
        echoEmitter.Emit(echoEmitter.SmallEcho, transform.position);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        puertaEmitter.enabled = true;
        Destroy(pared);
        Destroy(gameObject);
        
    }
}
