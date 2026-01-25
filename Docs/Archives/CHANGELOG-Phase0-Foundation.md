# Phase 0 - Application Foundation & Configuration

**Date:** 2026-01-18  
**Version:** Phase 0 - Foundation Layer

---

## 🎯 Nouvelle Phase Critique

Ajout d'une **Phase 0 (Foundation)** qui doit être implémentée **AVANT toutes les autres tâches**.

Cette phase gère :
1. **Seed Data** (données immuables déployées avec l'app)
2. **Configuration Globale** (paramètres partagés entre utilisateurs)

---

## 📋 Justification

### Problème Identifié

L'application nécessite des données qui doivent être :
- ✅ **Systématiquement présentes** (déployées avec l'app)
- ✅ **Immuables** (ne changent pas)
- ✅ **Partagées** entre tous les utilisateurs

### Données Concernées

#### 1. **Catégories Wikipedia** (13 catégories officielles)
- Utilisées pour filtrer les articles
- Doivent être identiques pour tous les utilisateurs
- Ne doivent pas être modifiables

#### 2. **Thèmes** (5 palettes de couleurs)
- Déjà en dur dans l'application actuellement
- Doivent être centralisés et accessibles via API
- Permettra l'ajout futur de thèmes personnalisés

#### 3. **Configuration LLM** (URL:Port)
- URL du serveur Ollama/AnythingLLM
- Configuration globale (même pour tous les utilisateurs)
- Doit être modifiable sans redéploiement

---

## 🏗️ Architecture

### Seed Data (Données Immuables)

```
/Data/seed/
├── categories.json    # 13 catégories Wikipedia
└── themes.json        # 5 thèmes de couleurs
```

**Caractéristiques :**
- Déployées avec l'application
- Initialisées au premier démarrage
- Immuables (lecture seule pour utilisateurs)
- Accessibles via API GET uniquement

### Configuration Globale (Modifiable)

```
/Data/config/
└── app-config.json    # Configuration globale (LLM, etc.)
```

**Caractéristiques :**
- Créée au premier démarrage avec valeurs par défaut
- Modifiable via API PUT (admin)
- Partagée entre tous les utilisateurs
- Persistée entre redémarrages

---

## 📊 Modèles de Données

### WikipediaCategory (Seed Data)

```csharp
public class WikipediaCategory
{
    public string Id { get; set; }          // "culture-arts"
    public string Name { get; set; }        // "Culture and the arts"
    public string NameFr { get; set; }      // "Culture et arts"
    public int Order { get; set; }          // 1-13
    public bool IsActive { get; set; }      // true
}
```

**13 Catégories à Initialiser :**
1. general-reference
2. culture-arts
3. geography-places
4. health-fitness
5. history-events
6. human-activities
7. mathematics-logic
8. natural-sciences
9. people-self
10. philosophy-thinking
11. religion-belief
12. society-sciences
13. technology-sciences

---

### Theme (Seed Data)

```csharp
public class Theme
{
    public string Id { get; set; }          // "derot-brain"
    public string Name { get; set; }        // "Derot Brain"
    public string Description { get; set; } // "Dark theme with violet accents"
    public bool IsDefault { get; set; }     // true for derot-brain
    public bool IsActive { get; set; }      // true
}
```

**5 Thèmes à Initialiser :**
1. curiosity-loop (Dark/Blue)
2. derot-brain (Dark/Violet) - **Default**
3. knowledge-core (Dark/Cyan)
4. mind-lab (Dark/Teal)
5. neo-wikipedia (Light/Blue)

---

### AppConfiguration (Global Config)

```csharp
public class AppConfiguration
{
    public string Id { get; set; } = "global";
    public LLMConfiguration LLM { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class LLMConfiguration
{
    public string Url { get; set; }          // "http://localhost:11434"
    public int Port { get; }                 // Url.GetPort()
    public string Provider { get; set; }     // "ollama", "anythingllm", "openai"
    public string DefaultModel { get; set; } // "llama3:8b"
    public int TimeoutSeconds { get; set; }  // 30
}
```

**Valeurs par Défaut :**
- URL: `http://localhost:11434`
- Provider: `ollama`
- Model: `llama3:8b`
- Timeout: `30` secondes

---

## 🔌 API Endpoints

### Seed Data (Read-Only)

```
GET /api/categories
→ Retourne les 13 catégories Wikipedia

GET /api/themes
→ Retourne les 5 thèmes disponibles
```

### Configuration Globale (Read/Write)

```
GET /api/config
→ Retourne la configuration globale complète

PUT /api/config
→ Met à jour la configuration globale (admin)

GET /api/config/llm
→ Retourne uniquement la configuration LLM

PUT /api/config/llm
→ Met à jour la configuration LLM
```

---

## 🔄 Processus d'Initialisation

### Au Premier Démarrage

```
1. Application démarre
   ↓
2. Vérifier si /Data/seed/categories.json existe
   ↓ Non
3. Créer et initialiser categories.json (13 catégories)
   ↓
4. Vérifier si /Data/seed/themes.json existe
   ↓ Non
5. Créer et initialiser themes.json (5 thèmes)
   ↓
6. Vérifier si /Data/config/app-config.json existe
   ↓ Non
7. Créer app-config.json avec valeurs par défaut
   ↓
8. Logger: "Application initialized successfully"
   ↓
9. Application prête
```

### Aux Démarrages Suivants

```
1. Application démarre
   ↓
2. Charger seed data depuis fichiers
   ↓
3. Charger configuration globale
   ↓
4. Valider intégrité des données
   ↓
5. Application prête
```

---

## ✅ Avantages

### Pour le Déploiement
- ✅ Données de référence toujours présentes
- ✅ Pas de setup manuel requis
- ✅ Configuration par défaut fonctionnelle
- ✅ Idempotent (peut redémarrer sans problème)

### Pour la Maintenance
- ✅ Catégories centralisées (pas en dur dans le code)
- ✅ Thèmes centralisés (pas en dur dans le code)
- ✅ Configuration LLM modifiable sans redéploiement
- ✅ Facile d'ajouter de nouvelles catégories/thèmes

### Pour les Utilisateurs
- ✅ Application prête à l'emploi immédiatement
- ✅ Pas de configuration initiale requise
- ✅ Expérience cohérente entre utilisateurs

---

## 🔧 Impact sur les Autres Tâches

### Task 8.2 (Category Preferences)
**Avant :** Devait créer/gérer les catégories  
**Après :** Utilise les catégories depuis seed data

**Changements :**
- ✅ Pas besoin de créer les catégories
- ✅ Juste charger depuis `GET /api/categories`
- ✅ Validation automatique (catégories existent toujours)

### Task 2.2 (User Preferences - Themes)
**Avant :** Thèmes en dur dans le frontend  
**Après :** Thèmes chargés depuis API

**Changements :**
- ✅ Charger depuis `GET /api/themes`
- ✅ Permet ajout futur de thèmes personnalisés
- ✅ Centralisation de la gestion des thèmes

### Task 5.2 (Quiz Generation - LLM)
**Avant :** URL LLM en dur ou dans appsettings.json  
**Après :** URL LLM depuis configuration globale

**Changements :**
- ✅ Charger depuis `GET /api/config/llm`
- ✅ Modifiable sans redéploiement
- ✅ Gestion d'erreur si LLM non accessible

---

## 📝 Checklist d'Implémentation

### Backend

#### Seed Data
- [ ] Créer `/Data/seed/` directory
- [ ] Créer `categories.json` avec 13 catégories
- [ ] Créer `themes.json` avec 5 thèmes
- [ ] Créer `SeedDataService.cs`
- [ ] Implémenter initialisation idempotente
- [ ] Ajouter endpoints GET pour categories et themes

#### Configuration Globale
- [ ] Créer `/Data/config/` directory
- [ ] Créer modèles `AppConfiguration` et `LLMConfiguration`
- [ ] Créer `ConfigurationService.cs`
- [ ] Implémenter création config par défaut
- [ ] Ajouter endpoints GET/PUT pour configuration
- [ ] Validation des données de configuration

#### Initialisation
- [ ] Créer `InitializationService.cs`
- [ ] Appeler au démarrage de l'application
- [ ] Logger les étapes d'initialisation
- [ ] Gestion d'erreurs robuste
- [ ] Tests unitaires

### Frontend

#### Intégration
- [ ] Créer `useCategories` hook
- [ ] Créer `useThemes` hook
- [ ] Créer `useAppConfig` hook
- [ ] Afficher statut LLM dans settings
- [ ] Page admin pour modifier config LLM (optionnel V1)

---

## 🚀 Ordre d'Implémentation

### Sprint 0 (Avant tout)
**⚠️ CRITIQUE - À faire en premier**

1. **Task 0.1: Application Initialization & Configuration**
   - Seed data (categories + themes)
   - Global configuration (LLM)
   - Endpoints API
   - Tests

### Puis Sprint 1, 2, 3, etc.

Toutes les autres tâches dépendent de Task 0.1.

---

## 📊 Résumé

| Aspect | Détails |
|--------|---------|
| **Nouvelle Phase** | Phase 0 - Foundation |
| **Nouvelle Tâche** | Task 0.1 - Application Initialization |
| **Priorité** | **CRITICAL** |
| **Dépendances** | Aucune (doit être fait en premier) |
| **Complexité** | Medium |
| **Temps Estimé** | ~1 jour |
| **Impact** | Toutes les autres tâches |

---

## 📚 Documents Mis à Jour

| Document | Modifications | Statut |
|----------|---------------|--------|
| **Implementation-Roadmap.md** | Ajout Phase 0 + Task 0.1 | ✅ |
| **functional_specifications_derot_my_brain.md** | Section 1.3.0 ajoutée | ✅ |
| **Project-Status.md** | Phase 0 ajoutée | ✅ |
| **CHANGELOG-Phase0-Foundation.md** | Nouveau changelog | ✅ |

---

## 🎯 Prochaines Étapes

1. **Implémenter Task 0.1** en priorité absolue
2. Valider que seed data s'initialise correctement
3. Tester endpoints API
4. Vérifier configuration LLM par défaut
5. **Puis** commencer Sprint 1 (Task 8.1 - i18n)

---

**Total des tâches : 17 → 18 tâches**  
**Nouvelle structure : Phase 0 + Phases 1-8**

---

**Fin du Changelog - Phase 0 Foundation**
