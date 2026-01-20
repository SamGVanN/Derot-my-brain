# Spécifications fonctionnelles

_(version propre, structurée, et ordonnée comme un vrai plan de dev)_

## 1.1 Présentation générale de l'application

**Nom de travail**

**"Derot My Brain"**

**Objectif**

Application web locale destinée à stimuler la curiosité et l'apprentissage actif à partir de contenus Wikipédia, en alternant **lecture libre** et **quiz généré dynamiquement**, sans répétition mécanique des questions.

**Principe global**

- L'application sélectionne une **page Wikipédia aléatoire**, éventuellement filtrée par **axe d'intérêt**.
- L'utilisateur consulte librement l'article.
- Il déclenche un **quiz de 5 questions** générées dynamiquement par une IA locale.
- À la fin du quiz :
  - L'utilisateur obtient son **score**
  - Les **réponses attendues** sont affichées
- Le sujet est enregistré dans un **historique utilisateur**
- Le sujet peut être ajouté aux **Sujets Suivis** (Tracked Topics) pour révision ultérieure.

**Contraintes clés**

- Hébergement **local**
- **SQLite (Embedded)**
- **IA locale** ou auto-hébergée
- Identification utilisateur **simple**
- Architecture maintenable, évolutive

## 1.2 Stack technique

- **Backend** : ASP.NET Core Web API, C# 13
- **Frontend** : React 18 + TypeScript, Vite, shadcn/ui, Tailwind CSS, Lucide icons
- **LLM** : Ollama (local) ou AnythingLLM
- **Stockage** : SQLite (.db) non chiffré (pour debug facile)
- **ORM** : Entity Framework Core

---

## 1.2.1 Contraintes Techniques de Stockage ⚠️

### Règle Fondamentale : SQLite pour V1

**Obligatoire :**
- ✅ Fichier database unique stocké localement dans `/Data/`
- ✅ Application autonome, déployable localement
- ✅ Fonctionne hors ligne sans dépendances externes

**Interdit :**
- ❌ SQL Server, PostgreSQL, MySQL (nécessitent installation/serveur externe)
- ❌ Bases de données cloud (nécessitent connexion internet)
- ❌ Toute dépendance nécessitant configuration utilisateur (connection string complexe, etc.)

### Alternatives Acceptables (Si Complexité le Nécessite)

Si un agent IA détecte que SQLite devient insuffisant pour des besoins spécifiques, **UNIQUEMENT** les alternatives suivantes sont acceptables :

1. **SQLite** ✅
   - Base de données embarquée (fichier unique)
   - Aucune installation requise
   - Fonctionne hors ligne
   - Gérée de façon autonome par l'application

2. **LiteDB** ✅ (Recommandé pour .NET)
   - Base de données NoSQL embarquée
   - DLL unique, aucune installation
   - Fonctionne hors ligne
   - Natif .NET

3. **RavenDB Embedded** ✅
   - Base de données NoSQL embarquée
   - Aucune installation requise
   - Fonctionne hors ligne

### Justification

L'application "Derot My Brain" doit être :
- **Portable** : Copier/coller le dossier suffit pour déployer
- **Autonome** : Aucune dépendance externe à installer
- **Offline-first** : Fonctionne sans connexion internet
- **Simple** : Pas de configuration complexe pour l'utilisateur

### Structure de Stockage

```
/Data/
├── derot-my-brain.db              # Base de données SQLite principale
│   ├── Users                      # Table utilisateurs
│   ├── UserPreferences            # Table préférences
│   └── Activities                 # Table activités
├── seed/                          # Données de référence immuables
│   ├── categories.json            # 13 catégories Wikipedia
│   └── themes.json                # 5 thèmes de couleurs
└── config/                        # Configuration globale
    └── app-config.json            # URL LLM, paramètres globaux
```

---

## 1.3 Parcours utilisateur global

