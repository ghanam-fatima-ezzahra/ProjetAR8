# 🥽 ProjetAR-8 — Montage PC en Réalité Augmentée

> Application Android AR qui guide un technicien lors du montage d'un PC de bureau,  
> étape par étape, avec vue 3D éclatée et codage couleur de la visserie.

---

## 📸 Aperçu

 
 Vue AR en fonctionnement | Vue exploded-view |
 

## 🎬 Démo Vidéo

 

## ✅ Fonctionnalités implémentées

| Critère du prof | Statut | Détail |
|---|:---:|---|
| APK Android avec min. 5 niveaux de guidage | ✅ | 5 étapes PC : Boîtier → Carte mère → CPU → RAM → GPU |
| Reconnaissance QR code / marqueur ARCore | ✅ | ARTrackedImageManager + Reference Image Library |
| Vue 3D exploded-view animée | ✅ | Animation Coroutine depuis position éclatée → position cible |
| Codage couleur visserie | ✅ | M3 doré, M4 bleu, M5 noir, M2 orange |
| Codage couleur câblage | ✅ | Rouge = alimentation 24P, Bleu = SATA data |
| Navigation étape par étape | ✅ | Boutons flottants AR "Suivant / Précédent" + indicateur "Étape X/5" |
| HUD AR flottant | ✅ | Canvas World Space avec progression et instructions |
| Feedback visuel vert/rouge | ✅ | FeedbackManager + vibration haptic Android |
| Architecture ScriptableObjects | ✅ | StepData × 5 assets configurables sans code |
| Machine à états | ✅ | Idle → Detecting → Guiding → Verifying → Complete |
| Marqueurs ARCore imprimables | ✅ | QR code PDF couleur dans Assets/XR/ |
| Code source commenté sur dépôt Git | ✅ | Ce dépôt GitHub public |

---

## 🛠️ Stack technique

| Outil | Version | Rôle |
|---|---|---|
| Unity | 2022.3 LTS | Moteur principal |
| AR Foundation | 5.x | Framework AR multiplateforme |
| Google ARCore XR Plugin | 5.x | Tracking spatial Android |
| C# | — | Scripts : machine à états, ScriptableObjects |
| Modèles 3D | — | Unity Asset Store (composants PC low-poly) |
| Git + GitHub | — | Versionnage et collaboration |

---

## 📁 Structure du projet

```
ProjetAR8/
├── Assets/
│   ├── Scripts/
│   │   ├── AssemblyController.cs   ← Machine à états principale
│   │   ├── StepData.cs             ← ScriptableObject par étape
│   │   ├── UIManager.cs            ← HUD AR flottant
│   │   ├── FeedbackManager.cs      ← Feedbacks visuels + vibration
│   │   └── ColorCodingSystem.cs    ← Codage couleur visserie/câbles
│   ├── Prefabs/
│   │   ├── BoitierPC               ← Étape 1
│   │   ├── motherboard1            ← Étape 2
│   │   ├── Procesador              ← Étape 3
│   │   ├── Ram                     ← Étape 4
│   │   └── DrakeIndustries_GPU     ← Étape 5
│   ├── Data/Steps/
│   │   ├── Step_01_Boitier.asset
│   │   ├── Step_02_CarteMere.asset
│   │   ├── Step_03_CPU.asset
│   │   ├── Step_04_RAM.asset
│   │   └── Step_05_GPU.asset
│   ├── Materials/                  ← Matériaux URP + codage couleur
│   └── XR/                         ← Reference Image Library + QR code
├── Builds/
│   └── Android/
│       └── ProjetAR8.apk           ← APK installable
├── screenshots/                    ← 📸 Tes captures d'écran ici
├── demo/                           ← 🎬 Ta vidéo MP4 ici
└── README.md
```

---

## 🚀 Comment tester

### Prérequis
- Téléphone Android avec **ARCore supporté** ([liste officielle](https://developers.google.com/ar/devices))
- Android 8.0 (API 26) minimum

### Installation
1. Télécharge l'APK : [`Builds/Android/ProjetAR8.apk`](Builds/Android/ProjetAR8.apk)
2. Sur ton téléphone : **Paramètres → Sécurité → Sources inconnues → Autoriser**
3. Installe l'APK

### Utilisation
1. Imprime le QR code : [`Assets/XR/QRCode_ProjetAR8.png`](Assets/XR/QRCode_ProjetAR8.png) en **10×10 cm minimum**
2. Pose le QR code sur une surface plane bien éclairée
3. Lance l'application **ProjetAR8**
4. Pointe la caméra vers le QR code → les pièces 3D apparaissent
5. Appuie sur **"→ Suivant"** pour passer à l'étape suivante
6. Suis les 5 étapes de montage guidées

---

## 📋 Les 5 étapes de montage

| # | Étape | Pièce | Visserie | Câble |
|---|---|---|---|---|
| 1 | Installation du boîtier | BoitierPC | — | — |
| 2 | Installation carte mère | motherboard1 | M3 doré × 6 | — |
| 3 | Installation CPU | Procesador | M3 doré × 4 | — |
| 4 | Installation RAM | Ram | — | — |
| 5 | Installation GPU | DrakeIndustries_GPU | M3 doré × 2 | Alimentation PCIe |

---
