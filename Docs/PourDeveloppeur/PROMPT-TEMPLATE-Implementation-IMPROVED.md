# Prompt Template pour Implémentation de Tâche - Version Améliorée

## 📋 Nouveautés et Améliorations

Cette version améliorée inclut :
- ✅ Exemples pour Phase -1 (Frontend Architecture Migration)
- ✅ Template pour TDD (Test-Driven Development)
- ✅ Template pour création de Mock Data (TestUser)
- ✅ Template pour vérification post-implémentation
- ✅ Template pour debugging/troubleshooting
- ✅ Checklist de qualité de code

---

## 🎯 Template de Base (Inchangé)

```
Je veux implémenter la tâche [TASK_NUMBER] du projet "Derot My Brain". Commence par prendre connaissance de ANTIGRAVITY_INSTRUCTIONS.md.

CONTEXTE DU PROJET :
- Lire Docs/README.md pour comprendre l'organisation de la documentation
- Lire Docs/Project-Status.md pour voir l'état actuel du projet
- Lire Docs/TECHNICAL-CONSTRAINTS-Storage.md pour les contraintes de stockage (JSON UNIQUEMENT)
- Lire Docs/frontend_guidelines.md pour les principes d'architecture frontend (si tâche frontend)

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
8. Mettre à jour Docs/Project-Status.md :
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

## 🆕 Nouveaux Exemples

### Exemple 4 : Implémenter Task -1.1 (Infrastructure Layer Setup)

```
Je veux implémenter la tâche -1.1 (Infrastructure Layer Setup) du projet "Derot My Brain".

CONTEXTE DU PROJET :
- Lire Docs/README.md pour comprendre l'organisation de la documentation
- Lire Docs/Project-Status.md pour voir l'état actuel du projet
- Lire Docs/frontend_guidelines.md pour comprendre les principes d'architecture frontend

TÂCHE À IMPLÉMENTER :
- Lire la section "Task -1.1: Infrastructure Layer Setup" dans Docs/Implementation-Roadmap.md
- Suivre EXACTEMENT les spécifications, l'objectif et les critères d'acceptation
- Cette tâche n'a AUCUNE dépendance - c'est la première de Phase -1

SPÉCIFICATIONS FONCTIONNELLES :
- Cette tâche est technique, pas de section spécifique dans Specifications-fonctionnelles.md
- Se référer aux principes d'architecture dans frontend_guidelines.md

OBJECTIF PRINCIPAL :
⚠️ CRITIQUE : Établir une Infrastructure Layer propre avec un client HTTP centralisé
- Créer /api directory structure (client.ts, endpoints.ts, userApi.ts, categoryApi.ts)
- Implémenter un client axios centralisé avec base URL depuis l'environnement
- Migrer UserService.ts → userApi.ts
- Éliminer le mélange axios/fetch

WORKFLOW D'IMPLÉMENTATION :
1. Lire et comprendre frontend_guidelines.md (section Infrastructure Layer)
2. Analyser le code existant (UserService.ts, composants utilisant axios/fetch)
3. Créer la structure /src/api/ avec les fichiers nécessaires
4. Implémenter le client axios centralisé (client.ts)
5. Créer endpoints.ts pour centraliser les URLs
6. Migrer UserService.ts → userApi.ts
7. Mettre à jour tous les composants pour utiliser le nouveau client
8. Tester que toutes les requêtes API fonctionnent
9. Mettre à jour Docs/Project-Status.md :
   - Marquer "Task -1.1" comme complétée [x]
   - Mettre à jour le statut à "Completed"
   - Ajouter la date de complétion (2026-01-18)

POINTS CLÉS :
- Aucun hardcoded API URL dans les composants
- Un seul client HTTP centralisé
- Gestion d'erreur centralisée
- Configuration depuis variables d'environnement

