using UnityEngine;

/// <summary>
/// PositionVerifier.cs
/// Compare la position/rotation de la pièce instanciée (suivant l'ancre AR)
/// avec la position/rotation cible définie dans le StepData, avec une tolérance
/// configurable par étape.
/// À placer sur un GameObject "Managers" dans la scène AR.
/// </summary>
public class PositionVerifier : MonoBehaviour
{
    [Header("Référence caméra AR (pour la vérification par raycast)")]
    public Camera arCamera;

    [Header("Raycast")]
    [Tooltip("Distance maximale du rayon lancé depuis la caméra AR pour viser une pièce")]
    public float maxRayDistance = 1.5f;

    [Tooltip("Layer(s) sur lesquels les pièces AR sont placées (pour filtrer le raycast)")]
    public LayerMask piecesLayerMask = ~0; // tous les layers par défaut

    /// <summary>
    /// Vérification "directe" : compare la position/rotation locale courante de la pièce
    /// (par rapport à son parent = ancre AR) avec la position/rotation attendue du StepData.
    /// Utilisée pour la validation automatique de l'étape (bouton "Vérifier").
    /// </summary>
    public bool VerifyPosition(GameObject piece, StepData step)
    {
        if (piece == null || step == null) return false;

        // Position attendue exprimée dans le même repère local que la pièce (= ancre AR)
        Vector3 expectedLocalPos = step.targetLocalPosition;
        Vector3 actualLocalPos = piece.transform.localPosition;

        float distance = Vector3.Distance(expectedLocalPos, actualLocalPos);
        bool positionOk = distance <= step.positionTolerance;

        Quaternion expectedRot = Quaternion.Euler(step.targetLocalRotation);
        Quaternion actualRot = piece.transform.localRotation;
        float angleDiff = Quaternion.Angle(expectedRot, actualRot);
        bool rotationOk = angleDiff <= step.rotationTolerance;

        return positionOk && rotationOk;
    }

    /// <summary>
    /// Vérification "physique" : l'utilisateur pointe la pièce réellement montée avec
    /// l'appareil. Un raycast depuis la caméra AR cherche un marqueur secondaire
    /// (collider) sur la pièce, et compare sa position monde à la position attendue.
    /// Utile pour le Module 2 (robot) où chaque segment porte un marqueur imprimé.
    /// </summary>
    public bool VerifyWithRaycast(StepData step, Transform anchorRoot, out RaycastHit hitInfo)
    {
        hitInfo = default;

        if (arCamera == null || anchorRoot == null) return false;

        Ray ray = new Ray(arCamera.transform.position, arCamera.transform.forward);

        if (!Physics.Raycast(ray, out hitInfo, maxRayDistance, piecesLayerMask))
            return false;

        Vector3 expectedWorldPos = anchorRoot.TransformPoint(step.targetLocalPosition);
        float distance = Vector3.Distance(hitInfo.point, expectedWorldPos);

        return distance <= step.positionTolerance;
    }
}
