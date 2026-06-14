# ProjetAR-8 : Montage de machine en réalité augmentée

Application Android AR (Unity + AR Foundation + ARCore) qui guide un utilisateur pas à pas
dans le montage d'un PC de bureau, en superposant une vue 3D "exploded view" animée,
un codage couleur de la visserie / du câblage et une vérification de position en temps réel.

## Fonctionnalités implémentées

- Vue 3D exploded-view animée (DOTween) pour chaque pièce du PC
- Reconnaissance de marqueur / QR code via ARCore Image Tracking (AnchorManager)
- Codage couleur de la visserie (M3 doré, M5 noir) et des câbles (rouge = ATX 24 broches,
  bleu = SATA data)
- Navigation étape par étape ("Étape suivante / précédente") avec indicateur de
  progression "Étape X / N"
- Vérification de position de la pièce (validation verte / alerte rouge + vibration)
- Machine à états (Idle -> Detecting -> Guiding -> Verifying -> Complete)

## Stack technique

- Unity 2022 LTS + AR Foundation
- ARCore SDK 1.40+
- DOTween (animations)
- Unity URP (shaders d'outline / émission)
- Unity XR Interaction Toolkit
- TextMeshPro (UI)

## Architecture du code (Assets/Scripts)

| Script | Rôle |
|---|---|
| `StepData.cs` | ScriptableObject décrivant une étape (pièce, position cible/éclatée, visserie, câble, instruction, tolérances) |
| `AssemblyController.cs` | Machine à états principale, orchestre le chargement de chaque étape |
| `ExplodedViewController.cs` | Anime la pièce active (position éclatée -> position de montage) |
| `ColorCodingSystem.cs` | Applique la couleur de la visserie / des câbles selon le `StepData` |
| `PositionVerifier.cs` | Compare la position réelle/attendue de la pièce |
| `FeedbackManager.cs` | Effets visuels (contour vert/rouge), particules, sons, vibration |
| `UIManager.cs` | HUD : boutons flottants, indicateur d'étape, barre de progression, alertes |
| `AnchorManager.cs` | Détection du marqueur/QR code, génération de l'ancre AR |

## Module 1 — Montage d'un PC de bureau (5 étapes)

1. Carte mère (`motherboard1`) — vis M3 dorées
2. Processeur (`Procesador`) — pas de visserie (levier de socket)
3. RAM (`Ram`) — clipsage dans les slots
4. Carte graphique (`DrakeIndustries_GPU`) — vis M3 dorées (équerre)
5. Alimentation (`600wattpsu` / `psu_power_supply_unit`) — vis M5 noires +
   câble ATX 24 broches (rouge) + câble SATA data (bleu, `SataCable`)

## Build & test

1. `File > Build Settings > Android` (Switch Platform)
2. Vérifier `Project Settings > XR Plug-in Management > ARCore` activé
3. `Build and Run` avec le téléphone connecté (mode développeur + débogage USB)

## Tests utilisateurs

Voir `docs/rapport_tests_utilisateurs.md` (à compléter avec au moins 5 testeurs).

## Statut du projet

- [x] Étape 1 — Import des assets / prefabs
- [ ] Étape 2 — Configuration scène AR
- [ ] Étape 3 — Création des 5 StepData
- [ ] Étape 4 — Build APK + tests
- [ ] Étape 5 — Documentation finale + rapport de tests