Peux-tu commencer par lire la documentation et me confirmer que tu as bien compris la tâche avant de commencer l'implémentation ?
```

---

### Exemple 5 : Implémenter Task -1.3 (Custom Hooks Implementation)

```
Je veux implémenter la tâche -1.3 (Custom Hooks Implementation) du projet "Derot My Brain".

CONTEXTE DU PROJET :
- Lire Docs/README.md pour comprendre l'organisation de la documentation
- Lire Docs/Project-Status.md pour voir l'état actuel du projet
- Lire Docs/frontend_guidelines.md pour comprendre les principes d'architecture frontend

TÂCHE À IMPLÉMENTER :
- Lire la section "Task -1.3: Custom Hooks Implementation" dans Docs/Implementation-Roadmap.md
- Suivre EXACTEMENT les spécifications, l'objectif et les critères d'acceptation
- Dépendances : Task -1.1 (Infrastructure) et Task -1.2 (Zustand) doivent être complétées

SPÉCIFICATIONS FONCTIONNELLES :
- Cette tâche est technique, pas de section spécifique dans Specifications-fonctionnelles.md
- Se référer aux principes "Custom Hooks" et "Separation of Concerns" dans frontend_guidelines.md

OBJECTIF PRINCIPAL :
⚠️ CRITIQUE : Extraire TOUTE la logique métier des composants vers des custom hooks
- Créer hooks/useAuth.ts (wraps useAuthStore)
- Créer hooks/useUser.ts (opérations utilisateur)
- Créer hooks/usePreferences.ts (wraps usePreferencesStore)
- Créer hooks/useHistory.ts (opérations historique)
- Améliorer hooks/useCategories.ts (utiliser le client API centralisé)

WORKFLOW D'IMPLÉMENTATION :
1. Vérifier que Task -1.1 et -1.2 sont complétées
2. Lire frontend_guidelines.md (section Custom Hooks)
3. Analyser les composants existants pour identifier la logique métier
4. Créer /src/hooks/ si nécessaire
5. Implémenter chaque hook selon les spécifications
6. S'assurer que les hooks respectent le Single Responsibility Principle
7. Mettre à jour les composants pour utiliser les hooks (sera fait en Task -1.4)
8. Tester chaque hook individuellement
9. Mettre à jour Docs/Project-Status.md

POINTS CLÉS :
- Les hooks encapsulent la logique métier
- Les composants deviennent "dumb" (présentation uniquement)
- Pas d'appels API directs dans les composants
- Les hooks peuvent composer d'autres hooks

EXEMPLE DE STRUCTURE :
```typescript
// hooks/useAuth.ts
export const useAuth = () => {
  const store = useAuthStore();
  
  const login = async (userId: string) => {
    // Logique métier ici
  };
  
  const logout = () => {
    // Logique métier ici
  };
  
  return { login, logout, user: store.user, isAuthenticated: store.isAuthenticated };
};
```

Peux-tu commencer par lire la documentation et me confirmer que tu as bien compris la tâche avant de commencer l'implémentation ?
```

---

### Exemple 6 : Implémenter Task 3.1 (Main Navigation Menu)

```
Je veux implémenter la tâche 3.1 (Main Navigation Menu) du projet "Derot My Brain".

CONTEXTE DU PROJET :
- Lire Docs/README.md pour comprendre l'organisation de la documentation
- Lire Docs/Project-Status.md pour voir l'état actuel du projet
- Lire Docs/frontend_guidelines.md pour les principes d'architecture frontend
- Lire Docs/TECHNICAL-CONSTRAINTS-Storage.md pour les contraintes de stockage

TÂCHE À IMPLÉMENTER :
- Lire la section "Task 3.1: Main Navigation Menu" dans Docs/Implementation-Roadmap.md
- Suivre EXACTEMENT les spécifications, l'objectif et les critères d'acceptation
- Dépendances : Phase -1 (Architecture Migration) et Phase 2 (i18n) doivent être complétées

