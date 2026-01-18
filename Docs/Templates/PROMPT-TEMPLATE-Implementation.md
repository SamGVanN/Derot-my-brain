# Prompt Template pour Ajouter des tâches dans les /Docs selon besoin
```
Met à jours /Docs (à minima Specifications-fonctionnelles.md, Implementation-Roadmap.md, Project-Status.md) pour la feature ou tâche suivante à implémenter.
Tu n'implémente rien, tu prépare le terrain et maintiens à jours les instructions pour l'agent développeur.

FEATURE À AJOUTER :
[FEATURE-DESCRIPTION-DETAILED]

IMPORTANT :
- Ne PAS implémenter la feature
- Ne PAS modifier la codebase
- Soulève les potentielles contradictions avec les spécifications fonctionnelles
- Soulève les potentielles contradictions avec les spécifications techniques
- Soulève les potentielles contradictions avec les spécifications de stockage
- Demande des précisions si un point manque de clarté

Peux-tu commencer par lire la demande de feature et me confirmer que tu as bien compris la tâche avant de commencer ?
Reformule la demande de feature si nécessaire pour clarifier et organiser le besoin.

```


# Prompt Template pour Implémentation de Tâche

## 📋 Template de Base

```
Je veux implémenter la tâche [TASK_NUMBER] du projet "Derot My Brain".

CONTEXTE DU PROJET :
- Lire Docs/README.md pour comprendre l'organisation de la documentation
- Lire Docs/Project-Status.md pour voir l'état actuel du projet
- Lire Docs/TECHNICAL-CONSTRAINTS-Storage.md pour les contraintes de stockage (JSON UNIQUEMENT)

TÂCHE À IMPLÉMENTER :
- Lire la section "[TASK_NUMBER]" dans Docs/Implementation-Roadmap.md
- Suivre EXACTEMENT les spécifications, l'objectif et les critères d'acceptation
- Respecter les dépendances listées

SPÉCIFICATIONS FONCTIONNELLES :
- Lire la section correspondante dans Docs/Specifications-fonctionnelles.md
- Comprendre les besoins métier avant de coder

CONTRAINTES TECHNIQUES :
⚠️ CRITIQUE : Utiliser UNIQUEMENT des fichiers JSON pour le stockage (pas de SQL Server, PostgreSQL, etc.)
- Alternatives acceptables SI NÉCESSAIRE : SQLite, LiteDB, RavenDB Embedded
- Voir Docs/TECHNICAL-CONSTRAINTS-Storage.md pour détails

WORKFLOW D'IMPLÉMENTATION :
1. Lire et comprendre toute la documentation de la tâche
2. Vérifier que les dépendances sont complétées
3. Implémenter le backend selon les spécifications
4. Implémenter le frontend selon les spécifications
5. Tester selon les critères d'acceptation
6. Mettre à jour Docs/Project-Status.md :
   - Marquer la tâche comme complétée [x]
   - Mettre à jour le statut de "Not Started" à "Completed"
   - Ajouter la date de complétion

IMPORTANT :
- Ne PAS modifier les autres tâches dans Project-Status.md
- Ne PAS modifier Implementation-Roadmap.md (sauf si tu détectes une erreur)
- Suivre les standards de code du projet
- Documenter tout changement significatif

Peux-tu commencer par lire la documentation et me confirmer que tu as bien compris la tâche avant de commencer l'implémentation ?
```

---

## 🎯 Exemples Concrets

### Exemple 1 : Implémenter Task 0.1 (Foundation)

