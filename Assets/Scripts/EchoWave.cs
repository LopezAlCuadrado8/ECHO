using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EchoWave : MonoBehaviour
{
    [Header("Wave Settings")]
    [Tooltip("Número de puntos alrededor del anillo. Más = onda más suave, más costoso.")]
    public int samples = 32;
    [Tooltip("Velocidad de expansión de la onda (unidades/segundo).")]
    public float expandSpeed = 12f;

    [Header("Reveal")]
    public Color revealColor = Color.white;
    public bool permanentReveal = true;
    [Tooltip("Capas que bloquean la onda (paredes, hazards).")]
    public LayerMask obstacleMask;
    [Tooltip("Radio del blob que pinta cada punto del frente de onda.")]
    public float frontBlobRadius = 0.2f;
    [Tooltip("Cada cuántas muestras pintamos también el relleno interno.")]
    public int fillEveryNSamples = 2;

    [Header("Visual")]
    public float visualLineWidth = 0.08f;
    public AnimationCurve falloffCurve = AnimationCurve.Linear(0, 1, 1, 0);

    // Runtime
    private LineRenderer line;
    private float distanceTravelled = 0f;
    private float maxRadius;
    private float currentRadius = 0f;
    private float blobRadiusMultiplier = 1f;
    private bool finished = false;
    private bool initialized = false;

    public void Initialize(Vector2 origin, float maxRadius, int bounces, Color color, bool permanent, float blobMult = 1f)
    {
        transform.position = origin;

        this.maxRadius = maxRadius;
        this.revealColor = color;
        this.permanentReveal = permanent;
        this.blobRadiusMultiplier = blobMult;

        // LineRenderer
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
        line.startWidth = visualLineWidth;
        line.endWidth = visualLineWidth;
        line.useWorldSpace = true;

        // Material del LineRenderer (necesita existir en build)
        if (line.material == null || line.material.shader == null)
        {
            line.material = new Material(Shader.Find("Sprites/Default"));
        }
        line.startColor = color;
        line.endColor = color;

        // Reset estado
        distanceTravelled = 0f;
        currentRadius = 0f;
        finished = false;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || finished) return;

        distanceTravelled += expandSpeed * Time.deltaTime;
        currentRadius = distanceTravelled;

        DrawWaveVisual();
        PaintWaveFront();

        if (currentRadius >= maxRadius)
            Finish();
    }

    private void PaintWaveFront()
    {
        if (RevealPainter.Instance == null && TemporalBlobTracker.Instance == null)
        {
            // Ni siquiera hay painter. No pintamos nada.
            return;
        }

        float angleStep = Mathf.PI * 2f / samples;

        for (int i = 0; i < samples; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 origin = transform.position;

            // ¿Hay pared antes de llegar al radio actual?
            Vector2 point;
            float energy = 1f;

            if (obstacleMask != 0)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, currentRadius, obstacleMask);
                if (hit.collider != null)
                {
                    point = hit.point;
                    energy = 0.6f;
                }
                else
                {
                    point = origin + dir * currentRadius;
                }
            }
            else
            {
                // Sin máscara de obstáculos, la onda pasa a través de todo
                point = origin + dir * currentRadius;
            }

            // Blob del frente (anillo visible)
            Color c = revealColor * energy;
            c.a = 1f;
            float frontRadius = frontBlobRadius * blobRadiusMultiplier;
            PaintAt(point, frontRadius, c);

            // Relleno interno tenue (cada N muestras)
            if (fillEveryNSamples > 0 && i % fillEveryNSamples == 0 && currentRadius > 0.8f)
            {
                Vector2 innerPoint = origin + dir * (currentRadius * 0.5f);
                Color innerColor = revealColor * 0.25f;
                innerColor.a = 1f;
                float innerRadius = currentRadius * 0.3f;
                PaintAt(innerPoint, innerRadius, innerColor);
            }
        }
    }

    private void PaintAt(Vector2 point, float radius, Color color)
    {
        if (permanentReveal)
        {
            if (RevealPainter.Instance != null)
                RevealPainter.Instance.PaintPermanent(point, radius, color);
        }
        else
        {
            if (TemporalBlobTracker.Instance != null)
                TemporalBlobTracker.Instance.AddBlob(point, radius, color);
        }
    }

    private void DrawWaveVisual()
    {
        if (line == null) return;

        int segs = 48;
        line.positionCount = segs + 1;

        for (int i = 0; i <= segs; i++)
        {
            float a = i * Mathf.PI * 2f / segs;
            Vector3 p = transform.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * currentRadius;
            line.SetPosition(i, p);
        }

        // Fade visual según el radio
        float t = Mathf.Clamp01(currentRadius / Mathf.Max(0.01f, maxRadius));
        Color c = revealColor;
        c.a = falloffCurve.Evaluate(t);
        line.startColor = c;
        line.endColor = c;
    }

    private void Finish()
    {
        finished = true;

        // Un último frame de pintado para cerrar el círculo
        PaintWaveFront();

        Destroy(gameObject, 0.05f);
    }
}