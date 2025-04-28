using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;  // ← for EventTrigger
using TMPro;                     // ← if you’re using TextMeshPro for your coin display

public class UIManager : MonoBehaviour
{
    [Header("Cursor Settings (on Start)")]
    public bool lockAndHideCursor = true;

    [Header("Pause Menu UI")]
    public GameObject pauseMenuUI;
    public Button resumeButton;
    public Button resetButton;

    [Header("Coin Display")]
    [Tooltip("Optional: if you’re using PlayerPrefs to store coins, this will show it")]
    public TMP_Text coinText;           // or use `public Text coinText;` if you’re on legacy UI
    [Tooltip("PlayerPrefs key used for storing your coin total")]
    public string coinPrefKey = "CoinCount";

    [Header("UI SFX")]
    [Tooltip("AudioSource for all UI sounds")]
    public AudioSource uiAudioSource;
    [Tooltip("Clip to play when pausing")]
    public AudioClip pauseClip;
    [Tooltip("Clip to play when resuming/unpausing")]
    public AudioClip resumeClip;
    [Tooltip("Clip to play when hovering over any button")]
    public AudioClip hoverClip;
    [Tooltip("Clip to play when clicking a button")]
    public AudioClip clickClip;

    bool isPaused = false;

    void Awake()
    {
        // force fullscreen 1920×1080
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
    }

    void Start()
    {
        // --- Cursor & Pause UI setup ---
        if (lockAndHideCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        pauseMenuUI.SetActive(false);

        // --- Coin setup via PlayerPrefs ---
        if (!PlayerPrefs.HasKey(coinPrefKey))
            PlayerPrefs.SetInt(coinPrefKey, 0000);
        PlayerPrefs.Save();

        if (coinText != null)
            coinText.text = PlayerPrefs.GetInt(coinPrefKey).ToString("D4");

        // --- AudioSource setup ---
        if (uiAudioSource == null)
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f; // 2D

        // --- Wire up buttons ---
        WireButton(resumeButton, TogglePause);
        WireButton(resetButton, ResetLevel);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    void Pause()
    {
        if (pauseClip != null) uiAudioSource.PlayOneShot(pauseClip);

        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isPaused = true;
    }

    void Resume()
    {
        if (resumeClip != null) uiAudioSource.PlayOneShot(resumeClip);

        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;

        if (lockAndHideCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        isPaused = false;
    }

    void ResetLevel()
    {
        // 0) your custom coin‐reset system
        Coin.ResetScore();

        // 1) (optional) also reset PlayerPrefs coin count if you still use it
        PlayerPrefs.SetInt(coinPrefKey, 0000);
        PlayerPrefs.Save();

        // 2) update on-screen text immediately
        if (coinText != null)
            coinText.text = "0000";

        // 3) restore time & reload
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Helper to wire hover, click and onClick events on a Button
    void WireButton(Button btn, UnityEngine.Events.UnityAction onClickAction)
    {
        // 1) onClick
        btn.onClick.AddListener(onClickAction);

        // 2) click SFX
        btn.onClick.AddListener(() =>
        {
            if (clickClip != null) uiAudioSource.PlayOneShot(clickClip);
        });

        // 3) hover SFX via EventTrigger
        var trigger = btn.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entry.callback.AddListener((_) =>
        {
            if (hoverClip != null) uiAudioSource.PlayOneShot(hoverClip);
        });
        trigger.triggers.Add(entry);
    }
}