SPÉCIFICATIONS FONCTIONNELLES :
- Lire la section "1.4.4 Navigation et structure des pages" dans Docs/Specifications-fonctionnelles.md
- Comprendre les besoins métier avant de coder

OBJECTIF PRINCIPAL :
Créer un menu de navigation principal avec les liens suivants :
- Derot (page principale)
- My Brain (dropdown: History + Tracked Topics)
- Profile
- Preferences
- Guide
- Logout

MÉTHODOLOGIE TDD :
1. ✅ Écrire les tests pour le composant Navigation
2. ✅ Tester l'affichage des liens
3. ✅ Tester le dropdown "My Brain"
4. ✅ Tester la traduction des labels (i18n)
5. ✅ Tester le comportement responsive (mobile/desktop)

WORKFLOW D'IMPLÉMENTATION :
1. Vérifier que Phase -1 et Task 2.5 (i18n) sont complétées
2. Lire les spécifications fonctionnelles
3. Écrire les tests (TDD) pour le composant Navigation
4. Créer components/Navigation.tsx (ou Header.tsx)
5. Utiliser shadcn/ui components (NavigationMenu, DropdownMenu)
6. Implémenter les traductions (nav.derot, nav.myBrain, etc.)
7. Implémenter le comportement responsive
8. Tester sur mobile et desktop
9. Mettre à jour Docs/Project-Status.md

POINTS CLÉS :
- Utiliser react-i18next pour toutes les traductions
- Utiliser shadcn/ui components (pas de HTML brut)
- Respecter le système de thème existant
- Composant doit être "dumb" (pas de logique métier)
- Utiliser useAuth() hook pour l'état d'authentification

TERMINOLOGIE MISE À JOUR (2026-01-18) :
- "Backlog" → "Tracked Topics" (Sujets Suivis)
- "My Brain" regroupe History + Tracked Topics

Peux-tu commencer par lire la documentation et me confirmer que tu as bien compris la tâche avant de commencer l'implémentation ?
```

---

## 🧪 Template TDD (Test-Driven Development)

```
Je veux implémenter la tâche [TASK_NUMBER] en suivant strictement la méthodologie TDD.

CONTEXTE TDD :
- Lire Docs/Implementation-Roadmap.md section "Development Methodology Requirements"
- Comprendre le cycle Red → Green → Refactor
- Objectif : 80% de couverture de code minimum

