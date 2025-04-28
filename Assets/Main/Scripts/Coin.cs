// Coin.cs
using UnityEngine;
using UnityEngine.VFX;
using TMPro;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class Coin : MonoBehaviour
{
    [Header("Score Settings")]
    [Tooltip("How many points this coin is worth.")]
    public int points = 1;
    [Tooltip("Reference to your on-screen score TextMeshProUGUI (assign in inspector).")]
    public TMP_Text scoreText;

    [Header("Animation Settings")]
    [Tooltip("How high the coin bobs above/below its start pos.")]
    public float bobAmplitude = 0.25f;
    [Tooltip("How fast the coin bobs (cycles per second).")]
    public float bobFrequency = 1f;
    [Tooltip("Degrees per second to spin around Y.")]
    public float spinSpeed = 90f;

    [Header("Audio (Optional)")]
    [Tooltip("AudioSource used to play collection sound (assign in inspector or auto-get).")]
    public AudioSource audioSource;
    [Tooltip("Clip to play when collected.")]
    public AudioClip collectSound;
    [Range(0f, 1f)] public float collectVolume = 1f;

    [Header("VFX Graph (Optional)")]
    [Tooltip("The VFX Graph asset to play on collect.")]
    public VisualEffectAsset collectEffectAsset;
    [Tooltip("Duration in seconds before the spawned VFX is destroyed.")]
    public float collectEffectDuration = 2f;

    private Vector3 _startPos;
    private static int _currentScore = 0;

    void Awake()
    {
        // Ensure we have an AudioSource
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    void Start()
    {
        // Record our start position for bobbing
        _startPos = transform.localPosition;

        // Make sure our collider is a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // Update the UI at start
        UpdateScoreUI();
    }

    void Update()
    {
        // Bob animation
        float bobOffset = Mathf.Sin(Time.time * bobFrequency * 2f * Mathf.PI) * bobAmplitude;
        transform.localPosition = _startPos + Vector3.up * bobOffset;

        // Spin animation
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 1) Award points
        _currentScore += points;
        UpdateScoreUI();

        // 2) Play collection sound
        float delay = 0f;
        if (collectSound != null)
        {
            audioSource.PlayOneShot(collectSound, collectVolume);
            delay = collectSound.length;
        }

        // 3) Spawn VFX Graph if assigned
        if (collectEffectAsset != null)
        {
            var vfxGO = new GameObject("CoinCollectVFX");
            vfxGO.transform.position = transform.position;
            var vfx = vfxGO.AddComponent<VisualEffect>();
            vfx.visualEffectAsset = collectEffectAsset;
            vfx.Play();
            Destroy(vfxGO, collectEffectDuration);
        }

        // 4) Hide the coin immediately
        GetComponent<Collider>().enabled = false;
        var rend = GetComponent<MeshRenderer>();
        if (rend != null) rend.enabled = false;

        // 5) Destroy the coin after sound/VFX delay
        Destroy(gameObject, delay);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            // Always show 4 digits with leading zeros
            scoreText.text = _currentScore.ToString("D4");
    }

    /// <summary>
    /// Call this to reset the running coin total back to zero.
    /// </summary>
    public static void ResetScore()
    {
        _currentScore = 0;
    }
}
