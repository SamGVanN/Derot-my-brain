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

#### UI et contrôles spécifiques (Derot Zone pour Wikipédia)
- **Header de filtres / catégories (session-only)** :
  - L’utilisateur peut sélectionner des catégories ou thèmes pour filtrer les articles.
  - Ces préférences sont **stockées côté client pour la session** et **ne modifient pas** la table `UserPreference`.
- **Champ URL directe** :
  - Permet de coller l’URL complète d’un article Wikipédia (ex: `https://fr.wikipedia.org/wiki/Intelligence_artificielle`).
  - Le champ extrait le `lang` et le `title` et déclenche le flux `Read` si l’utilisateur le confirme.
- **Bouton Recycle** :
  - Refait une sélection / récupération d’articles aléatoires ou basés sur les filtres du header.
  - Appelle l’endpoint backend `GET /api/wikipedia/random` ou `search` selon le mode.

#### Article Cards
- Chaque article est affiché en carte avec : titre, court résumé, vignette (si disponible), langue, lien vers la page complète.
- Actions sur la carte :
  - **Read** : crée une `UserActivity` Type=`Read` côté serveur et ouvre la vue de lecture.
  - **Add to Backlog** : ajoute la source (title+url+lang) au `Backlog` de l’utilisateur.
  - Pendant que l’utilisateur explore (par navigation, recherche, Recycle), la UserActivity est en étape Explore (Enum ActivityType). Tout comme pour Read et Quiz, le temps de la UserActivity de Type Explore est enregistré (calculé par le front ou back selon choix fait par l'architecte). Quand l'utilisateur clique sur le bouton Read, la UserActivity de type Explore est enregistré et on commence une nouvelle UserActivity de type Read (avec ses propres timers).

#### Modes
- **Read Mode** : affichage du contenu d'une Source
- **Quiz Mode** : questions générées dynamiquement par IA basées uniquement sur le contenu de la Source.

#### Comportements UX
- Le passage en lecture via le bouton `Read` doit :
  - enregistrer la UserActivity de type Explore (stop des timers)
  - récupérer l'article complet (titre et body),
  - créer la `UserActivity` Type=`Read` associée (avec métadonnées : `title`, `lang`, `sourceUrl`, `pageId`),
  - proposer d’ajouter l’article aux `UserFocus` si souhaité.
  - Adapter la Derot Zone en mode Read
- Le bouton `Add to Backlog` n’enregistre pas de `UserActivity` immédiatement.

#### Cas d’usage principaux (Derot Zone)
- L’utilisateur explore des articles filtrés ou aléatoires (`Explore`).
- L’utilisateur déclenche la lecture d’une source (`Read`) — crée `UserActivity`.
- L’utilisateur ajoute une source au `Backlog` pour traitement ultérieur.

---

### 5.2 User Activity

#### Types
- **Read**
- **Quiz**

Additional type
- **Explore** — interactions where the user browses/searches/refetches content without explicitly starting a Read session. Typical while entering Derot Zone.

#### Données enregistrées
- Type d’activité
- Date / heure
- Content Source
- Score (uniquement pour Quiz)

#### Données minimales enregistrées pour chaque type
- `Explore` : `Type=Explore`, timestamp, `SourceHint` (optionnel: query/filters), `SessionId` (optionnel)
- `Read` : `Type=Read`, timestamp, `Content Source` (title/url/lang/pageId), `Duration` (optionnel)
- `Quiz` : `Type=Quiz`, timestamp, `Score`, `Content Source`

#### Champs additionnels pour `Explore`
- `SourceType` / `SourceId` : **doivent être renseignés** pour toute `Explore`. Pour les explorations qui ne proviennent pas d'une Source précise, utiliser des valeurs conventionnelles générées au démarrage de l'activité :
  - `SourceType = "DerotZoneExploration"`
  - `SourceId = <ISO8601 timestamp>` (ex : `2026-01-25T14:32:00Z`) — valeur horodatée définie au moment du début de l'Explore.
- `ResultingReadActivityId` *(nullable Guid)* : si l'exploration débouche sur une lecture (`Read`), stocker l'ID de la `UserActivity` `Read` correspondante ; sinon `null`.
- `BacklogAddsCount` *(nullable int)* : nombre d'articles ajoutés au Backlog durant cette session d'`Explore`. `null` signifie "non renseigné"; `0` signifie aucune addition.

Ces champs permettent :
- relier une session d'exploration à la lecture résultante (si elle existe),
- mesurer le niveau d'engagement exploratoire (combien d'items ont été ajoutés au Backlog),
- garder l'historique immuable tout en fournissant des métriques utiles.

