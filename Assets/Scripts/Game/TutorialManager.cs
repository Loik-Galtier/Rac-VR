using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class TutorialManager : MonoBehaviour, IGameManager
{
    public static TutorialManager instance;

    [Header("Tutorial Elements")]
    [SerializeField] GameObject movePoint1;
    [SerializeField] GameObject movePoint2;
    [SerializeField] GameObject bin;
    [SerializeField] GameObject wastes;
    [SerializeField] GameObject NPC;

    [Header("UI Elements")]
    [SerializeField] GameObject move1Canvas;
    [SerializeField] GameObject characterCanvas;
    [SerializeField] GameObject move2Canvas;
    [SerializeField] GameObject pickupCanvas;
    [SerializeField] GameObject dropCanvas;
    [SerializeField] GameObject dialogueCanvas;
    [SerializeField] GameObject binCanvas;
    [SerializeField] GameObject finishCanvas;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] SlicedFilledImage loadingBarFill;
    
    [SerializeField] GameObject walkingModeCanvas;
    [SerializeField] GameObject jumpingModeCanvas;

    [SerializeField] GameObject languageCanvas;
    [SerializeField] TMP_Dropdown languageDropdown;

    [Header("Movement")]
    [SerializeField] InputActionReference buttonA;
    [SerializeField] InputActionReference buttonB;
    [SerializeField] ControllerInputActionManager rightControllerInput;
    [SerializeField] float cooldownTime = 2.5f;
    [SerializeField] GameObject moveProvider;
    [SerializeField] GameObject teleportProvider;

    [Header("Sound effects")]
    [SerializeField] AudioSource endTutorialSound;

    int numWastesCollected = 0;
    int numWastesProcessed = 0;
    bool firstClick = true;
    bool isCollected = false;
    float switchCooldownTimer = 0;

    void Awake()
    {
        instance = this;
        Game.RegisterManager(this);
    }

    void Start()
    {
        StartCoroutine(WaitForInventoryUpdate());

        walkingModeCanvas.SetActive(false);
        jumpingModeCanvas.SetActive(false);

        moveProvider.SetActive(false);
        teleportProvider.SetActive(false);
    }

    void Update()
    {
        SwitchMovementMode();
    }

    IEnumerator WaitForInventoryUpdate()
    {
        yield return new WaitUntil(() => Inventory.instance != null);
        Inventory.instance.OnInventoryUpdated += CheckAmountCollected;
    }

    void OnDisable()
    {
        if (Inventory.instance == null) Debug.Log("Inventory is null");
        Inventory.instance.OnInventoryUpdated -= CheckAmountCollected;
    }

    public void FinishLanguageSelection()
    {
        
        int value = languageDropdown.value;

        switch (value)
        {
            case 0:
                LocalizationManager.Instance.SetLanguage("French");
                break;
            case 1:
                LocalizationManager.Instance.SetLanguage("English");
                break;
            case 2:
                LocalizationManager.Instance.SetLanguage("Vietnamese");
                break;
        }

        move1Canvas.SetActive(true);
        movePoint1.SetActive(true);

        languageCanvas.SetActive(false);

        moveProvider.SetActive(true);
        teleportProvider.SetActive(true);

        TimeManager.Instance.isInGame = true;
    }

    public void Reached1stDestination()
    {
        if(rightControllerInput.smoothMotionEnabled)
        {
            movePoint1.SetActive(false);
            move1Canvas.SetActive(false);
            NPC.SetActive(true);
        }
    }

    public void Reached2ndDestination()
    {
        move2Canvas.SetActive(false);
        movePoint2.SetActive(false);
        binCanvas.SetActive(true);
    }

    public void CheckAmountCollected()
    {
        numWastesCollected = Inventory.instance.wastes.Count;
        if (numWastesCollected == 1 && !isCollected)
        {
            pickupCanvas.SetActive(false);
            dropCanvas.SetActive(false);
            move2Canvas.SetActive(true);
            movePoint2.SetActive(true);
            bin.SetActive(true);
            isCollected = true;
        }
    }

    public void AddScore(Quest quest)
    {
        numWastesProcessed++;
        if (numWastesProcessed == 1)
        {
            EndTutorial();
        }
    }

    public void MinusScore(Quest quest)
    {
        numWastesProcessed--;
        if (numWastesProcessed == 1)
        {
            EndTutorial();
        }
    }

    public void AddQuest(Quest questPrefab, GameObject wastes, string desKey)
    {
        if (!firstClick) return;

        this.wastes.SetActive(true);

        characterCanvas.SetActive(false);
        pickupCanvas.SetActive(true);
        dropCanvas.SetActive(true);

        firstClick = false;
    }

    public void CompleteQuest(Quest quest)
    {
        
    }

    public void SessionTimeout()
    {
        EndTutorial();
    }

    void EndTutorial()
    {
        binCanvas.SetActive(false);
        finishCanvas.SetActive(true);
        endTutorialSound.Play();
        // Inventory.instance.gameObject.SetActive(false);
    }

    public void LoadMainScene()
    {
        StartCoroutine(LoadMainSceneOperation());
    }
    
    IEnumerator LoadMainSceneOperation()
    {
        finishCanvas.SetActive(false);
        loadingPanel.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync("Rac_MainScene");
        operation.allowSceneActivation = false;
        while (operation.progress < 0.9f)
        {
            loadingBarFill.fillAmount = Mathf.Clamp01(operation.progress / 0.9f);
            yield return null;
        }
        loadingBarFill.fillAmount = 1;

        yield return new WaitForSeconds(1f);
        operation.allowSceneActivation = true;
    }

    #region Switch Movement Mode
    bool isBothPressed = false;
    void SwitchMovementMode()
    {
        switchCooldownTimer += Time.deltaTime;
        if (switchCooldownTimer < cooldownTime) return;

        bool a = buttonA.action.IsPressed();
        bool b = buttonB.action.IsPressed();

        if (a && b)
        {
            if (!isBothPressed)
            {
                rightControllerInput.smoothMotionEnabled = !rightControllerInput.smoothMotionEnabled;
                if (rightControllerInput.smoothMotionEnabled)
                {
                    DisplayMovementCanvas(walkingModeCanvas);
                }
                else
                {
                    DisplayMovementCanvas(jumpingModeCanvas);
                }

                switchCooldownTimer = 0;
                isBothPressed = true;
            }
        }
        else
        {
            isBothPressed = false;
        }
    }

    void DisplayMovementCanvas(GameObject canvas)
    {
        walkingModeCanvas.GetComponent<TweenFadeOut>().ForceEnd();
        jumpingModeCanvas.GetComponent<TweenFadeOut>().ForceEnd();
        canvas.SetActive(true);
    }
    #endregion 
}
