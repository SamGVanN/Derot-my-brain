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
- professionnels préparant des certifications
- toute personne souhaitant étudier ou apprendre librement avec du contenu personnel ou Wikipédia.

Le principe central est :
> *Lire → valider sa compréhension et améliorer sa mémoire → Mesurer la progression → Répéter intelligemment*

L’application transforme des contenus passifs (articles Wikipédia, documents personnels) en **activités interactives** basées sur des quiz générés par IA.

---

## 3. Concepts Clés (référence Glossaire)

Les termes suivants sont **normatifs** et doivent être utilisés tels quels dans l’UI, le code et la documentation :

- **Derot Zone** : espace de session où se déroule une ou plusieurs activités
- **User Activity** : interaction unique d’un utilisateur avec un contenu (Read ou Quiz)
- **Source** : hub central rattaché à un contenu (Wikipédia, Document)
- **Topic** : regroupement logique de plusieurs Sources (ex: "Physique Quantique", "Mathématiques", "exam de droit") créé par l'utilisateur
- **Source Tracking** : action de "suivre" une Source pour suivre sa progression (anciennement User Focus)
- **Backlog** : liste de contenus à traiter plus tard
- **My Documents** : bibliothèque de documents uploadés
- **Knowledge Area** : catégorie système (Wikipédia) utilisée pour le filtrage initial


**Mémo des Relations Clés**

1.  **Centralisation par `User`** : Tout appartient explicitement à un utilisateur (`UserId`).
2.  **Rôle de `Source`** : C'est le pivot du contenu. Un `Document` *appartient* à une Source. Un `BacklogItem` *pointe* vers une Source.
3.  **Organisation par `Topic`** : Les sources peuvent être regroupées dans des Topics (dossiers thématiques).
4.  **Tracking (`UserActivity` & `UserSession`)** :
    *   Une **Session** est le conteneur temporel (ex: "J'ai étudié pendant 30 min").
    *   Une **Activité** est l'action précise dans cette session (ex: "J'ai Lu l'article X", "J'ai fait le Quiz Y").
    *   L'activité lie la performance (Score, Durée) au contenu (Source).


---

## 4. Parcours Utilisateur Global

### 4.1 Flux principal

1. L’utilisateur sélectionne ou ajoute un **Content Source**
2. Il entre dans la **Derot Zone**
3. Il effectue une **User Activity** :
   - Lecture (Read)
   - Quiz (Quiz)
   - c'est le début d'une session (**UserSession**)
4. Le système enregistre l’activité
5. Les métriques de la **Source** et de la **UserSession** sont mises à jour
6. L’utilisateur peut recommencer une activité sur cette source ou arrêter la session

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
  - déclencher la sauvegarde de l'activité `Explore` (calcul de la durée par le front et envoi au back)
  - créer l'activité `Read` liée (linkage via `ResultingReadActivityId`)
  - récupérer l'article complet côté backend
  - persister le contenu dans la `UserActivity` de type Read.
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
  - `SourceId` (string) **non-nullable** : Clé étrangère vers le hub `Source`.
  - `SourceType` (enum) : Type de source (Wikipedia, Document, etc.).
- `Source` Identification :
  - **Deterministic Hash** : Pour les contenus web (Wikipedia, YouTube) afin d'éviter les doublons.
  - **GUID** : Pour les Documents, permettant plusieurs versions distinctes d'un même fichier.
- DTOs : étendre `UserActivityDto` avec `ResultingReadActivityId` et `BacklogAddsCount` (nullable) et exposer les métadonnées de la Source liée.
- API : l'endpoint POST qui crée une `Explore` doit accepter `{ sourceHint?, sessionId? }`. Le serveur génère la `Source` si elle n'existe pas.

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

### 5.4 Source Tracking & Topics (ex-User Focus)

#### Rôle
Le **Source Tracking** remplace le concept de "User Focus" par un flag `IsTracked` sur l'entité `Source`. Une Source trackée devient un point focal de progression.

#### Topic
Un **Topic** est un dossier virtuel créé par l'utilisateur pour organiser ses Sources trackées.
- Une Source peut être liée à **un seul Topic** (optionnel).
- Les statistiques peuvent être agrégées au niveau du Topic.