```
Je veux implémenter la tâche 0.1 (Application Initialization & Configuration) du projet "Derot My Brain".

CONTEXTE DU PROJET :
- Lire Docs/README.md pour comprendre l'organisation de la documentation
- Lire Docs/Project-Status.md pour voir l'état actuel du projet
- Lire Docs/TECHNICAL-CONSTRAINTS-Storage.md pour les contraintes de stockage (JSON UNIQUEMENT)
- Lire Docs/CHANGELOG-Phase0-Foundation.md pour comprendre les détails de cette phase

TÂCHE À IMPLÉMENTER :
- Lire la section "Task 0.1: Application Initialization & Configuration" dans Docs/Implementation-Roadmap.md
- Suivre EXACTEMENT les spécifications, l'objectif et les critères d'acceptation
- Cette tâche n'a AUCUNE dépendance - elle doit être faite EN PREMIER

SPÉCIFICATIONS FONCTIONNELLES :
- Lire la section "1.4.0 Initialisation de l'application et configuration" dans Docs/Specifications-fonctionnelles.md
- Comprendre les besoins métier avant de coder

CONTRAINTES TECHNIQUES :
⚠️ CRITIQUE : Utiliser UNIQUEMENT des fichiers JSON pour le stockage
- Seed data : /data/seed/categories.json et /data/seed/themes.json
- Config globale : /data/config/app-config.json
- Voir Docs/TECHNICAL-CONSTRAINTS-Storage.md pour détails

WORKFLOW D'IMPLÉMENTATION :
1. Lire et comprendre toute la documentation de la tâche
2. Créer la structure /data/ avec les sous-dossiers seed/ et config/
3. Implémenter SeedDataService pour initialiser les 13 catégories Wikipedia et 5 thèmes
4. Implémenter ConfigurationService pour gérer la config LLM
5. Créer les endpoints API (GET /api/categories, GET /api/themes, GET/PUT /api/config)
6. Tester l'initialisation idempotente
7. Mettre à jour Docs/Project-Status.md :
   - Marquer "Task 0.1" comme complétée [x]
   - Mettre à jour le statut de "Not Started" à "Completed"
   - Ajouter la date de complétion

IMPORTANT :
- Cette tâche est CRITIQUE - elle doit être faite AVANT toutes les autres
- Les catégories doivent être les 13 catégories officielles Wikipedia (voir roadmap)
- L'initialisation doit être idempotente (peut s'exécuter plusieurs fois sans erreur)
- Ne PAS utiliser de base de données SQL

Peux-tu commencer par lire la documentation et me confirmer que tu as bien compris la tâche avant de commencer l'implémentation ?
```

---

### Exemple 2 : Implémenter Task 8.1 (i18n)

```
Je veux implémenter la tâche 8.1 (Internationalization - i18n) du projet "Derot My Brain".

CONTEXTE DU PROJET :
- Lire Docs/README.md pour comprendre l'organisation de la documentation
- Lire Docs/Project-Status.md pour voir l'état actuel du projet
- Lire Docs/CHANGELOG-Phase8-Consolidated.md pour comprendre les détails de cette phase

TÂCHE À IMPLÉMENTER :
- Lire la section "Task 8.1: Internationalization (i18n) Implementation" dans Docs/Implementation-Roadmap.md
- Suivre EXACTEMENT les spécifications, l'objectif et les critères d'acceptation
- Dépendances : Task 0.1 doit être complétée

SPÉCIFICATIONS FONCTIONNELLES :
- Lire la section "1.4.2a Internationalisation (i18n)" dans Docs/Specifications-fonctionnelles.md
- Comprendre les besoins métier avant de coder

PRIORITÉ ABSOLUE :
⚠️ Cette tâche doit être faite EN PREMIER dans Sprint 1 pour éviter de refactoriser tout le code existant.
Tous les composants futurs doivent utiliser les traductions dès le départ.

WORKFLOW D'IMPLÉMENTATION :
1. Lire et comprendre toute la documentation de la tâche
2. Vérifier que Task 0.1 est complétée
3. Installer et configurer react-i18next
4. Créer /src/locales/en.json et /src/locales/fr.json
5. Configurer la détection automatique de la langue du navigateur
6. Créer le sélecteur de langue dans les préférences
7. Traduire TOUS les textes existants (aucun texte en dur)
8. Tester le changement de langue sans rechargement
9. Mettre à jour Docs/Project-Status.md :
   - Marquer "Task 8.1" comme complétée [x]
   - Mettre à jour le statut de "Not Started" à "Completed"
   - Ajouter la date de complétion

IMPORTANT :
- AUCUN texte ne doit être codé en dur dans les composants
- Tous les textes doivent être dans les fichiers de traduction
- Le changement de langue doit être immédiat (pas de rechargement)
- Utiliser les clés de traduction de manière cohérente

Peux-tu commencer par lire la documentation et me confirmer que tu as bien compris la tâche avant de commencer l'implémentation ?
```

---

### Exemple 3 : Implémenter Task 8.2 (Category Preferences)

