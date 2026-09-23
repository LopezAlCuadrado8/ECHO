using UnityEngine;

public class DestelloInicial : EchoEmitter
{

    [Header("Echo")]
    [SerializeField] private EchoEmitter echoEmitter;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Emitimos desde los pies, no desde el centro
        echoEmitter.Emit(echoEmitter.MediumEcho, transform.position);
    }
}