#### Identification technique
Toutes les activités et sessions pointent vers une `Source.Id` unique.
- **Source.Id** est la clé de voûte de l'agrégation.
- Si `IsTracked = true`, la source apparaît dans l'espace "Ma Progression".

#### Création / Tracking
- Une Source est créée automatiquement lors d'un `Read`.
- Le passage à `IsTracked = true` est explicite (via bouton "Track" ou promotion automatique).
- Le *Untrack* (IsTracked = false) garde l'historique mais masque la source des vues de progression.

#### Métriques (Calculées via le hub Source)
- **Best Score** : meilleur score de quiz sur cette source.
- **Total Time** : somme des durées `Explore + Read + Quiz`.
- **Level** : progression calculée sur le volume d'activités.

#### Renommage
- Le `DisplayTitle` de la Source est personnalisable.
- Le titre du Topic est personnalisable.

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
Source de contenu dynamique basée sur l'API de Wikipédia.

#### Fonctionnalités
- Recherche manuelle
- Sélection aléatoire via catégories / thèmes mis à disposition par l'API
- Lecture via URL directe (champ URL dans Derot Zone Header)

#### Règles
- Le titre Wikipédia est utilisé comme `Title` par défaut
- L’URL (lang + title) est la référence technique unique (`SourceId`)
- Lecture directe (via URL) déclenche le flux `Read` et crée une `UserActivity` Type=`Read` associée
- Les articles explorés via recherche génèrent des événements sur l'activité `Explore` (suivi du nombre d'articles ajoutés au backlog, temps passé sur la recherche)
- Lorsque l'utilisateur choisit de lire un article wikipédia (bouton "Lire l'article"), une activité `Read` est créée au sein de la même UserSession que l'activité `Explore`.

#### Intégration backend / endpoints (résumé)
- l'API DerotMyBrain est-elle même interfacée avec l'API Wikipedia pour fournir au frontend les données nécessaires.
- `GET /api/wikipedia/search?q={q}&lang={lang}&limit={n}` → liste d’articles
- `GET /api/wikipedia/summary?title={title}&lang={lang}` → résumé détaillé
- `GET /api/wikipedia/random?lang={lang}&count={n}&categories={csv}` → articles aléatoires
- `POST /api/wikipedia/read` → body `{ title, lang, sourceUrl? }` : récupère le résumé, crée `UserActivity` Type=`Read`, retourne l’activité + DTO article
- `POST /api/wikipedia/explore` → body `{ query?, lang?, filters? }` : log léger `Explore` event


#### Backlog interaction
- `Add to Backlog` côté frontend appelle le endpoint Backlog existant (ou `POST /api/backlog`) en fournissant `title`, `lang`, `sourceUrl`, `summary` et toute les infos nécessaire pour que l'API DerotMyBrain puisse créer la source.
- Ajouter au Backlog **n’entraîne pas** la création d’une `UserActivity`.

#### UX / Edge cases
- Disambiguation pages : afficher indication et options de désambiguïsation
- Redirects : Dans la DerotZone pas de redirection vers l'article. Car dans la DerotZone l'utilisateur peut lire els articles ou les ajouter au backlog. Un lien externe vers l'article wikipédia serait "tricher" en terme de suivi des temps, l'activité de lecture doit être faite dans la DerotZone.
- Thumbnails manquantes : afficher placeholder
- Validation des URL directes : signaler erreurs user-friendly

---

### 5.8 History Page

#### Description
Vue chronologique exhaustive de **toutes les User Activities**, indépendamment de leur statut de tracking.

---

#### Règles fonctionnelles
- Les activités sont affichées par **ordre décroissant de date**
- Chaque User Activity est rattachée à un **SourceId**
- La page permet :
  - de consulter l’historique global
  - de filtrer par Catégorie Wikipédia, SourceType, SourceId
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
- L’utilisateur contrôle ses priorités (Track/Untrack une source)
- Les scores et temps servent à **mesurer**, **informer** et **encourager**, jamais à sanctionner

---

## 7. Hors Périmètre (V1)

- Collaboration multi-utilisateurs
- Partage public de User Focus (Track/Untrack de Source)
- Gamification avancée (badges, streaks, record personnels)

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
- DataModel.md

