# Prompt Template pour Implémentation de Tâche

## 📋 Vue d'ensemble

Ce document contient le template principal pour l'implémentation des tâches du projet "Derot My Brain".

**Templates spécialisés disponibles :**
- [Backend.md](./Backend.md) - Implémentation backend uniquement
- [Frontend.md](./Frontend.md) - Implémentation frontend uniquement
- [Migration.md](./Migration.md) - Migrations et refactoring
- [QuickFix.md](./QuickFix.md) - Corrections de bugs
- [UI-UX.md](./UI-UX.md) - Améliorations UI/UX

---

## 📝 Template pour Ajouter des Features dans la Documentation

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

---

## 🎯 Template de Base pour Implémentation Complète

```
Je veux implémenter la tâche [TASK_NUMBER] du projet "Derot My Brain". Commence par prendre connaissance de ANTIGRAVITY_INSTRUCTIONS.md.

CONTEXTE DU PROJET :
- Lire Docs/README.md pour comprendre l'organisation de la documentation
- Lire Docs/Planning/Project-Status.md pour voir l'état actuel du projet
- Lire Docs/Technical/Storage-Policy.md pour les contraintes de stockage (JSON UNIQUEMENT)
- Lire Docs/Technical/Frontend-Architecture.md pour les principes d'architecture frontend (si tâche frontend)
- Lire Docs/Technical/Backend-Guidelines.md pour les principes d'architecture backend (si tâche backend)
- Lire Docs/Technical/Testing-Strategy.md pour la méthodologie TDD

TÂCHE À IMPLÉMENTER :
- Lire la section "[TASK_NUMBER]" dans Docs/Planning/Implementation-Roadmap.md
- Suivre EXACTEMENT les spécifications, l'objectif et les critères d'acceptation
- Respecter les dépendances listées

SPÉCIFICATIONS FONCTIONNELLES :
- Lire la section correspondante dans Docs/Planning/Specifications-fonctionnelles.md
- Comprendre les besoins métier avant de coder

CONTRAINTES TECHNIQUES :
⚠️ CRITIQUE : Utiliser UNIQUEMENT des fichiers JSON pour le stockage (pas de SQL Server, PostgreSQL, etc.)
- Alternatives acceptables SI NÉCESSAIRE : SQLite, LiteDB, RavenDB Embedded
- Voir Docs/Technical/Storage-Policy.md pour détails

MÉTHODOLOGIE TDD (CRITICAL) :
1. ✅ Écrire les tests AVANT le code d'implémentation
2. ✅ Red → Green → Refactor
3. ✅ Créer les données mock pour TestUser
4. ✅ Vérifier que la couverture de code ≥ 80%

WORKFLOW D'IMPLÉMENTATION :
1. Lire et comprendre toute la documentation de la tâche
2. Vérifier que les dépendances sont complétées
3. Écrire les tests (TDD)
4. Implémenter le backend selon les spécifications
5. Implémenter le frontend selon les spécifications
6. Créer les données mock pour TestUser
7. Tester selon les critères d'acceptation (back-end + front-end)
8. Mettre à jour Docs/Planning/Project-Status.md :
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

## 🎯 Exemples d'Utilisation

### Phase 0 - Foundation

**Task 0.1 - Application Initialization**
```
Je veux implémenter la tâche 0.1 (Application Initialization & Configuration) du projet "Derot My Brain".

[Utiliser le template de base ci-dessus]

POINTS SPÉCIFIQUES :
- Cette tâche n'a AUCUNE dépendance - elle doit être faite EN PREMIER
- Seed data : /data/seed/categories.json et /data/seed/themes.json
- Les 13 catégories doivent être les catégories officielles Wikipedia
- L'initialisation doit être idempotente
- Lire Docs/CHANGELOG-Phase0-Foundation.md pour les détails
```

### Phase 2 - i18n & Preferences

**Task 8.1 - Internationalization**
```
Je veux implémenter la tâche 8.1 (Internationalization - i18n) du projet "Derot My Brain".

[Utiliser le template de base ci-dessus]

POINTS SPÉCIFIQUES :
⚠️ PRIORITÉ ABSOLUE : Cette tâche doit être faite EN PREMIER dans Sprint 1
- AUCUN texte ne doit être codé en dur dans les composants
- Le changement de langue doit être immédiat (pas de rechargement)
- Lire Docs/CHANGELOG-Phase8-Consolidated.md pour les détails
```

**Task 8.2 - Category Preferences**
```
Je veux implémenter la tâche 8.2 (Category Preferences Management) du projet "Derot My Brain".

