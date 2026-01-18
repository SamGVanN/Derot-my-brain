# Analyse de Refonte Frontend

Cette analyse identifie les composants nécessitant une adaptation pour s'aligner sur `docs/frontend_guidelines.md`.

## résumé des Violations

L'application contient plusieurs violations majeures des principes architecturaux définis :
1. **Logique métier dans les composants** (Violations principes #1, #5, #8)
2. **Utilisation directe des Services/API dans l'UI** (Violation principe #8)
3. **Composants monolithiques** (Violation principe #2 - Max 200 lignes)
4. **Utilisation directe des Stores** au lieu de passer par des Hooks d'abstraction (Violation "State Management: Cloud Principles")

## Composants à Refactoriser

### 1. `src/frontend/src/App.tsx`
**État actuel :** 193 lignes. Mélange routing, check de session, et UI.
**Violations :**
- **Logique de Session :** Le `useEffect` pour `validateSession` (Lignes 36-61) contient de la logique métier complexe qui devrait être dans un hook (`useAuth` ou `useSession`).
- **Accès direct au Store :** Importe et utilise `useAuthStore` et `usePreferencesStore` directement. Devrait utiliser `useAuth()` et `usePreferences()`.
- **Import de Service :** Importe `UserService` directement.
- **Type Casting Sale :** `setCurrentUser: updateUser as any` (Ligne 113).

**Actions Requises :**
- Déplacer la logique `validateSession` dans `hooks/useAuth.ts` (méthode `initializeSession` ou similaire).
- Remplacer les imports de stores par les hooks `useAuth` et `usePreferences`.
- Supprimer l'import de `UserService`.

### 2. `src/frontend/src/pages/UserPreferencesPage.tsx`
**État actuel :** 486 lignes. Monolithe.
**Violations :**
- **Taille excessive :** Dépasse largement la limite de 150-200 lignes.
- **Responsabilités multiples :** Gère l'affichage, la logique de formulaire, les appels API, et la gestion d'état locale pour 3 domaines différents (Général, Thème, Catégories).
- **Implémentation UI Manuelle :** Réimplémente des dropdowns (Lignes 201-246) au lieu d'utiliser les composants UI standard (`Select`, `DropdownMenu`).
- **Appels API directs :** Utilise `UserService` et `categoryApi` directement dans `useMutation`/`useQuery`. Cette logique devrait être encapsulée dans `hooks/usePreferences.ts` et `hooks/useCategories.ts`.

**Actions Requises :**
- Découper en sous-composants :
    - `components/preferences/GeneralPreferencesForm.tsx`
    - `components/preferences/CategoryPreferencesForm.tsx`
- Utiliser les composants UI shadcn/ui (`Select`, `Checkbox`, etc.).
- Déplacer les `useQuery`/`useMutation` dans les hooks personnalisés correspondants.

### 3. `src/frontend/src/components/history-view.tsx`
**État actuel :** 83 lignes.
**Violations :**
- **Logique de Fetching :** Contient un `useEffect` qui appelle `UserService.getHistory` directement.
- **Violation "Dumb Component" :** Le composant "sait" comment récupérer ses données.

**Actions Requises :**
- Utiliser le hook `useHistory()` pour récupérer les données.
- Le composant ne doit gérer que l'affichage (`isLoading`, `error`, `activities` venant du hook).

### 4. `src/frontend/src/pages/UserSelectionPage.tsx`
*(Non listé mais supposé nécessiter check)*
- **Violations probables :** Appels directs à `UserService`.
- **Actions :** Utiliser `useUser()` ou `useAuth()` pour lister et sélectionner les utilisateurs.

## Vérification de l'Architecture
- **Dossier `api/`** : Existe, mais doit être utilisé exclusivement par les Hooks/Services, jamais par les Composants.
- **Dossier `hooks/`** : Les hooks existent (`useAuth`, `usePreferences`, etc.) mais doivent être vérifiés pour s'assurer qu'ils exposent bien toute la logique nécessaire aux composants ci-dessus.
