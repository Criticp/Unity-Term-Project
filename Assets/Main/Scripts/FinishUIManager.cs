using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro; // ← only if you’re using TextMeshPro for your coin display

[RequireComponent(typeof(Collider))]
public class FinishUIManager : MonoBehaviour
{
    [Header("Finish Screen UI")]
    public GameObject finishCanvas;           // Assign your Game Finished Canvas here
    public Button resumeButton;               // e.g. “Continue” or “Keep Exploring”
    public Button restartButton;              // e.g. “Play Again”
    public bool lockCursorOnStart = true;     // if you want the cursor locked at game start

    [Header("Coin Reset (on Restart)")]
    [Tooltip("PlayerPrefs key used for storing your coin total")]
    public string coinPrefKey = "CoinCount";
    [Tooltip("Optional: assign your on-screen text so it shows '0000' right away")]
    public TMP_Text coinText;                 // or use public Text coinText; for legacy UI

    [Header("UI SFX (same as pause)")]
    public AudioSource uiAudioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    [Header("Finish Game SFX (Optional)")]
    [Tooltip("Clip to play once when the player finishes the game.")]
    public AudioClip cheerClip;
    [Range(0f, 1f)] public float cheerVolume = 1f;

    bool isFinished = false;

    void Start()
    {
        // hide it at first
        finishCanvas.SetActive(false);

        // cursor initial state
        if (lockCursorOnStart)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // ensure we have an AudioSource
        if (uiAudioSource == null)
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f;

        // hook up buttons
        WireUpButton(resumeButton, HideFinishUI);
        WireUpButton(restartButton, RestartLevel);

        // ensure our collider is a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // ensure PlayerPrefs key exists (so we don't get missing-key errors)
        if (!PlayerPrefs.HasKey(coinPrefKey))
        {
            PlayerPrefs.SetInt(coinPrefKey, 0000);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Call this to show the “Game Finished” overlay.
    /// </summary>
    public void ShowFinishUI()
    {
        if (isFinished) return;
        isFinished = true;

        // play the cheer SFX if assigned
        if (cheerClip != null)
            uiAudioSource.PlayOneShot(cheerClip, cheerVolume);

        // show canvas
        finishCanvas.SetActive(true);

        // freeze everything
        Time.timeScale = 0f;

        // unlock cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void HideFinishUI()
    {
        if (!isFinished) return;
        isFinished = false;

        // hide canvas
        finishCanvas.SetActive(false);

        // unfreeze
        Time.timeScale = 1f;

        // re‐lock
        if (lockCursorOnStart)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void RestartLevel()
    {
        // 0) Reset the static coin total in Coin.cs
        Coin.ResetScore();

        // 1) reset PlayerPrefs coin count
        PlayerPrefs.SetInt(coinPrefKey, 0000);
        PlayerPrefs.Save();

        // 2) update on-screen coin text immediately (4 digits, leading zeros)
        if (coinText != null)
            coinText.text = "0000";

        // 3) unfreeze in case we were frozen
        Time.timeScale = 1f;

        // 4) reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Hooks up hover + click SFX, plus your onClick action.
    /// </summary>
    void WireUpButton(Button b, UnityAction onClickAction)
    {
        // 1) the user callback
        b.onClick.AddListener(onClickAction);

        // 2) click SFX
        b.onClick.AddListener(() =>
        {
            if (clickClip != null)
                uiAudioSource.PlayOneShot(clickClip);
        });

        // 3) hover SFX
        var trigger = b.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener((_) =>
        {
            if (hoverClip != null)
                uiAudioSource.PlayOneShot(hoverClip);
        });
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// When the player touches this trigger, finish the game.
    /// Make sure this GameObject has a Trigger Collider and the Player is tagged "Player".
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        ShowFinishUI();
    }
}
