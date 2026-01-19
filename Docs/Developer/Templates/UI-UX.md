# Prompt Template - UI/UX Improvements

## 🎯 Template

```
Je veux améliorer l'UI/UX de [COMPONENT_OR_PAGE] du projet "Derot My Brain".

DOCUMENTATION À LIRE :
- Docs/README.md (organisation)
- Docs/Planning/Implementation-Roadmap.md (tâche spécifique)
- Docs/Technical/Frontend-Architecture.md (standards UI)
- Docs/Reference/Color-Palettes.md (thèmes disponibles)

AMÉLIORATION DEMANDÉE :
[DESCRIPTION_DÉTAILLÉE]

SCOPE UI/UX :
- Design visuel et cohérence
- Expérience utilisateur
- Responsive design
- Accessibilité
- Animations et transitions

⚠️ CONTRAINTES :
- Maintenir la cohérence avec le design existant
- Responsive (mobile + desktop)
- Tous les textes via i18n
- Pas de régression fonctionnelle

WORKFLOW :
1. Analyser le composant/page actuel
2. Identifier les points d'amélioration
3. Proposer les modifications
4. Implémenter les changements
5. Tester sur mobile et desktop
6. Vérifier l'accessibilité

NE PAS :
- Modifier la logique métier
- Changer les endpoints API
- Casser les fonctionnalités existantes

Peux-tu analyser l'état actuel et proposer tes améliorations ?
```

---

## 📋 Exemple : Améliorer la Page Préférences

```
Je veux améliorer l'UI/UX de la page UserPreferencesPage du projet "Derot My Brain".

DOCUMENTATION À LIRE :
- Docs/Planning/Specifications-fonctionnelles.md section "1.4.2 Préférences utilisateur"
- Docs/Technical/Frontend-Architecture.md

AMÉLIORATION DEMANDÉE :
- Améliorer la hiérarchie visuelle des sections
- Ajouter des icônes pour chaque section
- Améliorer le feedback visuel lors des changements
- Rendre la page plus intuitive sur mobile

SCOPE UI/UX :
- Layout et organisation des sections
- Icônes et éléments visuels
- Animations de transition
- Feedback utilisateur (toasts, indicateurs)
- Responsive design

⚠️ CONTRAINTES :
- Garder toutes les fonctionnalités existantes
- Cohérence avec le reste de l'application
- Tous les textes via i18n
- Tester sur mobile et desktop

WORKFLOW :
1. Analyser UserPreferencesPage.tsx actuel
2. Identifier les points d'amélioration UX
3. Proposer un design amélioré
4. Implémenter les changements CSS/composants
5. Tester sur différentes tailles d'écran
6. Vérifier que tout fonctionne comme avant

Peux-tu analyser l'état actuel et proposer tes améliorations ?
```
