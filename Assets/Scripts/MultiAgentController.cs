using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiAgentController : MonoBehaviour
{
    public enum PlayMode
    {
        Autoplay,
        Step
    }
    
    [Header("References")]
    public MultiAgentGridManager gridManager;
    public MultiAgentPathfinder agent1;
    public MultiAgentPathfinder agent2;
    
    [Header("UI Elements")]
    public Button autoplayButton;
    public Button stepModeButton;
    public Button nextButton;
    public TextMeshProUGUI nextButtonText;
    public TextMeshProUGUI totalCostValueAgent1;
    public TextMeshProUGUI totalCostValueAgent2;
    
    [Header("Autoplay Settings")]
    public float autoplayCooldown = 3f;
    
    [Header("Colors")]
    public Color activeButtonColor = new Color(0.2f, 0.8f, 0.2f);
    public Color inactiveButtonColor = new Color(0.7f, 0.7f, 0.7f);
    
    private PlayMode currentMode = PlayMode.Autoplay;
    private bool bothAgentsComplete = false;
    
    void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<MultiAgentGridManager>();
        }
        
        autoplayButton.onClick.AddListener(OnAutoplayButtonClicked);
        stepModeButton.onClick.AddListener(OnStepModeButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        
        SetMode(PlayMode.Autoplay);
    }
    
    public void OnAutoplayButtonClicked()
    {
        SetMode(PlayMode.Autoplay);
    }
    
    public void OnStepModeButtonClicked()
    {
        SetMode(PlayMode.Step);
    }
    
    public void OnNextButtonClicked()
    {
        if (bothAgentsComplete)
        {
            GenerateNewPaths();
            return;
        }
        
        agent1.StepNext();
        agent2.StepNext();
        
        UpdateCostDisplay();
        
        if (agent1.PathComplete && agent2.PathComplete)
        {
            bothAgentsComplete = true;
            if (nextButtonText != null)
            {
                nextButtonText.text = "New Routes";
            }
        }
    }
    
    void SetMode(PlayMode mode)
    {
        currentMode = mode;
        
        StopAllCoroutines();
        
        if (mode == PlayMode.Autoplay)
        {
            StartCoroutine(AutoplayLoop());
        }
        else
        {
            GenerateNewPaths();
        }
        
        UpdateButtonStates(mode);
    }
    
    void UpdateButtonStates(PlayMode mode)
    {
        ColorBlock autoplayColors = autoplayButton.colors;
        ColorBlock stepColors = stepModeButton.colors;
        
        if (mode == PlayMode.Autoplay)
        {
            autoplayColors.normalColor = activeButtonColor;
            stepColors.normalColor = inactiveButtonColor;
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            autoplayColors.normalColor = inactiveButtonColor;
            stepColors.normalColor = activeButtonColor;
            nextButton.gameObject.SetActive(true);
        }
        
        autoplayButton.colors = autoplayColors;
        stepModeButton.colors = stepColors;
    }
    
    void GenerateNewPaths()
    {
        gridManager.ClearPathVisualization();
        
        agent1.ResetPath();
        agent2.ResetPath();
        
        agent1.GenerateNewPath();
        agent2.GenerateNewPath();
        
        bothAgentsComplete = false;
        
        if (nextButtonText != null)
        {
            nextButtonText.text = "Next";
        }
        
        UpdateCostDisplay();
    }
    
    IEnumerator AutoplayLoop()
    {
        yield return new WaitForSeconds(1f);
        
        while (currentMode == PlayMode.Autoplay)
        {
            if (!agent1.IsMoving && !agent2.IsMoving)
            {
                gridManager.ClearPathVisualization();
                
                agent1.ResetPath();
                agent2.ResetPath();
                
                agent1.GenerateNewPath();
                agent2.GenerateNewPath();
                
                UpdateCostDisplay();
                
                agent1.StartAutoplayMovement();
                agent2.StartAutoplayMovement();
                
                StartCoroutine(UpdateCostDuringMovement());
            }
            
            yield return new WaitForSeconds(autoplayCooldown);
        }
    }
    
    IEnumerator UpdateCostDuringMovement()
    {
        while (agent1.IsMoving || agent2.IsMoving)
        {
            UpdateCostDisplay();
            yield return new WaitForSeconds(0.1f);
        }
        
        UpdateCostDisplay();
    }
    
    void UpdateCostDisplay()
    {
        if (totalCostValueAgent1 != null)
        {
            totalCostValueAgent1.text = $"{agent1.CurrentCost} / {agent1.TotalPathCost}";
        }
        
        if (totalCostValueAgent2 != null)
        {
            totalCostValueAgent2.text = $"{agent2.CurrentCost} / {agent2.TotalPathCost}";
        }
    }
}
