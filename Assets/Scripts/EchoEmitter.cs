using UnityEngine;

public class EchoEmitter : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject echoWavePrefab;

    [Header("Presets por tamaño de objeto")]
    [SerializeField]
    private EchoPreset smallEcho = new EchoPreset
    {
        maxRadius = 4f,
        bounces = 1,
        color = new Color(0.7f, 0.85f, 1f),
        permanent = true,
        blobRadiusMultiplier = 1f
    };
    [SerializeField]
    private EchoPreset mediumEcho = new EchoPreset
    {
        maxRadius = 8f,
        bounces = 2,
        color = new Color(0.6f, 0.75f, 1f),
        permanent = true,
        blobRadiusMultiplier = 1f
    };
    [SerializeField]
    private EchoPreset largeEcho = new EchoPreset
    {
        maxRadius = 13f,
        bounces = 4,
        color = new Color(0.5f, 0.65f, 1f),
        permanent = true,
        blobRadiusMultiplier = 1f
    };
    [SerializeField]
    private EchoPreset footstepEcho = new EchoPreset
    {
        maxRadius = 3f,
        bounces = 0,
        color = new Color(1f, 0.95f, 0.8f),
        permanent = false,
        blobRadiusMultiplier = 0.5f
    };

    public EchoPreset SmallEcho => smallEcho;
    public EchoPreset MediumEcho => mediumEcho;
    public EchoPreset LargeEcho => largeEcho;
    public EchoPreset FootstepEcho => footstepEcho;

    public void Emit(EchoPreset preset, Vector2 position)
    {
        var go = Instantiate(echoWavePrefab, position, Quaternion.identity);
        var wave = go.GetComponent<EchoWave>();
        wave.Initialize(position, preset.maxRadius, preset.bounces, preset.color, preset.permanent, preset.blobRadiusMultiplier);
    }
}

[System.Serializable]
public class EchoPreset
{
    public float maxRadius = 5f;
    public int bounces = 1;
    public Color color = Color.white;
    public bool permanent = true;
    public float blobRadiusMultiplier = 1f;

}