#### Règles
- Toute interaction significative crée une User Activity
- Les activités ne sont jamais supprimées (historique immuable)

#### Règles
- Les interactions exploratoires (navigation entre cartes, Recycle, recherche) doivent compter comme une seule activité `Explore` : c'est une "pré-étape" avant un Read.
- Le bouton `Read` **crée obligatoirement** une `UserActivity` Type=`Read` avant d’ouvrir la session de lecture complète.
- Ajouter une source au `Backlog` **ne crée pas** une `UserActivity`.

#### Recommandations techniques (DB / DTO)
- `UserActivity` table / entity :
  - `SourceType` (string) **non-nullable**
  - `SourceId` (string) **non-nullable** — pour `Explore` utiliser la valeur horodatée décrite ci-dessus
  - ajouter nullable columns : `ResultingReadActivityId` (GUID, FK nullable vers `UserActivity`), `BacklogAddsCount` (int nullable)
  - `SourceHash` : calculer systématiquement à partir de `SourceType + SourceId` (concat deterministic + hashing). Pour les `Explore` rows, `SourceHash` restera déterministe et non-null.
- DTOs : étendre `UserActivityDto` avec `ResultingReadActivityId` et `BacklogAddsCount` (nullable) et exposer `SourceType`/`SourceId` (non-nullable).
- API : l'endpoint POST qui crée une `Explore` doit accepter `{ sourceHint?, sessionId?, backlogAddsCount? }`, mais le serveur doit générer et renvoyer la paire `SourceType`/`SourceId` finales (et l'ID de l'Explore créée).

#### Règles d'usage et validations
- Toujours renseigner `SourceType`/`SourceId` — **ne pas** stocker des chaînes vides ni des valeurs `null`. Utiliser les constantes/horodatage définies pour les `Explore`.
- Lors de la transition `Explore` → `Read` :
  1. Mettre à jour l'enregistrement `Explore` existant pour remplir `ResultingReadActivityId` avec l'ID de la nouvelle `Read` (transactionnel si possible).
  2. Optionnellement incrémenter `BacklogAddsCount` si l'utilisateur a ajouté des éléments durant l'exploration.
- Les mises à jour sur l'activité `Explore` doivent préserver l'immuabilité historique autant que possible (stocker plutôt que modifier quand cela a du sens), mais relier via `ResultingReadActivityId` est acceptable pour traçabilité.

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

#### Fonctionnalités
- Recherche manuelle
- Sélection aléatoire via catégories / thèmes
- Lecture via URL directe (champ URL dans Derot Zone Header)

#### Règles
- Le titre Wikipédia est utilisé comme `Title` par défaut
- L’URL (lang + title) est la référence technique unique (`SourceId`)
- Lecture directe (via URL) déclenche le flux `Read` et crée une `UserActivity` Type=`Read` associée
- Les articles explorés via recherche / Recycle génèrent des événements `Explore` (suivi léger)

#### Intégration backend / endpoints (résumé)
- `GET /api/wikipedia/search?q={q}&lang={lang}&limit={n}` → liste d’articles
- `GET /api/wikipedia/summary?title={title}&lang={lang}` → résumé détaillé
- `GET /api/wikipedia/random?lang={lang}&count={n}&categories={csv}` → articles aléatoires
- `POST /api/wikipedia/read` → body `{ title, lang, sourceUrl? }` : récupère le résumé, crée `UserActivity` Type=`Read`, retourne l’activité + DTO article
- `POST /api/wikipedia/explore` → body `{ query?, lang?, filters? }` : log léger `Explore` event

#### Backlog interaction
- `Add to Backlog` côté frontend appelle le endpoint Backlog existant (ou `POST /api/backlog`) en fournissant `title`, `lang`, `sourceUrl`, `summary`.
- Ajouter au Backlog **n’entraîne pas** la création d’une `UserActivity`.

#### UX / Edge cases
- Disambiguation pages : afficher indication et options de désambiguïsation
- Redirects : suivre et afficher la page finale
- Thumbnails manquantes : afficher placeholder
- Validation des URL directes : signaler erreurs user-friendly

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

