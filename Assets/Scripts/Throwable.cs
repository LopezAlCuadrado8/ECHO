using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Throwable : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Si está vacío, se buscará un EchoEmitter en el player o en la escena como fallback.")]
    [SerializeField] private EchoEmitter echoEmitter;

    [Header("Física")]
    [SerializeField] private float gravityScale = 2f;
    [SerializeField] private float impactVelocityThreshold = 1.5f;
    [SerializeField] private int maxBounces = 2;

    [Header("Asentamiento")]
    [SerializeField] private float minAirborneTime = 0.25f;
    [SerializeField] private float settleVelocityThreshold = 0.1f;
    [SerializeField] private float settleDelay = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private ThrowableData data;
    private Rigidbody2D rb;
    private int bouncesLeft;
    private float lifetimeTimer;
    private float airborneTimer;
    private float settleTimer;
    private bool hasEmittedFinalEcho = false;
    private bool isSettled = false;
    private bool hasLaunched = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // ─── Fallback: si no hay echoEmitter local, buscamos uno global ───
        if (echoEmitter == null)
        {
            echoEmitter = FindFirstObjectByType<EchoEmitter>();
            if (echoEmitter == null)
            {
                Debug.LogWarning($"[Throwable] {name}: No se encontró ningún EchoEmitter en la escena. Los objetos no revelarán.");
            }
            else if (debugLogs)
            {
                Debug.Log($"[Throwable] {name}: Usando EchoEmitter global de {echoEmitter.gameObject.name}");
            }
        }
    }

    public void Launch(ThrowableData throwableData, Vector2 velocity)
    {
        data = throwableData;

        if (data == null)
        {
            Debug.LogError($"[Throwable] {name}: Launch llamado con data null.");
            return;
        }

        // Física
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = gravityScale;
        rb.mass = data.mass;
        rb.linearDamping = data.linearDamping;
        rb.angularDamping = data.angularDamping;
        rb.WakeUp();

        rb.linearVelocity = velocity;
        rb.angularVelocity = Random.Range(-360f, 360f);

        // Visual
        if (spriteRenderer != null && data.sprite != null)
            spriteRenderer.sprite = data.sprite;

        // Reset estado
        bouncesLeft = maxBounces;
        lifetimeTimer = data.maxLifetime;
        airborneTimer = 0f;
        settleTimer = 0f;
        hasEmittedFinalEcho = false;
        isSettled = false;
        hasLaunched = true;

        // Eco inicial en el punto de lanzamiento
        EmitEcho(transform.position, 0.5f);
    }

    private void Update()
    {
        if (data == null || !hasLaunched) return;

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        airborneTimer += Time.deltaTime;
        if (airborneTimer < minAirborneTime) return;
        if (isSettled) return;

        if (rb.linearVelocity.sqrMagnitude < settleVelocityThreshold * settleVelocityThreshold)
        {
            settleTimer += Time.deltaTime;
            if (settleTimer >= settleDelay)
                SettleDown();
        }
        else
        {
            settleTimer = 0f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (data == null || isSettled) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < impactVelocityThreshold) return;

        // ¿Es un Hazard?
        var hazard = collision.collider.GetComponent<Hazard>();
        if (hazard == null)
            hazard = collision.collider.GetComponentInParent<Hazard>();

        if (hazard != null)
        {
            OnHitHazard(hazard, collision.GetContact(0).point);
            return;
        }

        Vector2 impactPoint = collision.GetContact(0).point;

        if (bouncesLeft > 0)
        {
            bouncesLeft--;
            EmitEcho(impactPoint, 0.7f);
        }
        else
        {
            EmitEcho(impactPoint, 1f);
            hasEmittedFinalEcho = true;
        }
    }

    private void OnHitHazard(Hazard hazard, Vector2 point)
    {
        EmitEcho(point, 1.2f, hazard.hazardEchoColor);
        hazard.OnHitByThrowable(this, point);
        Destroy(gameObject);
    }

    private void EmitEcho(Vector2 point, float energyMultiplier, Color? overrideColor = null)
    {
        if (echoEmitter == null)
        {
            Debug.LogWarning($"[Throwable] {name}: No hay EchoEmitter. No se emite eco.");
            return;
        }

        if (data == null)
        {
            Debug.LogWarning($"[Throwable] {name}: data es null en EmitEcho.");
            return;
        }

        EchoPreset preset = echoEmitter.GetPresetBySize(data.echoSize);
        if (preset == null)
        {
            Debug.LogWarning($"[Throwable] {name}: preset null para tamaño {data.echoSize}");
            return;
        }

        // Aplicamos el multiplicador de energía al radio
        var finalPreset = new EchoPreset
        {
            maxRadius = preset.maxRadius * energyMultiplier,
            bounces = preset.bounces,
            color = overrideColor ?? preset.color,
            permanent = preset.permanent,
            blobRadiusMultiplier = preset.blobRadiusMultiplier
        };

        if (debugLogs)
        {
            Debug.Log($"[Throwable] {name}: Emitiendo eco {data.echoSize} en {point} | radius={finalPreset.maxRadius} | perm={finalPreset.permanent} | color={finalPreset.color}");
        }

        echoEmitter.Emit(finalPreset, point);
    }

    private void SettleDown()
    {
        isSettled = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (!hasEmittedFinalEcho)
        {
            EmitEcho(transform.position, 0.8f);
            hasEmittedFinalEcho = true;
        }
    }
}