WORKFLOW TDD STRICT :
1. 🔴 RED PHASE - Écrire les tests (qui échouent)
   - Définir les cas de test (nominal, edge cases, erreurs)
   - Écrire les tests unitaires (backend) ou component tests (frontend)
   - Vérifier que les tests échouent (pas de code d'implémentation encore)

2. 🟢 GREEN PHASE - Implémenter le code minimal
   - Écrire le code minimal pour faire passer les tests
   - Ne pas optimiser, juste faire fonctionner
   - Vérifier que tous les tests passent

3. 🔵 REFACTOR PHASE - Améliorer le code
   - Nettoyer le code (DRY, SOLID)
   - Améliorer la lisibilité
   - Optimiser si nécessaire
   - Vérifier que les tests passent toujours

4. 📊 COVERAGE PHASE - Vérifier la couverture
   - Exécuter l'outil de couverture de code
   - Vérifier que la couverture ≥ 80%
   - Ajouter des tests si nécessaire

EXEMPLE DE WORKFLOW POUR BACKEND :
```csharp
// 1. RED PHASE - Tests/UserServiceTests.cs
[Fact]
public async Task GetUserById_ShouldReturnUser_WhenUserExists()
{
    // Arrange
    var userId = "test-user-id";
    // ... setup mock

    // Act
    var result = await _userService.GetUserById(userId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(userId, result.Id);
}

// 2. GREEN PHASE - Services/UserService.cs
public async Task<User> GetUserById(string userId)
{
    // Code minimal pour faire passer le test
    return await _repository.GetByIdAsync(userId);
}

// 3. REFACTOR PHASE - Améliorer le code
public async Task<User> GetUserById(string userId)
{
    if (string.IsNullOrEmpty(userId))
        throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
    
    var user = await _repository.GetByIdAsync(userId);
    
    if (user == null)
        throw new NotFoundException($"User with ID {userId} not found");
    
    return user;
}
```

EXEMPLE DE WORKFLOW POUR FRONTEND :
```typescript
// 1. RED PHASE - __tests__/useAuth.test.ts
describe('useAuth', () => {
  it('should login user successfully', async () => {
    // Arrange
    const { result } = renderHook(() => useAuth());
    
    // Act
    await act(() => result.current.login('test-user-id'));
    
    // Assert
    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.user?.id).toBe('test-user-id');
  });
});

// 2. GREEN PHASE - hooks/useAuth.ts
export const useAuth = () => {
  const login = async (userId: string) => {
    // Code minimal
    const user = await userApi.getUserById(userId);
    useAuthStore.getState().setUser(user);
  };
  
  return { login, /* ... */ };
};

// 3. REFACTOR PHASE - Améliorer
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

VÉRIFICATION FINALE :
- [ ] Tous les tests passent (backend + frontend)
- [ ] Couverture de code ≥ 80%
- [ ] Pas de tests ignorés/skipped
- [ ] Tests couvrent les edge cases
- [ ] Tests couvrent les cas d'erreur

Peux-tu commencer par écrire les tests AVANT toute implémentation ?
```

---

## 📦 Template Mock Data (TestUser)

```
Je veux créer les données mock pour TestUser pour la tâche [TASK_NUMBER].

CONTEXTE MOCK DATA :
- Lire Docs/Implementation-Roadmap.md section "Mock Data for TestUser"
- Comprendre la structure des données utilisateur
- Objectif : Données réalistes et représentatives

STRUCTURE DES DONNÉES TESTUSER :
Location: /data/users/
- users.json - Profil et préférences de TestUser
- user-{testuser-id}-history.json - Historique d'activité
- user-{testuser-id}-tracked.json - Sujets suivis (ex-Backlog)

WORKFLOW DE CRÉATION :
1. Identifier le TestUser ID existant dans /data/users/users.json
2. Créer des données réalistes pour la nouvelle feature
3. Couvrir les cas nominaux ET les edge cases
4. Documenter la structure des données

EXEMPLE POUR NOUVELLE FEATURE "Quiz Scores" :
```json
// user-test-user-id-001-history.json
{
  "userId": "test-user-id-001",
  "activities": [
    // Cas nominal : Score parfait
    {
      "id": "activity-001",
      "topic": "Quantum Mechanics",
      "category": "natural-sciences",
      "activityType": "Quiz",
      "score": 20,
      "totalQuestions": 20,
      "percentage": 100,
      "date": "2026-01-18T14:30:00Z",
      "llmInfo": {
        "model": "llama3:8b",
        "provider": "ollama"
      }
    },
    // Edge case : Score faible
    {
      "id": "activity-002",
      "topic": "Ancient History",
      "category": "history-events",
      "activityType": "Quiz",
      "score": 3,
      "totalQuestions": 20,
      "percentage": 15,
      "date": "2026-01-17T10:00:00Z"
    },
    // Edge case : Score moyen
    {
      "id": "activity-003",
      "topic": "Modern Art",
      "category": "culture-arts",
      "activityType": "Quiz",
      "score": 12,
      "totalQuestions": 20,
      "percentage": 60,
      "date": "2026-01-16T16:45:00Z"
    },
    // Edge case : Activité "Read" sans quiz
    {
      "id": "activity-004",
      "topic": "Philosophy",
      "category": "philosophy-thinking",
      "activityType": "Read",
      "date": "2026-01-15T09:20:00Z"
    }
  ]
}
```

CRITÈRES DE QUALITÉ :
- [ ] Données réalistes et cohérentes
- [ ] Couvre les cas nominaux
- [ ] Couvre les edge cases (vide, max, min)
- [ ] Timestamps valides et cohérents
- [ ] Références valides (catégories, thèmes, etc.)
- [ ] Documentation de la structure

EDGE CASES À COUVRIR :
- Données vides (nouveau utilisateur)
- Données au maximum (utilisateur très actif)
- Données avec erreurs (pour tester la robustesse)
- Données avec valeurs limites (0, 100%, etc.)

Peux-tu créer des données mock complètes et réalistes pour TestUser ?
```

---

## 🔍 Template Vérification Post-Implémentation

```
Je viens de terminer l'implémentation de la tâche [TASK_NUMBER]. Peux-tu effectuer une vérification complète ?

CHECKLIST DE VÉRIFICATION :

1. ✅ CRITÈRES D'ACCEPTATION
   - [ ] Tous les critères d'acceptation de Implementation-Roadmap.md sont remplis
   - [ ] Aucun critère n'a été ignoré ou partiellement implémenté

2. ✅ TESTS
   - [ ] Tous les tests unitaires passent (backend)
   - [ ] Tous les tests de composants passent (frontend)
   - [ ] Couverture de code ≥ 80%
   - [ ] Pas de tests ignorés/skipped
   - [ ] Tests couvrent les edge cases

3. ✅ MOCK DATA
   - [ ] Données TestUser créées et documentées
   - [ ] Données réalistes et représentatives
   - [ ] Edge cases couverts

4. ✅ ARCHITECTURE (Frontend)
   - [ ] Respect de frontend_guidelines.md
   - [ ] Composants "dumb" (pas de logique métier)
   - [ ] Utilisation de custom hooks
   - [ ] Pas d'appels API directs dans les composants
   - [ ] Utilisation de Zustand pour l'état global

5. ✅ ARCHITECTURE (Backend)
   - [ ] Respect des principes SOLID
   - [ ] Repository pattern utilisé
   - [ ] Service layer bien défini
   - [ ] Gestion d'erreur appropriée

6. ✅ STOCKAGE
   - [ ] Utilisation de JSON uniquement (pas de SQL)
   - [ ] Structure de fichiers respectée (/data/users/, etc.)
   - [ ] Pas de hardcoded paths

7. ✅ I18N
   - [ ] Tous les textes sont traduits (pas de texte en dur)
   - [ ] Clés de traduction cohérentes
   - [ ] Traductions en anglais ET français

8. ✅ QUALITÉ DE CODE
   - [ ] Pas de code dupliqué (DRY)
   - [ ] Nommage clair et cohérent
   - [ ] Commentaires pour les parties complexes
   - [ ] Pas de console.log oubliés
   - [ ] Pas de code commenté inutile

9. ✅ DOCUMENTATION
   - [ ] Project-Status.md mis à jour
   - [ ] Tâche marquée comme complétée [x]
   - [ ] Date de complétion ajoutée
   - [ ] Changements significatifs documentés

10. ✅ TESTS MANUELS
    - [ ] Application compile sans erreur
    - [ ] Application démarre sans erreur
    - [ ] Feature fonctionne comme attendu
    - [ ] Pas de régression sur les features existantes
    - [ ] Testé sur mobile ET desktop (si applicable)

COMMANDES DE VÉRIFICATION :
Backend:
```bash
cd src/backend
dotnet test
dotnet build
```

Frontend:
```bash
cd src/frontend
npm run test
npm run build
npm run lint
```

Si toutes les vérifications passent, la tâche est considérée comme complète ✅
```

---

## 🐛 Template Debugging/Troubleshooting

```
J'ai un problème avec l'implémentation de la tâche [TASK_NUMBER].

DESCRIPTION DU PROBLÈME :
[Décrire le problème en détail]

COMPORTEMENT ATTENDU :
[Ce qui devrait se passer]

COMPORTEMENT ACTUEL :
[Ce qui se passe réellement]

MESSAGES D'ERREUR :
```
[Copier les messages d'erreur complets]
```

CONTEXTE :
- Tâche : [TASK_NUMBER]
- Fichiers modifiés : [Liste des fichiers]
- Dernière modification qui fonctionnait : [Si applicable]

ÉTAPES POUR REPRODUIRE :
1. [Étape 1]
2. [Étape 2]
3. [Étape 3]

DÉJÀ ESSAYÉ :
- [Action 1] → [Résultat]
- [Action 2] → [Résultat]

DOCUMENTATION CONSULTÉE :
- [ ] Implementation-Roadmap.md
- [ ] Specifications-fonctionnelles.md
- [ ] frontend_guidelines.md (si frontend)
- [ ] TECHNICAL-CONSTRAINTS-Storage.md (si backend)

DEMANDE :
Peux-tu m'aider à identifier et résoudre ce problème en suivant une approche systématique ?
```

---

## 📊 Checklist de Qualité de Code

### Backend (C#)

```markdown
- [ ] **Naming Conventions**
  - [ ] PascalCase pour classes, méthodes, propriétés
  - [ ] camelCase pour variables locales et paramètres
  - [ ] Noms descriptifs et significatifs

- [ ] **SOLID Principles**
  - [ ] Single Responsibility Principle
  - [ ] Open/Closed Principle
  - [ ] Liskov Substitution Principle
  - [ ] Interface Segregation Principle
  - [ ] Dependency Inversion Principle

- [ ] **Error Handling**
  - [ ] Try-catch appropriés
  - [ ] Exceptions personnalisées si nécessaire
  - [ ] Messages d'erreur clairs
  - [ ] Logging des erreurs

- [ ] **Async/Await**
  - [ ] Utilisation correcte de async/await
  - [ ] Pas de .Result ou .Wait()
  - [ ] ConfigureAwait(false) si applicable

- [ ] **XML Documentation**
  - [ ] Commentaires XML pour méthodes publiques
  - [ ] Description des paramètres
  - [ ] Description des valeurs de retour
```

### Frontend (React/TypeScript)

```markdown
- [ ] **Naming Conventions**
  - [ ] PascalCase pour composants
  - [ ] camelCase pour fonctions, variables
  - [ ] Noms descriptifs et significatifs

- [ ] **Component Structure**
  - [ ] Composants "dumb" (présentation uniquement)
  - [ ] Logique métier dans custom hooks
  - [ ] Props typées avec TypeScript
  - [ ] Pas de logique complexe dans JSX

- [ ] **Hooks**
  - [ ] Respect des règles des hooks
  - [ ] Custom hooks pour logique réutilisable
  - [ ] Dépendances correctes dans useEffect
  - [ ] Cleanup dans useEffect si nécessaire

- [ ] **State Management**
  - [ ] Zustand pour état global
  - [ ] useState pour état local uniquement
  - [ ] Pas de prop drilling excessif

- [ ] **Performance**
  - [ ] useMemo pour calculs coûteux
  - [ ] useCallback pour fonctions passées en props
  - [ ] Éviter les re-renders inutiles

- [ ] **Accessibility**
  - [ ] Labels pour inputs
  - [ ] Alt text pour images
  - [ ] Keyboard navigation
  - [ ] ARIA attributes si nécessaire
```

---

## ✅ Résumé des Améliorations

Cette version améliorée ajoute :

1. **Exemples Phase -1** : Templates pour les tâches d'architecture frontend
2. **Template TDD** : Guide complet pour Test-Driven Development
3. **Template Mock Data** : Guide pour créer des données TestUser
4. **Template Vérification** : Checklist complète post-implémentation
5. **Template Debugging** : Structure pour demander de l'aide
6. **Checklist Qualité** : Standards de code backend et frontend

Ces templates garantissent une implémentation de haute qualité, conforme aux standards du projet, et facilitent la délégation à des agents développeurs.
