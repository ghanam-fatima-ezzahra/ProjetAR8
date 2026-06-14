using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// AnchorManager.cs (BONUS - optionnel si déjà fait à l'étape 1)
/// Détecte le marqueur / QR code via ARTrackedImageManager (AR Foundation Image Tracking)
/// et notifie AssemblyController dès qu'un ancrage stable est trouvé.
///
/// Mise en place :
/// - Sur l'objet "XR Origin" : ajouter le composant "AR Tracked Image Manager"
/// - Créer une "Reference Image Library" (clic droit > Create > XR > Reference Image Library)
///   et y ajouter l'image du marqueur/QR code (taille physique réelle en mètres)
/// - Ajouter ce script sur le même objet que l'AR Tracked Image Manager
/// - Glisser AssemblyController dans le champ "Assembly Controller"
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class AnchorManager : MonoBehaviour
{
    [Tooltip("AssemblyController de la scène, notifié quand l'ancre est trouvée")]
    public AssemblyController assemblyController;

    [Tooltip("Si vrai, l'ancrage n'est créé qu'une seule fois (recommandé pour un parcours guidé)")]
    public bool createAnchorOnce = true;

    private ARTrackedImageManager trackedImageManager;
    private bool anchorCreated;

    private void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
            HandleTrackedImage(trackedImage);

        foreach (var trackedImage in args.updated)
            HandleTrackedImage(trackedImage);
    }

    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        if (createAnchorOnce && anchorCreated) return;
        if (trackedImage.trackingState != TrackingState.Tracking) return;

        anchorCreated = true;
        assemblyController?.OnAnchorDetected(trackedImage.transform);
    }
}
