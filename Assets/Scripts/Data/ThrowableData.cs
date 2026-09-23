using UnityEngine;

[CreateAssetMenu(fileName = "Throwable_", menuName = "Echo/Throwable Data")]
public class ThrowableData : ScriptableObject
{
    [Header("Identidad")]
    public string displayName = "Piedra pequeña";
    public Sprite sprite;
    public ThrowableSize size = ThrowableSize.Small;

    [Header("Física")]
    public float mass = 0.5f;
    public float throwForceMultiplier = 1f;   // pequeño=vuela lejos, grande=vuela cerca
    public float linearDamping = 0.1f;
    public float angularDamping = 0.5f;

    [Header("Eco al impactar")]
    [Tooltip("Preset de eco que se emite al chocar. Se elige por tamaño.")]
    public EchoSize echoSize = EchoSize.Small;

    [Header("Feedback")]
    public Color impactColor = Color.white;
    public float impactFlashDuration = 0.2f;

    [Header("Vida")]
    public float maxLifetime = 8f;   // se autodestruye si no impacta
}

public enum ThrowableSize { Small, Medium, Large }
public enum EchoSize { Small, Medium, Large }