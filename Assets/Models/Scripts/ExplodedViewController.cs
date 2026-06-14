using UnityEngine;
using DG.Tweening;

/// <summary>
/// ExplodedViewController.cs
/// Anime la pièce active : elle apparaît d'abord en position "éclatée"
/// (au-dessus / à côté de l'assemblage), reste visible quelques secondes,
/// puis se déplace vers sa position cible (= position de montage finale).
/// Nécessite le package DOTween (Assets > Import DOTween, puis
/// Tools > Demigiant > DOTween Utility Panel > Setup DOTween).
/// </summary>
public class ExplodedViewController : MonoBehaviour
{
    [Header("Timings de l'animation exploded-view")]
    [Tooltip("Délai avant le début de l'animation (laisse le temps au modèle d'apparaître)")]
    public float startDelay = 0.3f;

    [Tooltip("Durée pendant laquelle la pièce reste visible en position éclatée")]
    public float holdDuration = 1.5f;

    [Tooltip("Durée du déplacement de la position éclatée vers la position cible")]
    public float moveToTargetDuration = 1.2f;

    [Tooltip("Courbe d'animation pour le déplacement final")]
    public Ease moveEase = Ease.InOutCubic;

    [Tooltip("La pièce tourne-t-elle légèrement pendant l'animation (effet 'présentation')")]
    public bool rotateDuringHold = true;
    public float rotationSpeedDegPerSec = 45f;

    private Tween activeTween;
    private Tween activeRotationTween;

    /// <summary>
    /// Joue l'animation exploded-view pour la pièce donnée :
    /// position éclatée -> pause -> position cible.
    /// Appelée par AssemblyController à chaque chargement d'étape.
    /// </summary>
    public void PlayExplodedAnimation(GameObject piece, StepData step)
    {
        if (piece == null || step == null) return;

        KillActiveTweens();

        // 1. Placer immédiatement la pièce en position éclatée
        piece.transform.localPosition = step.explodedLocalPosition;
        piece.transform.localRotation = Quaternion.Euler(step.targetLocalRotation);

        Sequence seq = DOTween.Sequence();
        seq.SetTarget(piece.transform);

        seq.AppendInterval(startDelay);
        seq.AppendInterval(holdDuration);
        seq.Append(
            piece.transform
                .DOLocalMove(step.targetLocalPosition, moveToTargetDuration)
                .SetEase(moveEase)
        );

        activeTween = seq;

        if (rotateDuringHold)
        {
            activeRotationTween = piece.transform
                .DOLocalRotate(new Vector3(0f, 360f, 0f), 360f / rotationSpeedDegPerSec, RotateMode.LocalAxisAdd)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);

            // Stoppe la rotation continue une fois la pièce arrivée en position finale
            seq.OnComplete(() =>
            {
                activeRotationTween?.Kill();
                piece.transform.localRotation = Quaternion.Euler(step.targetLocalRotation);
            });
        }
    }

    /// <summary>
    /// Permet de rejouer l'animation (ex: bouton "Revoir l'animation" dans le HUD).
    /// </summary>
    public void ReplayAnimation(GameObject piece, StepData step)
    {
        PlayExplodedAnimation(piece, step);
    }

    /// <summary>
    /// Place instantanément la pièce à sa position finale sans animation
    /// (utile lors de la navigation arrière entre étapes).
    /// </summary>
    public void SnapToTarget(GameObject piece, StepData step)
    {
        if (piece == null || step == null) return;

        KillActiveTweens();
        piece.transform.localPosition = step.targetLocalPosition;
        piece.transform.localRotation = Quaternion.Euler(step.targetLocalRotation);
    }

    private void KillActiveTweens()
    {
        activeTween?.Kill();
        activeRotationTween?.Kill();
        activeTween = null;
        activeRotationTween = null;
    }

    private void OnDestroy()
    {
        KillActiveTweens();
    }
}
