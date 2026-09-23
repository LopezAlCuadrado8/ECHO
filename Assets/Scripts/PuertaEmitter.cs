using UnityEngine;

public class PuertaEmitter : EchoEmitter
{

    [SerializeField] private EchoEmitter echoEmitter;
    private void OnEnable()
    {
        InvokeRepeating(nameof(Destello), 2f, 3f);
    }


    void Destello()
    {
        echoEmitter.Emit(echoEmitter.SmallEcho, transform.position);
    }
}
