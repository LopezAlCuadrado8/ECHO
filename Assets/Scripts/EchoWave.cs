using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EchoWave : MonoBehaviour
{
    [Header("Wave Settings")]
    public int rayCount = 64;              // más rayos = onda más suave
    public float maxDistance = 15f;
    public float expandSpeed = 12f;
    public float currentRadius = 0f;
    public int maxBounces = 3;
    public float energyLossPerBounce = 0.5f;

    [Header("Reveal")]
    public Color revealColor = Color.white;
    public bool permanentReveal = true;
    public LayerMask obstacleMask;
    public float blobRadiusAtImpact = 1.2f;

    [Header("Visual")]
    public float visualLineWidth = 0.08f;
    public AnimationCurve falloffCurve = AnimationCurve.Linear(0, 1, 1, 0);

    private LineRenderer line;
    private float distanceTravelled = 0f;
    private float maxRadius;
    private bool finished = false;
    private float blobRadiusMultiplier = 1f;

    private readonly List<Vector3> points = new();
    private readonly List<Vector3> directions = new();
    private readonly List<Vector3> origins = new();
    private readonly List<float> energies = new();

    public void Initialize(Vector2 origin, float maxRadius, int bounces, Color color, bool permanent, float blobMult= 1f)
    {
        transform.position = origin;
        this.maxRadius = maxRadius;
        this.maxBounces = bounces;
        this.revealColor = color;
        this.permanentReveal = permanent;
        this.blobRadiusMultiplier = blobMult;

        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
        line.startWidth = visualLineWidth;
        line.endWidth = visualLineWidth;
        line.useWorldSpace = true;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;

        // Inicializamos rayos en todas las direcciones
        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * Mathf.PI * 2f / rayCount;
            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            directions.Add(dir);
            origins.Add(origin);
            energies.Add(1f);
        }

    }

    private void Update()
    {
        if (finished) return;

        distanceTravelled += expandSpeed * Time.deltaTime;
        currentRadius = distanceTravelled;

        // Actualizamos el visual de la onda (círculo)
        DrawWaveVisual();

        // A cierta distancia, hacemos los raycasts para revelar
        // (no cada frame para no spamear el painter)
        if (Time.frameCount % 3 == 0)
        {
            CastEchoRays();
        }

        if (currentRadius >= maxRadius)
        {
            Finish();
        }
    }

    private void CastEchoRays()
    {
        for (int i = 0; i < directions.Count; i++)
        {
            if (energies[i] <= 0.05f) continue;

            Vector3 origin = origins[i];
            Vector3 dir = directions[i];
            float remaining = maxRadius - currentRadius;

            if (remaining <= 0) continue;

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, remaining, obstacleMask);

            if (hit.collider != null)
            {
                // ¡Impacto! Revelamos un blob en el punto de impacto
                RevealAt(hit.point, energies[i]);

                // Generamos un eco secundario (rebote)
                if (maxBounces > 0)
                {
                    Vector3 reflectDir = Vector3.Reflect(dir, hit.normal);
                    SpawnSubEcho(hit.point, reflectDir, energies[i] * energyLossPerBounce);
                }

                // Este rayo muere aquí
                energies[i] = 0;
            }
            else
            {
                // El rayo llegó al final sin chocar: revelamos un blob tenue al final
                Vector3 end = origin + dir * remaining;
                RevealAt(end, energies[i] * 0.4f);
            }
        }
    }

    private void RevealAt(Vector3 point, float energy)
    {
        if (energy <= 0.05f) return;

        float radius = blobRadiusAtImpact * energy * blobRadiusMultiplier;
        Color c = revealColor * energy;
        c.a = 1f;

        // Pintamos en la máscara correspondiente
        if (permanentReveal)
            RevealPainter.Instance.PaintPermanent(point, radius, c);
        else
            TemporalBlobTracker.Instance.AddBlob(point, radius, c);
    }

    private void SpawnSubEcho(Vector3 origin, Vector3 dir, float energy)
    {
        // En lugar de crear un GameObject nuevo (costoso), 
        // podríamos añadir un rayo secundario a este mismo EchoWave.
        // Para claridad ahora, creamos un sub-eco ligero:
        StartCoroutine(SubEchoRoutine(origin, dir, energy));
    }

    private System.Collections.IEnumerator SubEchoRoutine(Vector3 origin, Vector3 dir, float energy)
    {
        // Simulamos un eco que viaja hasta chocar (rebote)
        float dist = maxRadius * 0.6f;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, obstacleMask);

        float travelTime = 0.08f;
        yield return new WaitForSeconds(travelTime);

        if (hit.collider != null)
        {
            RevealAt(hit.point, energy);
        }
        else
        {
            RevealAt(origin + dir * dist, energy * 0.4f);
        }
    }

    private void DrawWaveVisual()
    {
        int segs = 48;
        line.positionCount = segs + 1;
        for (int i = 0; i <= segs; i++)
        {
            float a = i * Mathf.PI * 2f / segs;
            Vector3 p = transform.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * currentRadius;
            line.SetPosition(i, p);
        }

        // Fade del visual según el radio
        float t = Mathf.Clamp01(currentRadius / maxRadius);
        Color c = revealColor;
        c.a = falloffCurve.Evaluate(t);
        line.startColor = c;
        line.endColor = c;
    }

    private void Finish()
    {
        finished = true;
        Destroy(gameObject, 0.2f);
    }
}