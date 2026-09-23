using UnityEngine;

public class EchoEmitter : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject echoWavePrefab;

    [Header("Presets por tamaño")]
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
        blobRadiusMultiplier = 1.5f
    };
    [SerializeField]
    private EchoPreset largeEcho = new EchoPreset
    {
        maxRadius = 13f,
        bounces = 4,
        color = new Color(0.5f, 0.65f, 1f),
        permanent = true,
        blobRadiusMultiplier = 2.5f
    };
    [SerializeField]
    private EchoPreset footstepEcho = new EchoPreset
    {
        maxRadius = 0.8f,
        bounces = 0,
        color = new Color(1f, 0.95f, 0.8f),
        permanent = false,
        blobRadiusMultiplier = 0.5f
    };
    [SerializeField]
    private EchoPreset hazardEcho = new EchoPreset
    {
        maxRadius = 6f,
        bounces = 1,
        color = new Color(1f, 0.2f, 0.2f),
        permanent = true,
        blobRadiusMultiplier = 1.5f
    };

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public EchoPreset SmallEcho => smallEcho;
    public EchoPreset MediumEcho => mediumEcho;
    public EchoPreset LargeEcho => largeEcho;
    public EchoPreset FootstepEcho => footstepEcho;
    public EchoPreset HazardEcho => hazardEcho;

    private void Awake()
    {
        if (echoWavePrefab == null)
        {
            Debug.LogError($"[EchoEmitter] {name}: 'echoWavePrefab' NO está asignado. Los ecos no se instanciarán.");
        }
    }

    public EchoPreset GetPresetBySize(EchoSize size)
    {
        EchoPreset preset = size switch
        {
            EchoSize.Small => smallEcho,
            EchoSize.Medium => mediumEcho,
            EchoSize.Large => largeEcho,
            _ => smallEcho
        };

        if (preset == null)
        {
            Debug.LogWarning($"[EchoEmitter] {name}: preset para tamaño {size} es null. Devolviendo fallback.");
            preset = new EchoPreset
            {
                maxRadius = 5f,
                bounces = 1,
                color = Color.white,
                permanent = true,
                blobRadiusMultiplier = 1f
            };
        }

        return preset;
    }

    public void Emit(EchoPreset preset, Vector2 position)
    {
        if (preset == null)
        {
            Debug.LogWarning($"[EchoEmitter] {name}: Emit llamado con preset null.");
            return;
        }

        if (echoWavePrefab == null)
        {
            Debug.LogError($"[EchoEmitter] {name}: echoWavePrefab es null, no se puede emitir.");
            return;
        }

        if (preset.maxRadius <= 0.01f)
        {
            Debug.LogWarning($"[EchoEmitter] {name}: preset.maxRadius = {preset.maxRadius}, la onda no se verá.");
        }

        var go = Instantiate(echoWavePrefab, position, Quaternion.identity);
        var wave = go.GetComponent<EchoWave>();

        if (wave == null)
        {
            Debug.LogError($"[EchoEmitter] {name}: El prefab {echoWavePrefab.name} no tiene componente EchoWave.");
            return;
        }

        wave.Initialize(position, preset.maxRadius, preset.bounces, preset.color, preset.permanent, preset.blobRadiusMultiplier);

    }

    public void EmitHazardEcho(Vector2 position, Color color)
    {
        var preset = new EchoPreset
        {
            maxRadius = hazardEcho.maxRadius,
            bounces = hazardEcho.bounces,
            color = color,
            permanent = hazardEcho.permanent,
            blobRadiusMultiplier = hazardEcho.blobRadiusMultiplier
        };
        Emit(preset, position);
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