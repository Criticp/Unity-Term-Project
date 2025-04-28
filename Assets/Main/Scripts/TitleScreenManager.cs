using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TitleScreenManager : MonoBehaviour
{
    [Header("Title Screen UI")]
    [Tooltip("Drag your Title Screen Canvas (or Panel) here")]
    public GameObject titleScreenUI;
    [Tooltip("Drag the Start Game Button here")]
    public Button startGameButton;

    [Header("Game Settings")]
    public bool loadSeparateScene = false;
    public string gameplaySceneName = "GameScene";

    [Header("UI SFX")]
    public AudioSource uiAudioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    void Awake()
    {
        // 1) Pause the game and show the cursor
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 2) Show the title UI
        titleScreenUI.SetActive(true);

        // 3) Ensure we have an AudioSource
        if (uiAudioSource == null)
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f;

        // 4) Make sure there’s an EventSystem in the scene
        if (EventSystem.current == null)
        {
            var evt = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            // optional: evt.transform.SetParent(transform);
        }

        // 5) Wire up Start button + sounds
        startGameButton.onClick.AddListener(OnStartGameClicked);
        startGameButton.onClick.AddListener(() =>
        {
            if (clickClip != null) uiAudioSource.PlayOneShot(clickClip);
        });
        var trigger = startGameButton.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener((_) =>
        {
            if (hoverClip != null) uiAudioSource.PlayOneShot(hoverClip);
        });
        trigger.triggers.Add(entry);
    }

    void Update()
    {
        // As an extra safety, keep the cursor visible/unlocked while on title screen
        if (titleScreenUI.activeSelf)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void OnStartGameClicked()
    {
        // unpause
        Time.timeScale = 1f;

        if (loadSeparateScene)
            SceneManager.LoadScene(gameplaySceneName);
        else
            titleScreenUI.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
    }
}