using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("References")]
    public PathfindingAgent agent;
    
    [Header("UI Elements")]
    public Button autoplayButton;
    public Button stepModeButton;
    public Button nextButton;
    public TextMeshProUGUI nextButtonText;
    public TextMeshProUGUI totalCostValue;
    
    [Header("Colors")]
    public Color activeButtonColor = new Color(0.2f, 0.8f, 0.2f);
    public Color inactiveButtonColor = new Color(0.7f, 0.7f, 0.7f);
    
    void Start()
    {
        if (agent == null)
        {
            agent = FindAnyObjectByType<PathfindingAgent>();
        }
        
        autoplayButton.onClick.AddListener(OnAutoplayButtonClicked);
        stepModeButton.onClick.AddListener(OnStepModeButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        
        SetMode(PathfindingAgent.PlayMode.Autoplay);
    }
    
    public void OnAutoplayButtonClicked()
    {
        SetMode(PathfindingAgent.PlayMode.Autoplay);
    }
    
    public void OnStepModeButtonClicked()
    {
        SetMode(PathfindingAgent.PlayMode.Step);
    }
    
    public void OnNextButtonClicked()
    {
        if (agent != null)
        {
            agent.StepNext();
        }
    }
    
    void SetMode(PathfindingAgent.PlayMode mode)
    {
        if (agent != null)
        {
            agent.SetPlayMode(mode);
        }
        
        UpdateButtonStates(mode);
    }
    
    void UpdateButtonStates(PathfindingAgent.PlayMode mode)
    {
        ColorBlock autoplayColors = autoplayButton.colors;
        ColorBlock stepColors = stepModeButton.colors;
        
        if (mode == PathfindingAgent.PlayMode.Autoplay)
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
    
    public void UpdateNextButtonText(string text)
    {
        if (nextButtonText != null)
        {
            nextButtonText.text = text;
        }
    }
    
    public void UpdateTotalCost(int currentCost, int totalCost)
    {
        if (totalCostValue != null)
        {
            totalCostValue.text = $"{currentCost} / {totalCost}";
        }
    }
    
    public void ResetTotalCost()
    {
        if (totalCostValue != null)
        {
            totalCostValue.text = "0 / 0";
        }
    }
}
