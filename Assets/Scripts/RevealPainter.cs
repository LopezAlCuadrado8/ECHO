using System.Collections.Generic;
using UnityEngine;

public class RevealPainter : MonoBehaviour
{
    public static RevealPainter Instance { get; private set; }

    [Header("Render Texture")]
    [SerializeField] private RenderTexture revealRT;

    [Header("Brush")]
    [SerializeField] private Material brushMaterial;
    [SerializeField] private Camera worldCamera;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private struct PaintCommand
    {
        public Vector2 worldPos;
        public float radius;
        public Color color;
    }

    private readonly List<PaintCommand> pendingPaints = new();
    private Material _runtimeMaterial;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (revealRT == null)
        {
            Debug.LogError("[RevealPainter] revealRT NO asignada. Nada se pintará.");
            return;
        }
        if (brushMaterial == null)
        {
            Debug.LogError("[RevealPainter] brushMaterial NO asignado. Nada se pintará.");
            return;
        }
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
            if (worldCamera == null)
            {
                Debug.LogError("[RevealPainter] worldCamera NO asignada y no hay MainCamera.");
                return;
            }
        }

        // Material runtime (no modifica el asset)
        _runtimeMaterial = new Material(brushMaterial);

        // Limpiamos la RT al iniciar
        ClearRT();
    }

    private void ClearRT()
    {
        var prev = RenderTexture.active;
        RenderTexture.active = revealRT;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;
    }

    /// <summary>
    /// Encola un blob permanente. Se dibujará al final del frame.
    /// </summary>
    public void PaintPermanent(Vector2 worldPos, float radius, Color color)
    {
        pendingPaints.Add(new PaintCommand
        {
            worldPos = worldPos,
            radius = radius,
            color = color
        });
    }

    /// <summary>
    /// Compatibilidad: mismo comportamiento que PaintPermanent.
    /// </summary>
    public void PaintTemporal(Vector2 worldPos, float radius, Color color)
    {
        PaintPermanent(worldPos, radius, color);
    }

    private void LateUpdate()
    {
        if (pendingPaints.Count == 0) return;
        if (revealRT == null || _runtimeMaterial == null) return;

        // Configuramos una sola vez para todos los blobs del frame
        GL.PushMatrix();
        GL.LoadOrtho();
        RenderTexture.active = revealRT;

        _runtimeMaterial.SetPass(0);
        // ¿Se mantiene lo pintado?
        if (debugLogs)
        {
            RenderTexture.active = revealRT;
            Texture2D test = new Texture2D(1, 1);
            test.ReadPixels(new Rect(512, 288, 1, 1), 0, 0);
            test.Apply();
            Debug.Log($"[RevealPainter] ANTES de pintar: píxel central = {test.GetPixel(0, 0)}");
            Destroy(test);
        }

        GL.Begin(GL.QUADS);

        for (int i = 0; i < pendingPaints.Count; i++)
        {
            var cmd = pendingPaints[i];
            DrawBlob(cmd);
        }

        GL.End();
        if (debugLogs)
        {
            Texture2D test = new Texture2D(1, 1);
            test.ReadPixels(new Rect(512, 288, 1, 1), 0, 0);
            test.Apply();
            Debug.Log($"[RevealPainter] DESPUÉS de pintar: píxel central = {test.GetPixel(0, 0)}");
            Destroy(test);
        }
        GL.PopMatrix();
        RenderTexture.active = null;

        if (debugLogs)
            Debug.Log($"[RevealPainter] Dibujados {pendingPaints.Count} blobs este frame.");

        pendingPaints.Clear();
    }

    private void DrawBlob(PaintCommand cmd)
    {
        Vector3 vp = worldCamera.WorldToViewportPoint(cmd.worldPos);

        // Descartamos blobs fuera del viewport (con margen)
        if (vp.x < -0.5f || vp.x > 1.5f || vp.y < -0.5f || vp.y > 1.5f) return;
        if (vp.z < 0) return;   // detrás de la cámara

        // Tamaño del quad en viewport units (cuadrado, radio * 2)
        float halfSize = cmd.radius;

        _runtimeMaterial.SetColor("_Color", cmd.color);
        _runtimeMaterial.SetFloat("_Softness", 0.35f);

        float cx = vp.x;
        float cy = vp.y;

        GL.TexCoord2(0, 0); GL.Vertex3(cx - halfSize, cy - halfSize, 0);
        GL.TexCoord2(1, 0); GL.Vertex3(cx + halfSize, cy - halfSize, 0);
        GL.TexCoord2(1, 1); GL.Vertex3(cx + halfSize, cy + halfSize, 0);
        GL.TexCoord2(0, 1); GL.Vertex3(cx - halfSize, cy + halfSize, 0);
    }

    /// <summary>
    /// Limpia la máscara (llamar al cambiar de nivel).
    /// </summary>
    public void ResetMask()
    {
        pendingPaints.Clear();
        ClearRT();
    }
}