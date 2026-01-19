# Prompt Template - Quick Fix

## 🎯 Template

```
Je veux corriger le bug suivant dans le projet "Derot My Brain" :

BUG DESCRIPTION :
[DESCRIPTION_DU_BUG]

COMPORTEMENT ATTENDU :
[COMPORTEMENT_CORRECT]

COMPORTEMENT ACTUEL :
[COMPORTEMENT_INCORRECT]

CONTEXTE :
- Fichier(s) concerné(s) : [FICHIERS]
- Composant/Service : [NOM]

⚠️ CONTRAINTES :
- Fix minimal (pas de refactoring)
- Pas de régression
- Tester le fix

WORKFLOW :
1. Reproduire le bug
2. Identifier la cause
3. Proposer le fix minimal
4. Implémenter
5. Tester le fix
6. Vérifier qu'il n'y a pas de régression

NE PAS :
- Refactorer du code non lié
- Modifier d'autres fonctionnalités
- Changer l'architecture

Peux-tu analyser et corriger ce bug ?
```

---

## 📋 Exemple : Fix Indicateur de Langue

```
Je veux corriger le bug suivant dans le projet "Derot My Brain" :

BUG DESCRIPTION :
L'indicateur de langue ne s'affiche pas dans GeneralPreferencesForm quand la langue active diffère de la préférence sauvegardée.

COMPORTEMENT ATTENDU :
- Si langue active ≠ langue préférée → afficher "Currently using: [langue]"
- Si langue active = langue préférée → pas d'indicateur

COMPORTEMENT ACTUEL :
- L'indicateur ne s'affiche jamais

CONTEXTE :
- Fichier : src/frontend/src/components/preferences/GeneralPreferencesForm.tsx
- Composant : GeneralPreferencesForm
- Ligne approximative : ~50-70 (zone de l'indicateur de langue)

⚠️ CONTRAINTES :
- Fix minimal (juste l'indicateur)
- Garder le même style que l'indicateur de thème
- Utiliser i18n pour le texte
- Tester avec différentes combinaisons langue active/préférée

WORKFLOW :
1. Vérifier le code de l'indicateur de langue
2. Comparer avec l'indicateur de thème (qui fonctionne)
3. Identifier pourquoi la condition ne fonctionne pas
4. Corriger la logique
5. Tester les cas : FR→EN, EN→FR, FR→FR, EN→EN
6. Vérifier que l'indicateur de thème fonctionne toujours

Peux-tu analyser et corriger ce bug ?
```

---

## 📋 Exemple : Fix Crash au Démarrage

```
Je veux corriger le bug suivant dans le projet "Derot My Brain" :

BUG DESCRIPTION :
L'application crash au démarrage avec une erreur "Cannot read property 'theme' of undefined"

COMPORTEMENT ATTENDU :
- L'application démarre sans erreur
- Le thème par défaut est appliqué si aucune préférence n'existe

COMPORTEMENT ACTUEL :
- Crash avec erreur dans la console
- Page blanche

CONTEXTE :
- Fichier : src/frontend/src/App.tsx
- Erreur dans useEffect d'initialisation
- Ligne approximative : ~30-40

⚠️ CONTRAINTES :
- Fix minimal
- Gérer le cas où les préférences n'existent pas encore
- Tester avec et sans préférences sauvegardées

WORKFLOW :
1. Reproduire le crash
2. Analyser la stack trace
3. Identifier la variable undefined
4. Ajouter la vérification manquante
5. Tester le fix (nouveau user + user existant)
6. Vérifier qu'il n'y a pas de régression

Peux-tu analyser et corriger ce bug ?
```
