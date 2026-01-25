# Derot My Brain – Spécifications Fonctionnelles

## 1. Objectif du document

Ce document décrit les **spécifications fonctionnelles** de l’application **Derot My Brain**. Il sert de référence commune pour :
- cadrer le périmètre fonctionnel,
- aligner les choix produit avec le glossaire,
- guider le développement backend / frontend,
- éviter les ambiguïtés fonctionnelles.

---

## 2. Vision Produit

Derot My Brain est une application d’**apprentissage actif** destinée aux :
- curieux,
- étudiants,
- professionnels préparant des certifications.

Le principe central est :
> *Lire → S’auto-évaluer → Mesurer la progression → Répéter intelligemment*

L’application transforme des contenus passifs (articles Wikipédia, documents personnels) en **activités interactives** basées sur des quiz générés par IA.

---

## 3. Concepts Clés (référence Glossaire)

Les termes suivants sont **normatifs** et doivent être utilisés tels quels dans l’UI, le code et la documentation :

- **Derot Zone** : espace de session où se déroule une activité
- **User Activity** : interaction unique d’un utilisateur avec un contenu (Read ou Quiz)
- **User Focus** : agrégation de progression sur un sujet
- **Content Source** : origine du contenu (Wikipédia ou Document utilisateur)
- **Backlog** : liste de contenus à traiter plus tard
- **My Documents** : bibliothèque de documents uploadés
- **Knowledge Area** : catégorie de connaissance utilisée pour filtrer

---

## 4. Parcours Utilisateur Global

### 4.1 Flux principal

1. L’utilisateur sélectionne ou ajoute un **Content Source**
2. Il entre dans la **Derot Zone**
3. Il effectue une **User Activity** :
   - Lecture (Read)
   - Quiz (Quiz)
4. Le système enregistre l’activité
5. Les métriques du **User Focus** sont mises à jour
6. L’utilisateur peut recommencer ou changer de sujet

---

## 5. Fonctionnalités Détaillées

### 5.1 Derot Zone

#### Description
Espace central de travail où l’utilisateur interagit activement avec un contenu.

#### Règles fonctionnelles
- Une Derot Zone est toujours liée à **un seul Content Source**
- Une session produit au minimum **une User Activity**
- Le contenu affiché dépend du type de source

#### Modes
- **Read Mode** : affichage du contenu
- **Quiz Mode** : questions générées dynamiquement par IA

---

### 5.2 User Activity

#### Types
- **Read**
- **Quiz**

#### Données enregistrées
- Type d’activité
- Date / heure
- Content Source
- Score (uniquement pour Quiz)

#### Règles
- Toute interaction significative crée une User Activity
- Les activités ne sont jamais supprimées (historique immuable)

---

### 5.3 Quiz

#### Génération
- Basée exclusivement sur le contenu du Content Source
- Pilotée par un LLM via une interface abstraite

#### Règles
- Le quiz doit être auto-suffisant
- Les questions doivent tester la compréhension, pas la mémorisation brute

#### Résultat
- Score exprimé en pourcentage
- Enregistré dans l’activité

---

### 5.4 User Focus

#### Rôle réel (clarification clé)
Un **User Focus** est une **entité de visibilité et d’agrégation**, dont le seul objectif est de permettre à l’utilisateur de *suivre* sa progression sur une **Content Source donnée**.

Il ne représente **pas** une activité et **ne contient pas de données d’apprentissage propres**.

Toutes les données affichées dans un User Focus proviennent exclusivement des **User Activities** associées au même **SourceHash**.

---

#### Identification technique
Un User Focus est identifié par les mêmes propriétés qu’une User Activity :
- **SourceType** *(enum)* : Wikipedia | Document
- **SourceId** *(string)* : URL Wikipédia ou chemin logique du document
- **SourceHash** *(string)* : hash déterministe basé sur `SourceType + SourceId`

👉 `SourceHash` est la **clé primaire fonctionnelle** et le lien entre :
- User Activities
- User Focus

---

#### Création / Suppression
- Un User Focus **n’est jamais créé automatiquement**
- Il est créé **uniquement** via une action explicite de l’utilisateur :
  - depuis une **User Activity** dans l’Historique
  - ou depuis une **User Focus Card** (re-tracking)
- Le *Untrack* supprime l’entité User Focus **sans supprimer aucune User Activity**