- Page d'identification
- Sélection des axes d'intérêt
- Consultation d'un article Wikipédia
- Quiz
- Résultats
- Résultats
- Historique & Sujets Suivis ("My Brain")

## 1.4 Fonctionnalités détaillées (ordre d'implémentation)

### 1.4.0 Initialisation de l'application et configuration

**Description**

Système d'initialisation de l'application au premier démarrage, incluant les données de référence (seed data) et la configuration globale.

**Données de référence (Seed Data)**

Données immuables déployées avec l'application :

1. **Catégories Wikipedia** (13 catégories officielles)
   - Stockées dans `/Data/seed/categories.json`
   - Chaque catégorie contient :
     - ID unique (ex: "culture-arts")
     - Nom en anglais (ex: "Culture and the arts")
     - Nom en français (ex: "Culture et arts")
     - Ordre d'affichage
   - Initialisées au premier démarrage
   - Immuables (ne peuvent pas être modifiées par les utilisateurs)

2. **Thèmes** (5 palettes de couleurs)
   - Stockées dans `/Data/seed/themes.json`
   - Chaque thème contient :
     - ID unique (ex: "derot-brain")
     - Nom (ex: "Derot Brain")
     - Description
     - Indicateur de thème par défaut
   - Thèmes disponibles :
     - Curiosity Loop (Dark/Blue)
     - Derot Brain (Dark/Violet) - **Par défaut**
     - Knowledge Core (Dark/Cyan)
     - Mind Lab (Dark/Teal)
     - Mind Lab (Dark/Teal)
     - Neo-Wikipedia (Light/Blue)

**Menu "My Brain"**

Nouveau regroupement dans la navigation :
- **My Brain** (Mon Cerveau) : Point d'entrée unique regroupant :
  - **Historique** : Timeline de toutes les sessions
  - **Sujets Suivis** (Tracked Topics) : Liste consolidée des sujets à travailler/maîtriser ("Favoris")
  - Métaphore : "Historique = Ce que j'ai vécu", "Sujets Suivis = Ce que je retiens/veux retenir"

**Configuration globale**

Configuration partagée entre tous les utilisateurs :

- **Configuration LLM** :
  - URL du serveur LLM (ex: "http://localhost:11434")
  - Port (ex: 11434)
  - Provider (Ollama, AnythingLLM, OpenAI)
  - Modèle par défaut (ex: "llama3:8b")
  - Timeout en secondes
- Stockée dans `/Data/config/app-config.json` pour la valeur de SeedData
- Modifiable via API (endpoints admin)
- Valeurs par défaut créées au premier démarrage

**Processus d'initialisation**

1. Vérification de l'existence des données de référence
2. Si absentes, initialisation depuis les fichiers seed
3. Vérification de la configuration globale
4. Si absente, création avec valeurs par défaut
5. Journalisation du statut d'initialisation

**Règles**

- Initialisation idempotente (peut être exécutée plusieurs fois sans duplication)
- Seed data immuable (pas de modification/suppression par utilisateurs)
- Configuration globale modifiable uniquement via API
- Validation des données lors de l'initialisation

### 1.3.1 Identification utilisateur

**Description**

Permet d'identifier l'utilisateur sans authentification lourde.

**Fonctionnement**

- Champ texte : **Nom de l'utilisateur**
- Liste des utilisateurs existants (cliquable)
- Un clic sur un nom pré-remplit et valide la sélection

**Règles**

- Le nom est l'unique identifiant
- Création automatique si le nom n'existe pas
- Persistance dans un fichier local
- **Session persistante** : L'utilisateur reste connecté après rafraîchissement de la page
  - Session stockée en localStorage/sessionStorage
  - Validation de session au démarrage de l'application
  - Redirection vers login uniquement si session invalide
