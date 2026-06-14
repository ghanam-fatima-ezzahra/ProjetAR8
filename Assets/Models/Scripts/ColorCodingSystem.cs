using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ColorCodingSystem.cs
/// Applique dynamiquement une couleur de matériau sur les prefabs de vis et de câbles
/// en fonction du type défini dans le StepData courant.
/// À placer sur un GameObject "Managers" dans la scène AR.
/// </summary>
public class ColorCodingSystem : MonoBehaviour
{
    public static ColorCodingSystem Instance { get; private set; }

    [System.Serializable]
    public struct ScrewColorMapping
    {
        public ScrewType type;
        public Color color;
        [Tooltip("Texte affiché dans la légende UI, ex: 'M3 - Doré (carte mère)'")]
        public string label;
    }

    [System.Serializable]
    public struct CableColorMapping
    {
        public CableType type;
        public Color color;
        public string label;
    }

    [Header("Mapping couleurs vis (modifiable dans l'Inspector)")]
    public List<ScrewColorMapping> screwColors = new List<ScrewColorMapping>
    {
        new ScrewColorMapping { type = ScrewType.M2_Orange, color = new Color(1f, 0.55f, 0f),     label = "M2 - Orange (servomoteur)" },
        new ScrewColorMapping { type = ScrewType.M3_Gold,   color = new Color(0.83f, 0.69f, 0.22f), label = "M3 - Doré (carte mère / GPU)" },
        new ScrewColorMapping { type = ScrewType.M4_Blue,   color = new Color(0.1f, 0.3f, 1f),     label = "M4 - Bleu (structure)" },
        new ScrewColorMapping { type = ScrewType.M5_Black,  color = new Color(0.05f, 0.05f, 0.05f), label = "M5 - Noir (alimentation)" },
    };

    [Header("Mapping couleurs câbles (modifiable dans l'Inspector)")]
    public List<CableColorMapping> cableColors = new List<CableColorMapping>
    {
        new CableColorMapping { type = CableType.Power24Pin_Red, color = Color.red,   label = "Alimentation 24 broches - Rouge" },
        new CableColorMapping { type = CableType.SataData_Blue,  color = Color.blue,  label = "SATA Data - Bleu" },
        new CableColorMapping { type = CableType.Signal_Green,   color = Color.green, label = "Signal - Vert" },
    };

    [Header("Matériau de base (URP Lit recommandé)")]
    [Tooltip("Si vide, le matériau d'origine de chaque renderer est cloné.")]
    public Material baseEmissiveMaterial;

    [Range(0f, 1f)]
    [Tooltip("Intensité de l'émission appliquée par-dessus la couleur (effet 'glow' AR)")]
    public float emissionIntensity = 0.35f;

    private Dictionary<ScrewType, Color> screwColorMap;
    private Dictionary<CableType, Color> cableColorMap;
    private readonly Dictionary<Renderer, Material> instancedMaterials = new Dictionary<Renderer, Material>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        screwColorMap = new Dictionary<ScrewType, Color>();
        foreach (var m in screwColors)
            screwColorMap[m.type] = m.color;

        cableColorMap = new Dictionary<CableType, Color>();
        foreach (var m in cableColors)
            cableColorMap[m.type] = m.color;
    }

    public Color GetScrewColor(ScrewType type)
    {
        if (type == ScrewType.None) return Color.white;
        return screwColorMap.TryGetValue(type, out var c) ? c : Color.white;
    }

    public Color GetCableColor(CableType type)
    {
        if (type == CableType.None) return Color.white;
        return cableColorMap.TryGetValue(type, out var c) ? c : Color.white;
    }

    public string GetScrewLabel(ScrewType type)
    {
        foreach (var m in screwColors)
            if (m.type == type) return m.label;
        return string.Empty;
    }

    public string GetCableLabel(CableType type)
    {
        foreach (var m in cableColors)
            if (m.type == type) return m.label;
        return string.Empty;
    }

    /// <summary>
    /// Applique la couleur correspondant au type de vis sur tous les renderers
    /// de l'instance fournie (et ses enfants).
    /// </summary>
    public void ApplyScrewColor(GameObject screwInstance, ScrewType type)
    {
        ApplyColor(screwInstance, GetScrewColor(type));
    }

    /// <summary>
    /// Applique la couleur correspondant au type de câble sur tous les renderers
    /// de l'instance fournie (et ses enfants).
    /// </summary>
    public void ApplyCableColor(GameObject cableInstance, CableType type)
    {
        ApplyColor(cableInstance, GetCableColor(type));
    }

    /// <summary>
    /// Point d'entrée appelé par AssemblyController à chaque chargement d'étape :
    /// applique en une fois la couleur de vis et/ou de câble définie dans le StepData.
    /// </summary>
    public void ApplyStepColors(StepData step, GameObject screwInstance, GameObject cableInstance)
    {
        if (step == null) return;

        if (step.requiresScrews && screwInstance != null)
            ApplyScrewColor(screwInstance, step.requiredScrewType);

        if (step.requiresCable && cableInstance != null)
            ApplyCableColor(cableInstance, step.requiredCableType);
    }

    private void ApplyColor(GameObject target, Color color)
    {
        if (target == null) return;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        foreach (var rend in renderers)
        {
            Material mat = GetOrCreateInstanceMaterial(rend);

            mat.color = color;

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);

            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * emissionIntensity);
            }
        }
    }

    private Material GetOrCreateInstanceMaterial(Renderer rend)
    {
        if (instancedMaterials.TryGetValue(rend, out var existing) && existing != null)
            return existing;

        Material source = baseEmissiveMaterial != null ? baseEmissiveMaterial : rend.sharedMaterial;
        Material instance = new Material(source);
        rend.material = instance;
        instancedMaterials[rend] = instance;
        return instance;
    }

    /// <summary>
    /// Nettoie le cache de matériaux instanciés (à appeler si l'on détruit
    /// régulièrement les prefabs de vis/câbles entre les étapes pour éviter les fuites).
    /// </summary>
    public void ClearMaterialCache()
    {
        instancedMaterials.Clear();
    }
}
