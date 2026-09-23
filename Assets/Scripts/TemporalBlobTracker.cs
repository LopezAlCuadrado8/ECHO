using System.Collections.Generic;
using UnityEngine;

public class TemporalBlobTracker : MonoBehaviour
{
    public static TemporalBlobTracker Instance { get; private set; }

    [SerializeField] private RenderTexture temporalRT;
    [SerializeField] private Material temporalBrushMaterial;
    [SerializeField] private Camera worldCamera;

    [SerializeField] private float defaultLifetime = 1.5f;
    [SerializeField] private float fadeDuration = 0.8f;

    private class TempBlob
    {
        public Vector2 worldPos;
        public float radius;
        public Color color;
        public float spawnTime;
        public float lifetime;
    }

    private readonly List<TempBlob> blobs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var prev = RenderTexture.active;
        RenderTexture.active = temporalRT;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;
    }

    public void AddBlob(Vector2 worldPos, float radius, Color color, float lifetime = -1f)
    {
        blobs.Add(new TempBlob
        {
            worldPos = worldPos,
            radius = radius,
            color = color,
            spawnTime = Time.time,
            lifetime = lifetime < 0 ? defaultLifetime : lifetime
        });
    }

    private void LateUpdate()
    {
        // Limpiamos la RT temporal
        var prev = RenderTexture.active;
        RenderTexture.active = temporalRT;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;

        // Redibujamos cada blob con alpha según su edad
        for (int i = blobs.Count - 1; i >= 0; i--)
        {
            var b = blobs[i];
            float age = Time.time - b.spawnTime;
            if (age >= b.lifetime + fadeDuration) { blobs.RemoveAt(i); continue; }

            float fadeT = Mathf.Clamp01(1f - (age - b.lifetime) / fadeDuration);
            if (age > b.lifetime) fadeT = 1f - (age - b.lifetime) / fadeDuration;
            fadeT = Mathf.Clamp01(fadeT);

            Color c = b.color * fadeT;
            DrawBlob(b.worldPos, b.radius, c);
        }
    }

    private void DrawBlob(Vector2 worldPos, float radius, Color color)
    {
        Vector3 vp = worldCamera.WorldToViewportPoint(worldPos);
        if (vp.x < -0.3f || vp.x > 1.3f || vp.y < -0.3f || vp.y > 1.3f) return;

        float scale = radius * 2f;

        temporalBrushMaterial.SetColor("_Color", color);
        temporalBrushMaterial.SetFloat("_Softness", 0.35f);

        GL.PushMatrix();
        GL.LoadOrtho();
        RenderTexture.active = temporalRT;
        temporalBrushMaterial.SetPass(0);
        GL.Begin(GL.QUADS);

        float cx = vp.x, cy = vp.y;
        GL.TexCoord2(0, 0); GL.Vertex3(cx - scale * 0.5f, cy - scale * 0.5f, 0);
        GL.TexCoord2(1, 0); GL.Vertex3(cx + scale * 0.5f, cy - scale * 0.5f, 0);
        GL.TexCoord2(1, 1); GL.Vertex3(cx + scale * 0.5f, cy + scale * 0.5f, 0);
        GL.TexCoord2(0, 1); GL.Vertex3(cx - scale * 0.5f, cy + scale * 0.5f, 0);

        GL.End();
        GL.PopMatrix();
        RenderTexture.active = null;
    }
}