- **Application des préférences** :
  - **Connexion (Login)** : Au moment de la connexion, les préférences sauvegardées de l'utilisateur (Langue, Thème) doivent être **immédiatement chargées et appliquées** à l'interface.
  - **Création (Sign up)** : Lorsqu'un utilisateur est créé, les préférences **actives sur la page d'accueil** (Langue actuelle, Thème actuel) doivent être enregistrées comme ses préférences par défaut.

**Données stockées**

```json
{
  "users": [
    {
      "id": "unique-guid",
      "name": "Alex",
      "createdAt": "2026-01-10",
      "lastConnectionAt": "2026-01-18",
      "preferences": {
        "questionCount": 10,
        "preferredTheme": "derot-brain"
      }
    }
  ]
}
```



### 1.3.2 Page d'accueil pour nouveaux utilisateurs

**Description**

Page affichée lors de la première visite d'un utilisateur pour expliquer le fonctionnement de l'application.
Elle peut être ré-affichée si l'utilisateur le souhaite depuis sa page de paramètres.

**Fonctionnement**

- Détection automatique des nouveaux utilisateurs (via localStorage ou cookie)
- Affichage d'un message de bienvenue
- Trois options proposées :
  1. **"Lire le guide"** : Affiche un guide détaillé expliquant l'application
  2. **"Utiliser l'application"** : Passe directement à l'application
  3. **"Ne plus afficher"** : Passe à l'application et enregistre la préférence

**Contenu du guide**

- Objectif de l'application (apprentissage actif via Wikipédia)
- Fonctionnalités principales :
  - Lecture d'articles Wikipédia
  - Génération de quiz par IA
  - Génération de quiz par IA
  - Historique des activités
  - Sujets suivis (Tracked Topics)
- Explication "pour les nuls" (langage simple, visuel)
- Accessible ultérieurement depuis le menu d'aide

**Règles**

- Préférence stockée en localStorage : `hasSeenWelcome`
- Guide accessible à tout moment depuis le menu
- Design cohérent avec le système de thèmes


### 1.3.2a Internationalisation (i18n)

**Description**

Système de traduction complet permettant l'utilisation de l'application en anglais et en français.

**Fonctionnement**

- **Fichiers de ressources** : Tous les textes de l'interface stockés dans des fichiers JSON
  - `/src/locales/en.json` - Traductions anglaises
  - `/src/locales/fr.json` - Traductions françaises
- **Détection automatique** : Langue du navigateur détectée au premier lancement
- **Sélection manuelle** : Choix de la langue dans les préférences utilisateur
- **Persistance** : Préférence de langue sauvegardée dans les données utilisateur

**Contenu traduit**

- Menu de navigation
- Titres et en-têtes de pages
- Libellés de boutons
- Libellés et placeholders de formulaires
- Messages d'erreur
- Tooltips et textes d'aide
- Contenu de la page d'accueil
- Contenu du guide

**Structure des fichiers de traduction**

```json
{
  "nav": {
    "derot": "Derot",
    "my_brain": "My Brain",
    "tracked_topics": "Sujets Suivis",
    "history": "Historique",
    "profile": "Profil",
    "preferences": "Préférences",
    "guide": "Guide",
    "logout": "Déconnexion"
  },
  "common": {
    "save": "Enregistrer",
    "cancel": "Annuler",
    "delete": "Supprimer",
    "edit": "Modifier",
    "confirm": "Confirmer"
  }
  // ... etc
}
```

**Règles**

- Aucun texte codé en dur dans les composants
- Format de date/heure d'affichage adapté à la langue sélectionnée
- Langue par défaut : détection automatique ou français
- Changement de langue immédiat (pas de rechargement de page)


### 1.3.3 Préférences de catégories Wikipedia

**Description**

Système permettant aux utilisateurs de sélectionner leurs catégories Wikipedia préférées pour filtrer la sélection d'articles. Pas de profils nommés - juste une liste simple de catégories cochables dans les préférences.

**Catégories disponibles**