---

#### Comportement fonctionnel
- Un utilisateur peut avoir des **User Activities non trackées**
- Le tracking agit uniquement sur la **visibilité dans la page My Focus Area**
- Re-tracker un sujet restaure **l’intégralité de l’historique existant** lié au SourceHash

---

#### Affichage (My Focus Area Page)
- La page affiche la liste des **User Focus**
- Chaque card correspond à **un SourceHash unique**
- Le dépliage d’une card affiche :
  - la timeline complète des User Activities filtrées par `SourceHash`
  - triées par date décroissante

---

#### Métriques dérivées (calculées, non stockées)
- Best Score
- Last Attempt Score
- Last Activity Date

Ces métriques sont calculées dynamiquement à partir des User Activities.

---

#### Renommage / Display
- Chaque entité dispose d’un champ **DisplayName**
- Le DisplayName est modifiable sans impacter :
  - le SourceHash
  - l’agrégation
  - l’historique

---

### 5.5 Backlog

#### Description
Liste de **Content Sources** que l’utilisateur souhaite traiter ultérieurement **sans créer immédiatement de User Activity**.

Le Backlog est principalement destiné aux **articles Wikipédia** (notamment issus de sélections aléatoires ou exploratoires).

---

#### Règles fonctionnelles
- Le Backlog contient uniquement des **Content Sources non encore traitées**
- Démarrer une activité depuis le Backlog :
  - crée une **User Activity**
  - crée automatiquement un **User Focus** pour la source
  - supprime la source du Backlog
- Une source supprimée du Backlog n’entraîne aucune suppression de données

---


### 5.6 My Documents (Ma bibliothèque)

#### Description
Espace personnel de stockage des documents uploadés par l’utilisateur.

La page **Ma bibliothèque** constitue une forme de backlog implicite pour les documents :
- un document uploadé est disponible pour créer une activité à tout moment
- il n’est **pas nécessaire** de l’ajouter explicitement au Backlog

---

#### Règles fonctionnelles
- Les documents uploadés sont **immuables** (contenu et SourceId)
- Les activités basées sur un document peuvent être **Trackées / Untrackées** comme toute autre source
- Un document peut générer plusieurs User Activities
- Seul l’utilisateur peut supprimer un document depuis Ma bibliothèque

---

#### Suppression d’un document
- Supprimer un document :
  - supprime l’entrée de la bibliothèque
  - empêche toute nouvelle activité
  - **ne supprime pas** les User Activities existantes (historique conservé)

---


### 5.7 Wikipedia Integration

#### Description
Source de contenu dynamique basée sur Wikipédia.

#### Fonctionnalités
- Recherche manuelle
- Sélection aléatoire via catégories / thèmes

#### Règles
- Le titre Wikipédia est utilisé comme Title par défaut
- L’URL est la référence technique unique

---

### 5.8 History Page

#### Description
Vue chronologique exhaustive de **toutes les User Activities**, indépendamment de leur statut de tracking.

---

#### Règles fonctionnelles
- Les activités sont affichées par **ordre décroissant de date**
- Chaque User Activity est rattachée à un **SourceHash**
- La page permet :
  - de consulter l’historique global
  - de filtrer par Knowledge Area, SourceType, ou SourceId
  - de **Track / Untrack** la source associée à une activité

---

#### Lien avec User Focus
- Une action *Track* depuis l’Historique crée une entrée User Focus
- Une action *Untrack* n’affecte pas l’historique
- L’Historique est la **source de vérité** des données utilisateur

---


## 6. Règles Fonctionnelles Transverses

- Le système est **source-agnostique**
- La logique d’apprentissage est indépendante du format du contenu
- L’utilisateur contrôle ses priorités (User Focus)
- Les scores servent à **mesurer**, jamais à sanctionner

---

## 7. Hors Périmètre (V1)

- Collaboration multi-utilisateurs
- Partage public de User Focus
- Gamification avancée (badges, streaks)

---


## 8. Évolutions Futures Envisagées

- Mobile (Android / iOS)
- Répétition espacée
- Statistiques globales par Knowledge Area
- Import de nouvelles sources (web, markdown, notes)

---

## 9. Référence

Ce document doit être cohérent avec :
- Glossary.md
- Architecture.md
- Backend-Architecture.md
- Frontend-Architecture.md

