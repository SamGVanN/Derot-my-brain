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
- Le sujet peut être ajouté à un **backlog personnel** pour révision ultérieure.

**Contraintes clés**

- Hébergement **local** (PC utilisateur ou homelab)
- **Pas de base de données SQL**
- **IA locale** ou auto-hébergée
- Identification utilisateur **simple**
- Architecture simple, maintenable, évolutive

## 1.2 Parcours utilisateur global

- Page d'identification
- Sélection des axes d'intérêt
- Consultation d'un article Wikipédia
- Quiz
- Résultats
- Historique & backlog

## 1.3 Fonctionnalités détaillées (ordre d'implémentation)

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

**Données stockées**

```json
{
  "users": [
    {
      "name": "Alex",
      "createdAt": "2026-01-10"
    }
  ]
}
```



### 1.3.2 Sélection des axes d'intérêt

**Axes disponibles**

- Histoire / Géographie
- Sciences
- Économie
- Arts

**Comportement**

- Sélection **multi-choix**
- Les axes sélectionnés influencent le choix de la page Wikipédia
- Option "aucun filtre" possible (aléatoire total)

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

- Génération de **5 questions** liées au contenu de l'article
- Questions générées à la volée (non stockées)
- Types de questions :
  - Réponses courtes (texte libre)
  - Faits précis (dates, concepts, définitions simples)

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

- Score global (ex : 3 / 5)
- Pour chaque question :
  - Réponse utilisateur
  - Réponse attendue "idéale"

**Actions possibles**

- Bouton **"Ajouter au backlog"**
- Bouton **"Retour accueil"**

### 1.3.7 Historique utilisateur

**Description**

Liste chronologique des sujets consultés par l'utilisateur.

**Champs affichés**

- Sujet (titre de la page Wikipédia)
- Date de première consultation
- Dernier score obtenu

**Actions**

- Bouton **"Ajouter au backlog"** (si non déjà présent)

### 1.3.8 Backlog utilisateur

**Description**

Liste de sujets à retravailler ultérieurement.

**Règles**

- Contient uniquement :
  - Titre de la page Wikipédia
  - Lien / identifiant de la page
- **Aucune question stockée**
- À chaque nouvelle tentative :
  - Questions régénérées
  - Nouveau quiz indépendant

### 1.3.9 Persistance des données

**Contraintes**

- Pas de SQL
- Fichiers locaux

**Structure possible**

```json
{
  "user": "Alex",
  "history": [
    {
      "topic": "Révolution française",
      "firstSeen": "2026-01-10",
      "lastScore": 4
    }
  ],
  "backlog": [
    "Physique quantique"
  ]
}
```

### 1.3.10 Interface Utilisateur & Thèmes

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