Liste des **13 catégories officielles Wikipedia** :
1. General reference (Référence générale)
2. Culture and the arts (Culture et arts)
3. Geography and places (Géographie et lieux)
4. Health and fitness (Santé et forme)
5. History and events (Histoire et événements)
6. Human activities (Activités humaines)
7. Mathematics and logic (Mathématiques et logique)
8. Natural and physical sciences (Sciences naturelles et physiques)
9. People and self (Personnes et soi)
10. Philosophy and thinking (Philosophie et pensée)
11. Religion and belief systems (Religion et systèmes de croyance)
12. Society and social sciences (Société et sciences sociales)
13. Technology and applied sciences (Technologie et sciences appliquées)

**Configuration par défaut**

Pour les nouveaux utilisateurs, **TOUTES les 13 catégories sont cochées** par défaut.
- L'utilisateur peut décocher les catégories qui ne l'intéressent pas
- Au moins une catégorie doit rester cochée pour pouvoir soumettre le formulaire
- Les préférences sont sauvegardées dans les données utilisateur

**Gestion des catégories**

- Accessible depuis la page **Préférences**
- Section dédiée : "Catégories Wikipedia"
- Interface :
  - 13 cases à cocher (une par catégorie)
  - Chaque case à cocher est accompagnée d'un libellé traduit
  - Compteur : "X/13 catégories sélectionnées"
  - Boutons "Tout sélectionner" / "Tout désélectionner"
  - Bouton "Enregistrer" pour sauvegarder
- Validation :
  - Au moins une catégorie doit être cochée pour soumettre le formulaire
  - Message d'information quand aucune catégorie n'est cochée

**Utilisation sur la page Derot**

- Section de filtrage en haut de la page
- Affichage des catégories sélectionnées depuis les préférences
- Modifications temporaires possibles (voir section 1.3.13 pour détails)
- Filtrage des articles Wikipedia selon les catégories cochées

**Règles**

