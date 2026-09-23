using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Visual")]
    [Tooltip("Color del eco cuando un objeto cae en este hazard.")]
    public Color hazardEchoColor = new Color(1f, 0.2f, 0.2f);

    [Tooltip("Color del sprite al revelarse (para destacar peligro).")]
    public Color revealedSpriteColor = new Color(1f, 0.4f, 0.4f);

    [Header("Efectos")]
    public bool killPlayerOnContact = true;
    public float echoOnPlayerContact = 1.5f;

    [Header("Refs")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnValidate()
    {
        // Aplica el color de peligro en el editor para verlo
        if (spriteRenderer != null) spriteRenderer.color = revealedSpriteColor;
    }

    public void OnHitByThrowable(Throwable throwable, Vector2 point)
    {
        // Feedback opcional: partículas, sonido, etc.
        Debug.Log($"[Hazard] {throwable.name} cayó en {name} en {point}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!killPlayerOnContact) return;

        if (other.CompareTag("Player"))
        {
            // Emitimos un eco de "muerte" desde el hazard
            EchoEmitter emitter = FindFirstObjectByType<EchoEmitter>();
            if (emitter != null)
            {
                emitter.EmitHazardEcho(transform.position, hazardEchoColor);
            }

            // TODO: matar al player / reiniciar nivel
            Debug.Log("[Hazard] ¡Player ha muerto!");
        }
    }
}