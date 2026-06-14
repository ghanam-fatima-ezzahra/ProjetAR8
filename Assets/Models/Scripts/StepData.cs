using UnityEngine;

/// <summary>
/// Types de vis utilisés dans le projet (cf. ProjetAR-8, codage couleur de la visserie)
/// </summary>
public enum ScrewType
{
    None,
    M2_Orange,   // vis de servomoteur (Module 2 - robot)
    M3_Gold,     // vis dorées - carte mère / fixations GPU
    M4_Blue,     // vis de structure (Module 2 - robot)
    M5_Black     // vis noires - alimentation
}

/// <summary>
/// Types de câbles utilisés dans le projet
/// </summary>
public enum CableType
{
    None,
    Power24Pin_Red,   // câble alimentation 24 broches (rouge)
    SataData_Blue,    // câble SATA data (bleu)
    Signal_Green      // câble signal (Module 2 - robot)
}

/// <summary>
/// StepData : décrit une étape de montage (pièce, position, visserie, câblage,
/// texte d'instruction et tolérance de vérification).
/// Une instance = une étape. Créée via menu "Assets > Create > ProjetAR8 > Step Data".
/// </summary>
[CreateAssetMenu(fileName = "Step_New", menuName = "ProjetAR8/Step Data", order = 1)]
public class StepData : ScriptableObject
{
    [Header("Identification")]
    [Tooltip("Numéro affiché à l'utilisateur (1, 2, 3...)")]
    public int stepNumber;

    [Tooltip("Titre court de l'étape, ex: 'Installer la carte mère'")]
    public string stepTitle;

    [TextArea(3, 6)]
    [Tooltip("Texte d'instruction affiché dans le HUD")]
    public string instructionText;

    [Header("Pièce concernée")]
    [Tooltip("Prefab de la pièce à manipuler (ex: motherboard1, Ram, Procesador...)")]
    public GameObject piecePrefab;

    [Tooltip("Position locale finale (par rapport à l'ancre AR), en mètres")]
    public Vector3 targetLocalPosition;

    [Tooltip("Rotation locale finale en degrés (Euler)")]
    public Vector3 targetLocalRotation;

    [Tooltip("Position locale 'éclatée' utilisée pour l'animation exploded-view")]
    public Vector3 explodedLocalPosition;

    [Header("Visserie")]
    public bool requiresScrews;
    public ScrewType requiredScrewType = ScrewType.None;
    [Tooltip("Nombre de vis nécessaires pour cette étape (affichage uniquement)")]
    public int screwCount;

    [Header("Câblage")]
    public bool requiresCable;
    public CableType requiredCableType = CableType.None;

    [Header("Vérification de position")]
    [Tooltip("Tolérance de distance acceptée (en mètres)")]
    public float positionTolerance = 0.02f;

    [Tooltip("Tolérance de rotation acceptée (en degrés)")]
    public float rotationTolerance = 8f;

    [Header("Audio")]
    [Tooltip("Son joué lorsque l'étape est validée (optionnel, sinon le son par défaut de FeedbackManager est utilisé)")]
    public AudioClip stepCompleteSound;
}