- Nouveaux utilisateurs : Toutes les catégories cochées par défaut
- Au moins une catégorie doit rester cochée
- Catégories sauvegardées dans les préférences utilisateur
- Catégories synchronisées entre sessions
- Les 13 catégories sont fixes (provenant de l'API Wikipedia)

### 1.3.3 Sélection et affichage d'un article Wikipédia

**Source**

- Wikipédia (via API ou dump local selon implémentation)

**Fonctionnalités**

- Affichage du contenu de la page (texte principal)
- Bouton **"Recycler"** :
  - Charge une nouvelle page
  - Ne sauvegarde rien dans l'historique

**Contraintes UX**

- Pas d'édition du contenu
- Lecture libre sans timer

### 1.3.4 Passage au quiz

**Déclenchement**

Bouton **"Passer au quiz"**

**Comportement**

- Génération de **questions configurables** (5, 10, 15 ou 20) liées au contenu de l'article
  - Nombre de questions défini dans les préférences utilisateur
  - Valeur par défaut : 10 questions
- Questions générées à la volée (non stockées)
- Types de questions :
  - Réponses courtes (texte libre)
  - Faits précis (dates, concepts, définitions simples)
- **Informations LLM enregistrées** :
  - Nom du modèle utilisé (ex: "llama3:8b")
  - Version du modèle

### 1.3.5 Quiz - déroulement

**Pour chaque question**

- Affichage de la question
- Champ de réponse texte
- Validation manuelle par l'utilisateur

**Évaluation**

- Comparaison réponse utilisateur / réponse attendue
- Utilisation d'un **seuil d'acceptation sémantique**
  - Pas de mot-à-mot strict
  - Tolérance aux synonymes / reformulations

### 1.3.6 Résultats du quiz

**Affichage**

- Score global avec notation **X/Y (Z%)** 
  - X = nombre de réponses correctes
  - Y = nombre total de questions (5, 10, 15 ou 20)
  - Z = pourcentage calculé : (X / Y) × 100
  - Exemple : 7/10 (70%)
- Pour chaque question :
  - Réponse utilisateur
  - Réponse attendue "idéale"
  - Indication correct/incorrect
- **Informations LLM** : Modèle utilisé pour générer le quiz (affiché au survol ou dans les détails)

**Actions possibles**

- Bouton **"Ajouter au backlog"**
- Bouton **"Retour accueil"**
- Bouton **"Nouvel article"**

### 1.3.7 Historique utilisateur

**Description**

Liste chronologique immuable de toutes les sessions. Chaque entrée correspond à une session unique (Lecture ou Quiz).

**Règles de gestion : Création d'une entrée**
1.  **Session "READ" (Lecture)** :
    *   Créée si l'utilisateur a atteint le bas de la page (scroll) **OU** a cliqué sur "Passer au Quiz".
    *   Si l'utilisateur quitte avant, pas d'entrée.
2.  **Session "QUIZ"** :
    *   Créée si l'utilisateur a **soumis ses réponses** au backend.
    *   Le type "Quiz" remplace le type "Read" pour cette session spécifique si les deux conditions sont remplies.

**Champs affichés**

*   **Type** : Badge "Lecture" ou "Quiz"
*   **Sujet** : Titre de l'article
*   **Date** : Date de la session
*   **Score Session** : X/Y (si Quiz)
*   **Comparaison (si Sujet Suivi)** :
    *   Affichage "Split Card" (bicolonne sur Desktop / Vertical sur Mobile)
    *   Gauche : Score de *cette session*
    *   Droite : Meilleur score historique (*Personal Best*)
    *   *Carte Festive* : Si le score de la session est un nouveau record !
*   **Indicateur Suivi** : Icône ⭐ si le sujet est dans les "Tracked Topics"

**Actions**

- Clic sur le sujet : Relancer un quiz sur cet article
- Bouton **"Ajouter au backlog"** (si non déjà présent)
- Icône backlog cliquable pour accéder directement au backlog

**Fonctionnalités supplémentaires**

- Tri par : date, score, titre
- Filtrage : tous / dans le backlog / hors backlog
- Recherche par titre d'article

### 1.3.8 Backlog utilisateur

**Description**

Page dédiée listant les sujets que l'utilisateur souhaite retravailler ultérieurement.

**Affichage**

- Grille ou liste des articles en backlog
- Pour chaque article :
  - Titre de la page Wikipédia
  - Date d'ajout au backlog
  - Date de dernière tentative (si applicable)
  - Meilleur score obtenu (si déjà tenté) : X/Y (Z%)
  - Actions : "Démarrer le quiz", "Retirer du backlog"
- Message d'état vide si aucun article dans le backlog
- Fonctionnalités de recherche et filtrage

**Règles**

- Contient uniquement :
  - Titre de la page Wikipédia
  - Lien / identifiant de la page
  - Métadonnées (dates, scores)
- **Aucune question stockée**
- À chaque nouvelle tentative :
  - Questions régénérées par le LLM
  - Nouveau quiz indépendant
- Accessible depuis :
  - Menu de navigation principal
  - Page d'historique (via icône backlog)
  - Page Derot (bouton "Ajouter au backlog")

### 1.3.9 Persistance des données

### 1.3.9 Persistance des données

**Contraintes**

- **SQLite** : Base de données locale unique
- **Entity Framework Core** : Code-First approach
- **Pas de SQL Server** ou base externe

**Structure (Schéma Simplifié)**

**Table Users**
- `Id` (PK, Text)
- `Name` (Text)
- `CreatedAt` (Text)

**Table UserPreferences**
- `UserId` (PK/FK, Text)
- `QuestionCount` (Integer)
- `PreferredTheme` (Text)
- `Language` (Text)

**Table Activities**
- `Id` (PK, Text)
- `UserId` (FK, Text)
- `Topic` (Text)
- `WikipediaUrl` (Text)
- `LastScore` (Integer)
- `BestScore` (Integer)
- ... (autres champs)


### 1.3.10 Navigation et structure des pages

**Menu de navigation principal**

- Accessible depuis toutes les pages de l'application
- Options de navigation :
  - **Derot** : Page principale (lecture + quiz)
  - **Historique** : Historique des activités
  - **Backlog** : Articles sauvegardés
  - **Profil** : Informations utilisateur
  - **Préférences** : Paramètres utilisateur
  - **Guide** : Aide et guide d'utilisation
  - **Déconnexion** : Retour à la page de login
- Design responsive (sidebar sur desktop, hamburger menu sur mobile)
- Indication visuelle de la page active

### 1.3.11 Page Profil utilisateur

**Description**

Page affichant les informations de l'utilisateur avec possibilité de modification.

**Informations affichées**

- **Nom** : Modifiable
- **ID utilisateur** : Lecture seule
- **Date de création du compte** : Lecture seule
- **Dernière connexion** : Lecture seule
- **Statistiques** :
  - Nombre d'articles suivis (Tracked Topics)
  - Score moyen
  - Meilleur score global
  - Historique complet des sessions

**Fonctionnalités**

- Mode édition pour modifier le nom
- Boutons "Enregistrer" / "Annuler" en mode édition
- Validation du nom (non vide, longueur max)
- Lien vers la page de préférences
- Bouton d'export des données

### 1.3.12 Page Préférences utilisateur

**Description**

Page dédiée à la configuration des paramètres utilisateur.

**Paramètres disponibles**

**Paramètres de quiz**
- **Nombre de questions** : Sélection entre 5, 10, 15 ou 20
  - Boutons radio ou menu déroulant
  - Valeur par défaut : 10
  - Indication visuelle de la sélection actuelle

**Paramètres d'interface**
- **Thème** : Sélecteur de thème (réutilisation du composant existant)
- Prévisualisation du thème sélectionné

**Paramètres futurs** (placeholders)
- Difficulté des questions
- Langue de l'interface
- Notifications

**Actions**
- Bouton "Enregistrer" : Sauvegarde les préférences
- Bouton "Annuler" : Annule les modifications
- Notification de succès/erreur après sauvegarde

### 1.3.13 Page Derot (Lecture et Quiz)

**Description**

Page principale de l'application où l'utilisateur lit des articles Wikipédia et passe des quiz.

**Filtrage par catégories**

- **Section de filtrage des catégories** en haut de la page :
  - Affichage des catégories sélectionnées depuis les préférences utilisateur
  - Cases à cocher ou chips/badges pour chaque catégorie
  - Compteur : "X/13 catégories sélectionnées"

- **Comportement du filtre selon le contexte** :
  - **Nouvelle activité** (au chargement initial de la page) :
    - Catégories chargées depuis les préférences utilisateur
    - Utilisateur peut modifier la sélection temporairement
    - Modifications **non sauvegardées** sauf si bouton "Sauvegarder" cliqué
    - Indicateur visuel : "⚠️ Modifications temporaires (non sauvegardées)"
  
  - **Activité depuis Historique/Sujets Suivis** :
    - Filtre de catégories **masqué ou désactivé** (grisé)
    - Message affiché : "Filtre de catégories non disponible lors du retravail d'un article"
    - Impossible de modifier les catégories
  
  - **Après clic sur "Recycler"** :
    - Filtre de catégories **réactivé**
    - **Toutes les catégories décochées** (reset complet)
    - Utilisateur doit re-sélectionner des catégories ou cliquer sur "Charger depuis préférences"

- **Modifications temporaires** :
  - Cases à cocher pour les 13 catégories
  - Sélection actuelle mise en évidence
  - Compteur : "X/13 catégories sélectionnées"
  - **Indicateur d'avertissement** quand différent des préférences sauvegardées
  - **Bouton "Sauvegarder dans préférences"** apparaît si modifications :
    - Clic ouvre modale de confirmation :
      - "Sauvegarder ces X catégories dans vos préférences ?"
      - "Cela mettra à jour votre sélection par défaut de catégories"
      - Options : "Sauvegarder", "Annuler"
  - **Bouton "Charger depuis préférences"** :
    - Recharge les catégories depuis les préférences utilisateur
    - Annule les modifications temporaires
  - **Bouton "Reset"** :
    - Décoche TOUTES les catégories
    - Disponible uniquement pour nouvelle activité (pas depuis backlog/historique)

**Zone de lecture**

- Affichage de l'article Wikipédia (contenu principal)
- Rendu markdown du contenu
- Titre de l'article bien visible
- Boutons d'action :
  - **"Recycler"** : 
    - Charge un nouvel article sans sauvegarder
    - **Réinitialise le filtre** (décoche toutes les catégories)
    - Réactive le filtre de catégories
  - **"Suivre ce sujet"** (Track Topic) :
    - Sauvegarder l'article dans les "Tracked Topics" pour le revoir plus tard
    - Icône ⭐ simple et visible
  - **"Démarrer le quiz"** : Lancer le quiz sur cet article

**Accès rapide**

- Sidebar ou drawer pour accéder à :
  - **My Brain** (Historique & Sujets Suivis)
- **Préservation de l'état** : L'article en cours reste chargé lors de la consultation de l'historique/sujets suivis
- Possibilité de fermer le drawer et revenir à l'article

**Zone de quiz**

- Affichage des questions (une par une ou toutes ensemble selon préférence)
- Champ de saisie pour chaque réponse
- Indicateur de progression (Question X/Y)
- Bouton "Soumettre" pour valider les réponses
- Affichage des résultats après soumission

**Règles importantes**

- L'article est enregistré dans l'historique **"Read"** si scroll bas de page OU clic "Passer au Quiz"
- L'article est enregistré dans l'historique **"Quiz"** (remplace Read) si réponses soumises
- Le bouton "Recycler" ne sauvegarde rien ET réinitialise le filtre (décoche toutes les catégories)
- Le filtre est désactivé/masqué quand on retravaille un article existant
- Au moins une catégorie doit être sélectionnée pour charger un article
- L'utilisateur est informé de ces règles via tooltips et guide

**Tooltips et aide contextuelle**

- "Recycler" : "Charger un nouvel article sans sauvegarder celui-ci (décoche toutes les catégories)"
- "Suivre" (⭐) : "Ajouter aux sujets suivis pour le maîtriser"
- "Démarrer le quiz" : "Commencer le quiz (l'article sera sauvegardé dans l'historique)"
- "Sauvegarder dans préférences" : "Sauvegarder cette sélection de catégories comme défaut"
- "Charger depuis préférences" : "Recharger les catégories sauvegardées"
- "Reset" : "Décocher toutes les catégories"
- Icône d'aide (?) pour plus d'informations

### 1.3.14 Export des données utilisateur

**Description**

Fonctionnalité permettant à l'utilisateur d'exporter toutes ses données au format JSON.

**Données exportées**

- **Profil utilisateur** : ID, nom, dates, préférences
- **Tracked Topics** : Liste complète des sujets suivis
- **Historique** (optionnel) : Toutes les activités avec scores et détails

**Interface**

- Bouton "Exporter mes données" dans la page Profil ou Préférences
- Modal de confirmation avec options :
  - Checkbox "Inclure l'historique complet"
  - Bouton "Télécharger"
  - Bouton "Annuler"
- Génération d'un fichier JSON téléchargeable
- Nom du fichier : `derot-export-{username}-{date}.json`

**Format d'export**

```json
{
  "exportDate": "2026-01-18T12:00:00Z",
  "user": {
    "id": "...",
    "name": "...",
    "createdAt": "...",
    "lastConnectionAt": "...",
    "preferences": { ... }
  },
  "backlog": [ ... ],
  "history": [ ... ] // Optionnel
}
```

### 1.3.15 Interface Utilisateur & Thèmes

**Système de thèmes**

- L'application doit proposer **5 thèmes prédéfinis** (référencés comme Color Palettes) :
  - **Curiosity Loop** (Dark / Blue)
  - **Derot Brain** (Dark / Violet - Défaut)
  - **Knowledge Core** (Dark / Cyan)
  - **Mind Lab** (Dark / Teal)
  - **Neo-Wikipedia** (Light / Blue)

**Fonctionnalités**

- Sélecteur de thème accessible depuis le header sur toutes les pages.
- Persistance du choix utilisateur (LocalStorage).
- Adaptation automatique de tous les composants (boutons, cartes, textes) au thème actif.
- Transitions douces lors du changement de thème.

## 2) Technologies recommandées (local / IA)

Je te propose **3 niveaux**, du plus simple au plus "propre/évolutif".

### 2.1 Frontend (recommandé)

**Option principale (cohérente avec ton profil)**

✅ **React + TypeScript**

- SPA simple
- Très bon pour l'UX (quiz, transitions)
- Facile à brancher sur API locale

Libs utiles :

- React Query / TanStack Query
- Zustand ou Redux Toolkit (léger)
- Markdown renderer (pour Wikipédia)

### 2.2 Backend applicatif

**Option recommandée**

✅ **ASP.NET Core Web API**

- Tu connais déjà .NET
- Parfait pour :
  - Gestion utilisateurs
  - Historique / backlog
  - Appels au LLM
  - Abstraction de la source Wikipédia

Stockage :

- Fichiers JSON
- Sérialisation native .NET

### 2.3 Wikipédia : récupération des données

**Option 1 (simple, suffisant pour POC)**

- **API Wikipédia officielle**
- Récupération :
  - Page aléatoire
  - Pages par catégorie

➡️ Avantage : zéro stockage lourd  
➡️ Inconvénient : dépendance réseau

**Option 2 (plus "local pur")**

- Dump Wikipédia + index partiel
- Très lourd → **non recommandé pour V1**

### 2.4 LLM local - génération de questions & évaluation

**Ton besoin réel**

- Génération de questions
- Extraction de réponses
- Évaluation sémantique simple

Pas besoin d'un monstre.

**🟢 Option A - Tout en local sur ta machine (recommandé)**

**Ollama**

- Très simple
- Expose une API HTTP locale
- S'intègre parfaitement avec .NET

Modèles recommandés :

- llama3:8b
- qwen2.5:7b
- mistral:7b

➡️ Ton **Ryzen 7 5700X** est largement suffisant  
➡️ Génération rapide, fluide

**🟡 Option B - LLM séparé mais toujours local**

- Backend .NET → appelle Ollama en HTTP
- Frontend React → appelle uniquement ton backend

Architecture propre, découplée.

**🔴 Option C - LLM sur ton homelab (moins recommandé)**

Vu ton **i5-6500**, ce serait :

- Lent
- Frustrant pour du quiz interactif

👉 À éviter sauf si :

- Tu déportes juste l'IA
- Et que tu acceptes la latence

### 2.5 Évaluation des réponses (seuil d'acceptation)

Approche recommandée :

- Prompt LLM du type :

"Compare la réponse utilisateur à la réponse attendue.  
Donne un score entre 0 et 1 selon la similarité sémantique."

Puis :

- Seuil configurable (ex : ≥ 0.7 = correct)

➡️ Simple  
➡️ Robuste  
➡️ Pas besoin d'embeddings complexes en V1
➡️ 5 questions -> note sur 5

### 2.6 Stack finale recommandée (V1)

**Frontend**

- React + TypeScript

**Backend**

- ASP.NET Core Web API

**IA**

- Ollama + LLM 7-8B

**Stockage**

- JSON files

**Hébergement**

- 100 % local (PC principal)