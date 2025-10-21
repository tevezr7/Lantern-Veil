using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class LanternBeamTexture : MonoBehaviour
{
    [Header("Beam Shape")]
    [Range(0.05f, 0.95f)] public float beamWidthFraction = 0.40f; // base width (% of rect width)
    [Range(0.00f, 0.50f)] public float featherFraction = 0.18f; // softness at the edges
    [Range(0.10f, 1.00f)] public float alphaPeak = 0.85f; // center opacity

    [Header("Dynamics")]
    public bool animate = true;
    [Tooltip("Pixels to sway left/right.")]
    public float swayPixels = 10f;
    public float swaySpeed = 0.25f;

    [Tooltip("Fraction of base width to pulse (e.g. 0.05 = ±5%).")]
    [Range(0f, 0.3f)] public float pulseAmount = 0.05f;
    public float pulseSpeed = 0.7f;

    [Tooltip("Edge ripple as a fraction of half-width (0.0–0.2 is subtle).")]
    [Range(0f, 0.3f)] public float edgeWaveAmount = 0.08f;
    [Tooltip("How many waves vertically across the texture.")]
    public float edgeWaveFrequency = 1.5f;
    public float edgeWaveSpeed = 0.35f;

    [Tooltip("Darken top & bottom (0 = none, 1 = strong).")]
    [Range(0f, 1f)] public float verticalVignette = 0.20f;

    [Header("Texture / Update")]
    [Tooltip("Horizontal resolution of the beam (lower = cheaper).")]
    public int textureWidth = 384;                 // a bit lower for animated edges
    public bool matchRectHeight = true;
    [Tooltip("Seconds between texture refreshes (0.033 ≈ 30 FPS).")]
    [Range(0.01f, 0.2f)] public float updateInterval = 0.033f;

    [Header("Optional Flicker")]
    public bool flicker = false;
    public float flickerAmplitude = 0.08f;
    public float flickerSpeed = 0.8f;

    RawImage raw;
    Texture2D tex;
    RectTransform rt;
    int lastH;
    float accum; // for update timer

    void Awake()
    {
        raw = GetComponent<RawImage>();
        rt = GetComponent<RectTransform>();
        CreateOrResizeTexture();
        RenderBeam(Time.unscaledTime);
    }

    void Update()
    {
        bool resized = false;

        if (matchRectHeight)
        {
            int h = Mathf.Max(2, Mathf.RoundToInt(rt.rect.height));
            if (h != lastH) { CreateOrResizeTexture(); resized = true; }
        }

        // Only re-render at the requested cadence (and when resized)
        accum += Time.unscaledDeltaTime;
        if (!animate && !flicker && !resized) return;
        if (!resized && accum < updateInterval) return;
        accum = 0f;

        RenderBeam(Time.unscaledTime);
    }

    void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled) return;
        if (matchRectHeight) { CreateOrResizeTexture(); RenderBeam(Time.unscaledTime); }
    }

    void CreateOrResizeTexture()
    {
        int w = Mathf.Max(64, textureWidth);
        int h = matchRectHeight ? Mathf.Max(2, Mathf.RoundToInt(rt.rect.height)) : Mathf.Max(2, 1080);
        lastH = h;

        if (tex == null || tex.width != w || tex.height != h)
        {
            if (tex != null) Destroy(tex);
            tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            raw.texture = tex;
            raw.uvRect = new Rect(0, 0, 1, 1);
        }
    }

    void RenderBeam(float t)
    {
        if (tex == null) return;

        int w = tex.width;
        int h = tex.height;

        // Base parameters in pixels
        float rectWidthPx = Mathf.Max(2f, rt.rect.width) * (raw.canvas ? raw.canvas.scaleFactor : 1f);
        // We render in texture space (w), so compute everything in tex pixels
        float halfWidthBase = Mathf.Max(1f, w * beamWidthFraction * 0.5f);
        float feather = Mathf.Max(1f, w * featherFraction);

        // Dynamics
        float centerOffsetPx = animate ? swayPixels * Mathf.Sin(t * Mathf.PI * 2f * swaySpeed) : 0f;

        float widthPulse = animate ? (1f + pulseAmount * Mathf.Sin(t * Mathf.PI * 2f * pulseSpeed)) : 1f;
        float halfWidth = halfWidthBase * widthPulse;

        // Global flicker (alpha)
        float aPeak = alphaPeak;
        if (flicker)
        {
            float n = Mathf.PerlinNoise(t * flickerSpeed, 0f) * 2f - 1f;
            aPeak = Mathf.Clamp01(alphaPeak + n * flickerAmplitude);
        }

        // Precompute a per-row edge wobble using Perlin noise (subtle)
        // waveOffset is applied symmetrically to both edges, varies with Y and time
        float[] rowWave = new float[h];
        for (int y = 0; y < h; y++)
        {
            float vy = (float)y / Mathf.Max(1, h - 1); // 0..1
            float noise = Mathf.PerlinNoise(
                vy * edgeWaveFrequency + t * edgeWaveSpeed,
                t * 0.1234f);
            noise = (noise * 2f - 1f); // -1..1
            rowWave[y] = halfWidth * edgeWaveAmount * noise;
        }

        // Vertical vignette profile
        // 1 at center, fades toward top/bottom by verticalVignette
        float[] rowVignette = new float[h];
        for (int y = 0; y < h; y++)
        {
            float vy = (float)y / Mathf.Max(1, h - 1); // 0..1
            float distFromCenter = Mathf.Abs(vy - 0.5f) * 2f; // 0 center → 1 at edges
            float vig = Mathf.Lerp(1f, 1f - verticalVignette, distFromCenter);
            rowVignette[y] = Mathf.Clamp01(vig);
        }

        // Render
        for (int y = 0; y < h; y++)
        {
            float cx = (w - 1) * 0.5f + (centerOffsetPx * w / Mathf.Max(2f, rectWidthPx)); // sway in tex pixels
            float halfW = halfWidth + rowWave[y];                                           // wavy edges for this row

            Color c;
            float alpha;

            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x - cx);

                if (dx <= halfW)
                {
                    alpha = aPeak;
                }
                else
                {
                    float tEdge = Mathf.InverseLerp(halfW + feather, halfW, dx);
                    // cosine falloff for a smooth edge
                    alpha = aPeak * 0.5f * (1f + Mathf.Cos(Mathf.PI * (1f - tEdge)));
                    alpha = Mathf.Max(0f, alpha);
                }

                alpha *= rowVignette[y]; // apply vertical vignette

                c = new Color(1f, 1f, 1f, alpha);
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(false, false);
    }
}
