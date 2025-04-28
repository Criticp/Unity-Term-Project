using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class WaterDeathRespawn : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Collider depthPlaneTrigger;

    [System.Serializable]
    public class KillPlane
    {
        public Collider trigger;         // The collider that kills you
        [TextArea] public string deathMessage;   // The message to show when this plane kills you
    }
    [Tooltip("All death-zone triggers with their individual messages")]
    public KillPlane[] killPlanes;

    [Header("Safe-Terrain Check")]
    public Collider terrainCollider;
    public float terrainCheckDistance = 0.1f;

    [Header("Death UI (Image + TextMeshProUGUI)")]
    public Image deathMessageBackground;
    public TMP_Text deathMessageText;
    public float deathMessageDuration = 3f;
    public float deathMessageFadeCyclesPerSecond = 1f;

    [Header("Death Sound (Optional)")]
    public AudioSource deathAudioSource;
    public AudioClip deathClip;
    [Range(0f, 1f)] public float deathClipVolume = 1f;

    [Header("Respawn Offset")]
    public Vector3 respawnOffset = new Vector3(0, .1f, 0);

    [Header("Upright After Respawn")]
    public Vector3 defaultForward = Vector3.forward;

    // Internals
    private Vector3 _lastSafePosition;
    private float _waterY;
    private Color _bgColor, _txtColor;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Start()
    {
        // initial safe spot & water line
        _lastSafePosition = transform.position;
        if (depthPlaneTrigger != null)
            _waterY = depthPlaneTrigger.transform.position.y;

        // prep UI
        if (deathMessageBackground != null)
        {
            _bgColor = deathMessageBackground.color;
            _bgColor.a = 0f;
            deathMessageBackground.color = _bgColor;
        }
        if (deathMessageText != null)
        {
            _txtColor = deathMessageText.color;
            _txtColor.a = 0f;
            deathMessageText.color = _txtColor;
        }
    }

    void Update()
    {
        // record any time we're above water AND standing on the assigned terrain
        if (transform.position.y > _waterY + .05f && terrainCollider != null)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out var hit, terrainCheckDistance)
                && hit.collider == terrainCollider)
            {
                _lastSafePosition = transform.position;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // see if any KillPlane trigger matches
        foreach (var kp in killPlanes)
        {
            if (kp.trigger == other)
            {
                HandleDeath(kp.deathMessage);
                return;
            }
        }
    }

    private void HandleDeath(string message)
    {
        // teleport back
        transform.position = _lastSafePosition + respawnOffset;

        // upright the player
        UprightTransform();

        // zero out velocity
        if (_rb != null)
            _rb.linearVelocity = Vector3.zero;

        // play death sound
        if (deathClip != null)
        {
            if (deathAudioSource != null)
                deathAudioSource.PlayOneShot(deathClip, deathClipVolume);
            else
                AudioSource.PlayClipAtPoint(deathClip, transform.position, deathClipVolume);
        }

        // show custom message
        if (deathMessageText != null)
            deathMessageText.text = message;

        // flash UI
        StartCoroutine(ShowDeathUI());
    }

    private void UprightTransform()
    {
        Vector3 fwd = transform.forward;
        Vector3 flatFwd = Vector3.ProjectOnPlane(fwd, Vector3.up);
        if (flatFwd.sqrMagnitude < 1e-3f)
            flatFwd = defaultForward;
        transform.rotation = Quaternion.LookRotation(flatFwd.normalized, Vector3.up);
    }

    private IEnumerator ShowDeathUI()
    {
        float elapsed = 0f;
        while (elapsed < deathMessageDuration)
        {
            float a = Mathf.PingPong(elapsed * deathMessageFadeCyclesPerSecond, 1f);
            if (deathMessageBackground != null)
            {
                _bgColor.a = a;
                deathMessageBackground.color = _bgColor;
            }
            if (deathMessageText != null)
            {
                _txtColor.a = a;
                deathMessageText.color = _txtColor;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        // hide UI
        if (deathMessageBackground != null)
        {
            _bgColor.a = 0f;
            deathMessageBackground.color = _bgColor;
        }
        if (deathMessageText != null)
        {
            _txtColor.a = 0f;
            deathMessageText.color = _txtColor;
        }
    }
}
