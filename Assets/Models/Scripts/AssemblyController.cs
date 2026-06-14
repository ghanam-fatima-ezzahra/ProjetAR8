using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// États de la machine à états du montage (cf. architecture technique ProjetAR-8).
/// </summary>
public enum AssemblyState
{
    Idle,        // en attente, aucun marqueur détecté
    Detecting,   // marqueur/QR code en cours de détection
    Guiding,     // étape affichée, en attente d'action utilisateur
    Verifying,   // vérification de position en cours
    Complete     // toutes les étapes sont terminées
}

/// <summary>
/// AssemblyController.cs
/// Orchestre l'ensemble du parcours de montage : charge chaque StepData,
/// instancie la pièce + visserie/câblage associés, déclenche l'animation
/// exploded-view, applique le codage couleur, gère la navigation
/// (étape suivante/précédente) et la vérification de position.
/// À placer sur un GameObject "AssemblyController" dans la scène AR,
/// référencé par UIManager.
/// </summary>
public class AssemblyController : MonoBehaviour
{
    [Header("Données de montage (ordre = ordre d'assemblage)")]
    [Tooltip("Glisser ici les 5 StepData créés (Step_01_Motherboard ... Step_05_PSU)")]
    public List<StepData> steps = new List<StepData>();

    [Header("Références scène")]
    [Tooltip("Transform de l'ancre AR (assigné par AnchorManager une fois le marqueur détecté)")]
    public Transform anchorRoot;

    public ExplodedViewController explodedView;
    public PositionVerifier positionVerifier;
    public ColorCodingSystem colorCoding;
    public FeedbackManager feedbackManager;
    public UIManager uiManager;

    [Header("Prefabs visserie / câblage génériques")]
    [Tooltip("Prefab générique de vis, recoloré dynamiquement par ColorCodingSystem")]
    public GameObject screwPrefab;

    [Tooltip("Prefab générique de câble (utiliser SataCable comme base, recoloré dynamiquement)")]
    public GameObject cablePrefab;

    public AssemblyState CurrentState { get; private set; } = AssemblyState.Idle;
    public int CurrentStepIndex => currentStepIndex;

    private int currentStepIndex = 0;
    private GameObject currentPieceInstance;
    private GameObject currentScrewInstance;
    private GameObject currentCableInstance;

    private void Start()
    {
        CurrentState = AssemblyState.Idle;
    }

    /// <summary>
    /// Appelée par AnchorManager dès que le marqueur/QR code est détecté
    /// et qu'un anchor stable est généré.
    /// </summary>
    public void OnAnchorDetected(Transform anchor)
    {
        anchorRoot = anchor;
        currentStepIndex = 0;
        CurrentState = AssemblyState.Guiding;
        LoadStep(currentStepIndex);
    }

    /// <summary>
    /// Charge et affiche l'étape à l'index donné : instancie la pièce,
    /// la visserie et/ou le câble associés, lance l'animation exploded-view,
    /// applique le codage couleur et met à jour le HUD.
    /// </summary>
    private void LoadStep(int index)
    {
        if (steps == null || index < 0 || index >= steps.Count || anchorRoot == null)
            return;

        ClearCurrentInstances();

        StepData step = steps[index];
        CurrentState = AssemblyState.Guiding;

        // 1. Instancier la pièce principale
        if (step.piecePrefab != null)
        {
            currentPieceInstance = Instantiate(step.piecePrefab, anchorRoot);
            currentPieceInstance.transform.localPosition = step.targetLocalPosition;
            currentPieceInstance.transform.localRotation = Quaternion.Euler(step.targetLocalRotation);

            if (explodedView != null)
                explodedView.PlayExplodedAnimation(currentPieceInstance, step);
        }

        // 2. Instancier la visserie si nécessaire
        if (step.requiresScrews && screwPrefab != null)
        {
            currentScrewInstance = Instantiate(screwPrefab, anchorRoot);
            currentScrewInstance.transform.localPosition = step.targetLocalPosition;
        }

        // 3. Instancier le câble si nécessaire
        if (step.requiresCable && cablePrefab != null)
        {
            currentCableInstance = Instantiate(cablePrefab, anchorRoot);
            currentCableInstance.transform.localPosition = step.targetLocalPosition;
        }

        // 4. Appliquer le codage couleur de la visserie / du câblage
        if (colorCoding != null)
            colorCoding.ApplyStepColors(step, currentScrewInstance, currentCableInstance);

        // 5. Mettre à jour le HUD (texte d'instruction, indicateur d'étape, barre de progression)
        if (uiManager != null)
            uiManager.UpdateStepDisplay(index + 1, steps.Count, step);
    }

    /// <summary>
    /// Bouton "Étape suivante". Si on est sur la dernière étape, termine le montage.
    /// </summary>
    public void GoToNextStep()
    {
        if (CurrentState == AssemblyState.Complete) return;

        if (currentStepIndex < steps.Count - 1)
        {
            currentStepIndex++;
            LoadStep(currentStepIndex);
        }
        else
        {
            CompleteAssembly();
        }
    }

    /// <summary>
    /// Bouton "Étape précédente".
    /// </summary>
    public void GoToPreviousStep()
    {
        if (CurrentState == AssemblyState.Complete) return;

        if (currentStepIndex > 0)
        {
            currentStepIndex--;
            LoadStep(currentStepIndex);
        }
    }

    /// <summary>
    /// Bouton "Vérifier" : compare la position réelle/attendue via PositionVerifier
    /// et déclenche le feedback visuel/sonore/haptique correspondant.
    /// </summary>
    public void RequestVerification()
    {
        if (positionVerifier == null || currentPieceInstance == null || steps.Count == 0)
            return;

        CurrentState = AssemblyState.Verifying;
        StepData step = steps[currentStepIndex];

        bool isValid = positionVerifier.VerifyPosition(currentPieceInstance, step);

        if (isValid)
        {
            feedbackManager?.ShowSuccess(currentPieceInstance);
            uiManager?.ShowAlert("Pièce correctement positionnée !", false);
            feedbackManager?.PlayStepCompleteSound(step.stepCompleteSound);
        }
        else
        {
            feedbackManager?.ShowError(currentPieceInstance);
            uiManager?.ShowAlert("Position incorrecte, vérifiez le placement.", true);
        }

        CurrentState = AssemblyState.Guiding;
    }

    private void CompleteAssembly()
    {
        CurrentState = AssemblyState.Complete;
        uiManager?.ShowCompletionScreen();
    }

    private void ClearCurrentInstances()
    {
        if (currentPieceInstance != null) Destroy(currentPieceInstance);
        if (currentScrewInstance != null) Destroy(currentScrewInstance);
        if (currentCableInstance != null) Destroy(currentCableInstance);

        currentPieceInstance = null;
        currentScrewInstance = null;
        currentCableInstance = null;
    }
}