```
Je veux implémenter la tâche 8.2 (Category Preferences Management) du projet "Derot My Brain".

CONTEXTE DU PROJET :
- Lire Docs/README.md pour comprendre l'organisation de la documentation
- Lire Docs/Project-Status.md pour voir l'état actuel du projet
- Lire Docs/CHANGELOG-Phase8-Consolidated.md pour comprendre la simplification (pas de profils nommés)

TÂCHE À IMPLÉMENTER :
- Lire la section "Task 8.2: Category Preferences Management" dans Docs/Implementation-Roadmap.md
- Suivre EXACTEMENT les spécifications, l'objectif et les critères d'acceptation
- Dépendances : Task 0.1 (Seed Data), Task 2.1 (User Preferences), Task 8.1 (i18n)

SPÉCIFICATIONS FONCTIONNELLES :
- Lire la section "1.4.3 Préférences de catégories Wikipedia" dans Docs/Specifications-fonctionnelles.md
- Comprendre les besoins métier avant de coder

POINTS CLÉS :
⚠️ VERSION SIMPLIFIÉE : Pas de profils nommés multiples, juste une liste simple de catégories cochables
- Les 13 catégories proviennent du seed data (Task 0.1)
- TOUTES les catégories sont cochées par défaut pour les nouveaux utilisateurs
- Section dans la page Préférences (pas de page dédiée)

WORKFLOW D'IMPLÉMENTATION :
1. Lire et comprendre toute la documentation de la tâche
2. Vérifier que les dépendances sont complétées
3. Charger les catégories depuis GET /api/categories (seed data)
4. Ajouter section "Wikipedia Categories" dans UserPreferencesPage.tsx
5. Afficher les 13 catégories avec checkboxes
6. Implémenter "Select All" / "Deselect All"
7. Sauvegarder dans UserPreferences.SelectedCategories
8. Par défaut : toutes les catégories cochées pour nouveaux utilisateurs
9. Tester la persistance entre sessions
10. Mettre à jour Docs/Project-Status.md :
    - Marquer "Task 8.2" comme complétée [x]
    - Mettre à jour le statut de "Not Started" à "Completed"
    - Ajouter la date de complétion

IMPORTANT :
- NE PAS créer de profils nommés (version simplifiée)
- Charger les catégories depuis l'API (ne pas les hardcoder)
- Utiliser les traductions (Name ou NameFr selon la langue)
- Au moins 1 catégorie doit rester cochée (validation)

Peux-tu commencer par lire la documentation et me confirmer que tu as bien compris la tâche avant de commencer l'implémentation ?
```

---

## 🎯 Checklist de Vérification pour l'Agent

Après avoir donné le prompt, l'agent devrait confirmer :

1. ✅ J'ai lu le README.md et compris l'organisation de la documentation
2. ✅ J'ai lu le Project-Status.md et vérifié l'état actuel
3. ✅ J'ai lu la tâche spécifique dans Implementation-Roadmap.md
4. ✅ J'ai lu les spécifications fonctionnelles correspondantes
5. ✅ J'ai compris les contraintes techniques (JSON uniquement)
6. ✅ J'ai vérifié que les dépendances sont complétées
7. ✅ Je sais quoi mettre à jour dans Project-Status.md après implémentation

---

## 📝 Template de Mise à Jour du Project-Status.md

Après l'implémentation, l'agent doit mettre à jour Project-Status.md comme suit :

```markdown
### Phase X: [Phase Name]

#### [Feature Name]
- [x] **Task X.Y: [Task Name]**: [Description]
  - [Détails de la tâche]
  - **Status:** Completed ✅
  - **Completed on:** 2026-01-18
  - **Roadmap Task:** X.Y
```

---

## 🚀 Utilisation Recommandée

### Pour Chaque Tâche :

1. **Copier le template de base**
2. **Remplacer [TASK_NUMBER]** par le numéro de tâche (ex: 0.1, 8.1, etc.)
3. **Ajouter les points clés spécifiques** à la tâche
4. **Mentionner le changelog** pertinent si disponible
5. **Envoyer le prompt** à l'agent

### Ordre Recommandé :

1. **Task 0.1** (Foundation) - CRITIQUE - À faire EN PREMIER
2. **Task 8.1** (i18n) - PRIORITÉ ABSOLUE dans Sprint 1
3. **Puis suivre l'ordre des sprints** dans Implementation-Roadmap.md

---

**Ce template garantit que l'agent :**
- ✅ Lit toute la documentation nécessaire
- ✅ Comprend le contexte avant de coder
- ✅ Respecte les contraintes techniques
- ✅ Met à jour la documentation correctement
- ✅ Suit les critères d'acceptation
