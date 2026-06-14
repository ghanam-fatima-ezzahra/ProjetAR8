using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UIManager.cs
/// Gère le HUD AR : boutons virtuels flottants "Étape suivante / précédente / Vérifier",
/// indicateur de progression ("Étape 3 / 12"), texte d'instruction, barre de progression
/// et panneau d'alerte (validation verte / erreur rouge).
/// À placer sur le GameObject "Canvas" (Screen Space - Overlay) de la scène AR,
/// avec une référence vers AssemblyController.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Boutons de navigation flottants")]
    public Button nextStepButton;
    public Button previousStepButton;
    public Button verifyButton;

    [Header("Indicateurs de progression")]
    [Tooltip("Affiche 'Étape X / N'")]
    public TextMeshProUGUI stepIndicatorText;

    [Tooltip("Affiche le texte d'instruction de l'étape courante")]
    public TextMeshProUGUI instructionText;

    [Tooltip("Barre de progression (0 -> nombre total d'étapes)")]
    public Slider progressBar;

    [Header("Panneau d'alerte")]
    public GameObject alertPanel;
    public TextMeshProUGUI alertText;
    public float alertDisplayDuration = 2.5f;

    [Header("Écran de fin")]
    public GameObject completionPanel;

    [Header("Référence")]
    [Tooltip("AssemblyController de la scène (machine à états du montage)")]
    public AssemblyController assemblyController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (nextStepButton != null)
            nextStepButton.onClick.AddListener(OnNextStepClicked);

        if (previousStepButton != null)
            previousStepButton.onClick.AddListener(OnPreviousStepClicked);

        if (verifyButton != null)
            verifyButton.onClick.AddListener(OnVerifyClicked);

        if (alertPanel != null)
            alertPanel.SetActive(false);

        if (completionPanel != null)
            completionPanel.SetActive(false);
    }

    private void OnNextStepClicked()
    {
        assemblyController?.GoToNextStep();
    }

    private void OnPreviousStepClicked()
    {
        assemblyController?.GoToPreviousStep();
    }

    private void OnVerifyClicked()
    {
        assemblyController?.RequestVerification();
    }

    /// <summary>
    /// Appelée par AssemblyController.LoadStep() à chaque changement d'étape.
    /// Met à jour le texte d'instruction, l'indicateur "Étape X / N",
    /// la barre de progression et l'état des boutons de navigation.
    /// </summary>
    public void UpdateStepDisplay(int currentStep, int totalSteps, StepData data)
    {
        if (stepIndicatorText != null)
            stepIndicatorText.text = $"Étape {currentStep} / {totalSteps}";

        if (instructionText != null && data != null)
        {
            string title = string.IsNullOrEmpty(data.stepTitle) ? "" : $"<b>{data.stepTitle}</b>\n";
            instructionText.text = title + data.instructionText;
        }

        if (progressBar != null)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = totalSteps;
            progressBar.value = currentStep;
        }

        SetNavigationButtonsState(currentStep, totalSteps);
    }

    private void SetNavigationButtonsState(int currentStep, int totalSteps)
    {
        if (previousStepButton != null)
            previousStepButton.interactable = currentStep > 1;

        if (nextStepButton != null)
            nextStepButton.interactable = currentStep <= totalSteps; // permet de "terminer" sur la dernière étape
    }

    /// <summary>
    /// Affiche un message temporaire de validation (vert) ou d'erreur (rouge),
    /// appelé après PositionVerifier.VerifyPosition().
    /// </summary>
    public void ShowAlert(string message, bool isError)
    {
        if (alertPanel == null || alertText == null) return;

        alertText.text = message;
        alertText.color = isError ? Color.red : Color.green;
        alertPanel.SetActive(true);

        CancelInvoke(nameof(HideAlert));
        Invoke(nameof(HideAlert), alertDisplayDuration);
    }

    private void HideAlert()
    {
        if (alertPanel != null)
            alertPanel.SetActive(false);
    }

    /// <summary>
    /// Affichée par AssemblyController quand la dernière étape est validée
    /// et que GoToNextStep() est appelé une fois de plus.
    /// </summary>
    public void ShowCompletionScreen()
    {
        if (instructionText != null)
            instructionText.text = "Montage terminé ! Toutes les étapes ont été validées.";

        if (stepIndicatorText != null)
            stepIndicatorText.text = "Terminé";

        if (nextStepButton != null) nextStepButton.gameObject.SetActive(false);
        if (previousStepButton != null) previousStepButton.gameObject.SetActive(false);
        if (verifyButton != null) verifyButton.gameObject.SetActive(false);

        if (completionPanel != null)
            completionPanel.SetActive(true);
    }
}