[Utiliser le template de base ci-dessus]

POINTS SPÉCIFIQUES :
⚠️ VERSION SIMPLIFIÉE : Pas de profils nommés multiples
- TOUTES les catégories cochées par défaut pour nouveaux utilisateurs
- Au moins 1 catégorie doit rester cochée (validation)
- Charger les catégories depuis l'API (ne pas les hardcoder)
```

### Phase -1 - Frontend Architecture

**Task -1.1 - Infrastructure Layer**
```
Je veux implémenter la tâche -1.1 (Infrastructure Layer Setup) du projet "Derot My Brain".

[Utiliser le template de base ci-dessus]

POINTS SPÉCIFIQUES :
- Créer /api directory structure (client.ts, endpoints.ts, userApi.ts, categoryApi.ts)
- Migrer UserService.ts → userApi.ts
- Éliminer le mélange axios/fetch
- Lire Docs/Technical/Frontend-Architecture.md section "Infrastructure Layer"
```

**Task -1.3 - Custom Hooks**
```
Je veux implémenter la tâche -1.3 (Custom Hooks Implementation) du projet "Derot My Brain".

[Utiliser le template de base ci-dessus]

POINTS SPÉCIFIQUES :
⚠️ CRITIQUE : Extraire TOUTE la logique métier des composants vers des custom hooks
- Créer hooks/useAuth.ts, useUser.ts, usePreferences.ts, useHistory.ts
- Les composants deviennent "dumb" (présentation uniquement)
- Dépendances : Task -1.1 et -1.2 doivent être complétées
```

### Phase 3 - Navigation

**Task 3.1 - Main Navigation Menu**
```
Je veux implémenter la tâche 3.1 (Main Navigation Menu) du projet "Derot My Brain".

[Utiliser le template de base ci-dessus]

POINTS SPÉCIFIQUES :
- Menu : Derot, My Brain (dropdown: History + Tracked Topics), Profile, Preferences, Guide, Logout
- Utiliser shadcn/ui components (NavigationMenu, DropdownMenu)
- Composant doit être "dumb" (utiliser useAuth() hook)
- TERMINOLOGIE : "Backlog" → "Tracked Topics"
```

---

## 🧪 Méthodologie TDD

Pour une implémentation stricte TDD, suivre le cycle **Red → Green → Refactor** :

### 1. 🔴 RED PHASE - Écrire les tests (qui échouent)
- Définir les cas de test (nominal, edge cases, erreurs)
- Écrire les tests unitaires (backend) ou component tests (frontend)
- Vérifier que les tests échouent

### 2. 🟢 GREEN PHASE - Implémenter le code minimal
- Écrire le code minimal pour faire passer les tests
- Ne pas optimiser, juste faire fonctionner

### 3. 🔵 REFACTOR PHASE - Améliorer le code
- Nettoyer le code (DRY, SOLID)
- Améliorer la lisibilité
- Vérifier que les tests passent toujours

### 4. 📊 COVERAGE PHASE - Vérifier la couverture
- Exécuter l'outil de couverture de code
- Vérifier que la couverture ≥ 80%

**Exemple Backend (C#):**
```csharp
// RED: Test qui échoue
[Fact]
public async Task GetUserById_ShouldReturnUser_WhenUserExists()
{
    var userId = "test-user-id";
    var result = await _userService.GetUserById(userId);
    Assert.NotNull(result);
    Assert.Equal(userId, result.Id);
}

// GREEN: Code minimal
public async Task<User> GetUserById(string userId)
{
    return await _repository.GetByIdAsync(userId);
}

// REFACTOR: Code amélioré
public async Task<User> GetUserById(string userId)
{
    if (string.IsNullOrEmpty(userId))
        throw new ArgumentException("User ID cannot be null or empty");
    
    var user = await _repository.GetByIdAsync(userId);
    if (user == null)
        throw new NotFoundException($"User with ID {userId} not found");
    
    return user;
}
```

**Exemple Frontend (TypeScript):**
```typescript
// RED: Test qui échoue
it('should login user successfully', async () => {
    const { result } = renderHook(() => useAuth());
    await act(() => result.current.login('test-user-id'));
    expect(result.current.isAuthenticated).toBe(true);
});

// GREEN: Code minimal
export const useAuth = () => {
    const login = async (userId: string) => {
        const user = await userApi.getUserById(userId);
        useAuthStore.getState().setUser(user);
    };
    return { login, /* ... */ };
};

