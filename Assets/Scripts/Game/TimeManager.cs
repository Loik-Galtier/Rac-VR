using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviour
{
    [Header("Scene Times")]
    [SerializeField] float mainSceneTime = 300f;
    [SerializeField] float tutorialSceneTime = 180f;
    [SerializeField] float globalTime = 420f;

    [Header("UI")]
    [SerializeField] GameObject timerCanvas;
    [SerializeField] TextMeshProUGUI currentTimerText;
    [SerializeField] TextMeshProUGUI globalTimerText;
    [SerializeField] InputActionReference buttonX;

    [HideInInspector] public bool isInGame = false;
    public static TimeManager Instance;

    float currentTimer, globalTimer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += UpdateCurrentTimer;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= UpdateCurrentTimer;
    }


    void Update()
    {
        if (isInGame)
        {
            currentTimer -= Time.deltaTime;
            globalTimer -= Time.deltaTime;

            if (globalTimer <= 0 || currentTimer <= 0)
            {
                isInGame = false;
                Game.Manager.SessionTimeout();
            }
        }

        ToggleTimerCanvas();

        if (timerCanvas.activeInHierarchy)
        {
            currentTimerText.text = FormatTime(currentTimer);
            globalTimerText.text = FormatTime(globalTimer);
        }
    }

    void UpdateCurrentTimer(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "RAC_MainScene":
                currentTimer = mainSceneTime;
                break;
            case "RAC_Tutorial":
                currentTimer = tutorialSceneTime;
                globalTimer = globalTime;
                break;
        }

        isInGame = false;
    }

    string FormatTime(float timeInSeconds)
    {
        float timeToDisplay = Mathf.Max(0, timeInSeconds);

        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        return string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    void ToggleTimerCanvas()
    {
        if (buttonX.action.WasPerformedThisFrame())
        {
            timerCanvas.SetActive(!timerCanvas.activeInHierarchy);
        }
    }
}
