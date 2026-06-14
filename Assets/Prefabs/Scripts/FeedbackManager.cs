using System.Collections;
using UnityEngine;

/// <summary>
/// FeedbackManager.cs
/// Déclenche les retours visuels (contour vert/rouge, particules de validation),
/// les retours sonores et la vibration haptique Android lors de la vérification
/// de position d'une pièce (cf. PositionVerifier).
/// À placer sur un GameObject "Managers" dans la scène AR.
/// </summary>
public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("Particules de validation")]
    [Tooltip("Préfab de particules joué quand la pièce est correctement positionnée")]
    public ParticleSystem successParticlesPrefab;

    [Tooltip("Préfab de particules joué quand la pièce est mal positionnée")]
    public ParticleSystem errorParticlesPrefab;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip errorSound;
    public AudioClip stepCompleteSound;

    [Header("Surbrillance (contour)")]
    [Tooltip("Couleur de contour appliquée en cas de validation correcte")]
    public Color successColor = Color.green;

    [Tooltip("Couleur de contour appliquée en cas d'erreur de position")]
    public Color errorColor = Color.red;

    [Tooltip("Nom de la propriété couleur du shader d'outline (URP custom shader)")]
    public string outlineColorProperty = "_OutlineColor";

    [Tooltip("Nom de la propriété épaisseur du shader d'outline")]
    public string outlineWidthProperty = "_OutlineWidth";

    public float outlineWidthActive = 4f;
    public float highlightDuration = 1.2f;

    [Header("Vibration (Android)")]
    public bool enableHapticFeedback = true;
    [Tooltip("Durée de vibration en millisecondes pour une validation correcte")]
    public long successVibrationMs = 60;
    [Tooltip("Durée de vibration en millisecondes pour une erreur (double pulse)")]
    public long errorVibrationMs = 120;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Appelé par AssemblyController quand PositionVerifier confirme que la
    /// pièce est correctement placée.
    /// </summary>
    public void ShowSuccess(GameObject target)
    {
        PlaySound(successSound);
        SpawnParticles(successParticlesPrefab, target);
        Highlight(target, successColor);
        Vibrate(successVibrationMs);
    }

    /// <summary>
    /// Appelé par AssemblyController quand PositionVerifier détecte un
    /// mauvais positionnement.
    /// </summary>
    public void ShowError(GameObject target)
    {
        PlaySound(errorSound);
        SpawnParticles(errorParticlesPrefab, target);
        Highlight(target, errorColor);
        Vibrate(errorVibrationMs);
    }

    /// <summary>
    /// Son joué lors du passage à l'étape suivante / fin d'étape validée.
    /// </summary>
    public void PlayStepCompleteSound(AudioClip overrideClip = null)
    {
        PlaySound(overrideClip != null ? overrideClip : stepCompleteSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    private void SpawnParticles(ParticleSystem prefab, GameObject target)
    {
        if (prefab == null || target == null) return;

        ParticleSystem instance = Instantiate(prefab, target.transform.position, Quaternion.identity);
        instance.Play();

        float lifetime = instance.main.duration + instance.main.startLifetime.constantMax;
        Destroy(instance.gameObject, lifetime);
    }

    private void Highlight(GameObject target, Color color)
    {
        if (target == null) return;
        StopAllCoroutines();
        StartCoroutine(HighlightRoutine(target, color));
    }

    private IEnumerator HighlightRoutine(GameObject target, Color color)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        Material[] mats = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            mats[i] = renderers[i].material;

            if (mats[i].HasProperty(outlineColorProperty))
                mats[i].SetColor(outlineColorProperty, color);

            if (mats[i].HasProperty(outlineWidthProperty))
                mats[i].SetFloat(outlineWidthProperty, outlineWidthActive);
        }

        yield return new WaitForSeconds(highlightDuration);

        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] != null && mats[i].HasProperty(outlineWidthProperty))
                mats[i].SetFloat(outlineWidthProperty, 0f);
        }
    }

    private void Vibrate(long durationMs)
    {
        if (!enableHapticFeedback) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                if (vibrator != null)
                    vibrator.Call("vibrate", durationMs);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("FeedbackManager: vibration indisponible - " + e.Message);
        }
#else
        // Dans l'Editeur / autres plateformes : fallback générique
        Handheld.Vibrate();
#endif
    }
}