// REFACTOR: Code amélioré avec gestion d'erreur
export const useAuth = () => {
    const login = async (userId: string) => {
        try {
            const user = await userApi.getUserById(userId);
            useAuthStore.getState().setUser(user);
            usePreferencesStore.getState().loadPreferences(user.preferences);
        } catch (error) {
            console.error('Login failed:', error);
            throw error;
        }
    };
    return { login, /* ... */ };
};
```

---

## 📦 Mock Data pour TestUser

**Structure des données :**
```
/data/users/
  ├── users.json                          # Profil et préférences
  ├── user-{testuser-id}-history.json     # Historique d'activité
  └── user-{testuser-id}-tracked.json     # Sujets suivis
```

**Critères de qualité :**
- [ ] Données réalistes et cohérentes
- [ ] Couvre les cas nominaux ET les edge cases
- [ ] Timestamps valides et cohérents
- [ ] Références valides (catégories, thèmes, etc.)

**Edge cases à couvrir :**
- Données vides (nouveau utilisateur)
- Données au maximum (utilisateur très actif)
- Données avec valeurs limites (0, 100%, etc.)

---

## 🔍 Checklist de Vérification Post-Implémentation

### 1. ✅ Critères d'Acceptation
- [ ] Tous les critères de Implementation-Roadmap.md sont remplis

### 2. ✅ Tests
- [ ] Tous les tests passent (backend + frontend)
- [ ] Couverture de code ≥ 80%
- [ ] Tests couvrent les edge cases

### 3. ✅ Architecture
- [ ] Respect de Technical/Frontend-Architecture.md (si frontend)
- [ ] Respect de Technical/Backend-Guidelines.md (si backend)
- [ ] Pas d'appels API directs dans les composants

### 4. ✅ Stockage
- [ ] Utilisation de JSON uniquement (pas de SQL)
- [ ] Pas de hardcoded paths

### 5. ✅ I18N
- [ ] Tous les textes sont traduits (pas de texte en dur)
- [ ] Traductions en anglais ET français

### 6. ✅ Qualité de Code
- [ ] Pas de code dupliqué (DRY)
- [ ] Nommage clair et cohérent
- [ ] Pas de console.log oubliés

### 7. ✅ Documentation
- [ ] Project-Status.md mis à jour
- [ ] Tâche marquée comme complétée [x]
- [ ] Date de complétion ajoutée

### 8. ✅ Tests Manuels
- [ ] Application compile et démarre sans erreur
- [ ] Feature fonctionne comme attendu
- [ ] Pas de régression

**Commandes de vérification :**
```bash
# Backend
cd src/backend
dotnet test
dotnet build

# Frontend
cd src/frontend
npm run test
npm run build
npm run lint
```

---

## 📊 Standards de Qualité de Code

### Backend (C#)
- **Naming:** PascalCase (classes, méthodes), camelCase (variables)
- **SOLID:** Respecter les 5 principes
- **Async/Await:** Pas de .Result ou .Wait()
- **XML Documentation:** Pour méthodes publiques

### Frontend (React/TypeScript)
- **Naming:** PascalCase (composants), camelCase (fonctions)
- **Components:** "Dumb" components (présentation uniquement)
- **Hooks:** Logique métier dans custom hooks
- **State:** Zustand pour état global, useState pour local
- **Performance:** useMemo, useCallback si nécessaire
- **Accessibility:** Labels, alt text, keyboard navigation

---

## 🎯 Checklist pour l'Agent

Avant de commencer, l'agent doit confirmer :

1. ✅ J'ai lu le README.md et compris l'organisation
2. ✅ J'ai lu le Project-Status.md et vérifié l'état actuel
3. ✅ J'ai lu la tâche dans Implementation-Roadmap.md
4. ✅ J'ai lu les spécifications fonctionnelles
5. ✅ J'ai compris les contraintes techniques (JSON uniquement)
6. ✅ J'ai vérifié que les dépendances sont complétées
7. ✅ Je sais quoi mettre à jour dans Project-Status.md

---

## 📝 Format de Mise à Jour du Project-Status.md

```markdown
### Phase X: [Phase Name]

#### [Feature Name]
- [x] **Task X.Y: [Task Name]**: [Description]
  - [Détails de la tâche]
  - **Status:** Completed ✅
  - **Completed on:** YYYY-MM-DD
  - **Roadmap Task:** X.Y
```

---

## 🚀 Ordre Recommandé d'Implémentation

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
- ✅ Applique la méthodologie TDD
- ✅ Crée des données mock réalistes
- ✅ Vérifie la qualité du code
