using UnityEngine;

public class RevealPainter : MonoBehaviour
{
    public static RevealPainter Instance { get; private set; }

    [Header("Render Texture")]
    [SerializeField] private RenderTexture revealRT;

    [Header("Brush")]
    [SerializeField] private Material brushMaterial; // Material con shader "Echo/RevealBrush"
    [SerializeField] private Camera worldCamera;

    private Material _tempBrushMaterial;
    private Material _fadeMaterial;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Limpiamos la RT al iniciar (todo negro = todo oscuro)
        ClearRT();

        _tempBrushMaterial = new Material(brushMaterial);
        _fadeMaterial = new Material(Shader.Find("Echo/RevealBrush"));
    }

    private void ClearRT()
    {
        var prev = RenderTexture.active;
        RenderTexture.active = revealRT;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;
    }

    /// <summary>
    /// Dibuja un blob permanente en la máscara.
    /// </summary>
    public void PaintPermanent(Vector2 worldPos, float radius, Color color)
    {
        Paint(worldPos, radius, color, _tempBrushMaterial);
    }

    /// <summary>
    /// Dibuja un blob temporal. Se desvanecerá con el tiempo.
    /// </summary>
    public void PaintTemporal(Vector2 worldPos, float radius, Color color)
    {
        Paint(worldPos, radius, color, _tempBrushMaterial);
    }

    private void Paint(Vector2 worldPos, float radius, Color color, Material mat)
    {
        // Convertimos world → viewport → UV de la RT
        Vector3 vp = worldCamera.WorldToViewportPoint(worldPos);
        if (vp.x < -0.2f || vp.x > 1.2f || vp.y < -0.2f || vp.y > 1.2f) return;

        // Tamaño del brush en UV (viewport units), compensando el aspect ratio
        float aspect = (float)revealRT.width / revealRT.height;
        Vector2 sizeUV = new Vector2(radius * 2f, radius * 2f);

        // Escala del brush en NDC: del viewport (0-1) al rango (-1,1) que espera Graphics.Blit-like
        float scaleX = sizeUV.x;
        float scaleY = sizeUV.y;

        mat.SetColor("_Color", color);
        mat.SetFloat("_Softness", 0.35f);

        GL.PushMatrix();
        GL.LoadOrtho();
        RenderTexture.active = revealRT;

        mat.SetPass(0);
        GL.Begin(GL.QUADS);

        // Quad centrado en la posición del mundo (en coordenadas viewport)
        float cx = vp.x;
        float cy = vp.y;

        GL.TexCoord2(0, 0); GL.Vertex3(cx - scaleX * 0.5f, cy - scaleY * 0.5f, 0);
        GL.TexCoord2(1, 0); GL.Vertex3(cx + scaleX * 0.5f, cy - scaleY * 0.5f, 0);
        GL.TexCoord2(1, 1); GL.Vertex3(cx + scaleX * 0.5f, cy + scaleY * 0.5f, 0);
        GL.TexCoord2(0, 1); GL.Vertex3(cx - scaleX * 0.5f, cy + scaleY * 0.5f, 0);

        GL.End();
        GL.PopMatrix();

        RenderTexture.active = null;

        // Si es temporal, programamos su fade
        // (lo gestiona el TemporalBlobTracker, no aquí)
    }
}