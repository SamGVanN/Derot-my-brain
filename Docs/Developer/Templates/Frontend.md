# Prompt Template - Frontend Only

## 🎯 Template

```
Je veux implémenter le FRONTEND de la tâche [TASK_NUMBER] du projet "Derot My Brain".

DOCUMENTATION À LIRE :
- ANTIGRAVITY_INSTRUCTIONS.md (workflow général)
- Docs/README.md (organisation)
- Docs/Planning/Project-Status.md (état actuel)
- Docs/Planning/Implementation-Roadmap.md section [TASK_NUMBER]
- Docs/Technical/Frontend-Architecture.md (architecture et best practices)
- Docs/Technical/Testing-Strategy.md (méthodologie TDD)

SCOPE FRONTEND :
- Composants React
- Custom hooks
- State management (Zustand)
- Intégration API
- Traductions i18n
- Tests unitaires/composants

⚠️ CONTRAINTES :
- Suivre frontend_guidelines.md (architecture en couches)
- Utiliser les hooks personnalisés (pas d'appels API directs)
- Tous les textes via i18n (pas de texte en dur)
- Responsive design (mobile + desktop)

WORKFLOW :
1. Lire la documentation de la tâche
2. Vérifier les dépendances (backend ready)
3. Créer les composants UI
4. Implémenter les hooks si nécessaire
5. Ajouter les traductions (en.json + fr.json)
6. Tester les composants
7. Mettre à jour Project-Status.md

NE PAS :
- Modifier le backend
- Appeler l'API directement depuis les composants
- Hardcoder des textes
- Modifier les autres tâches dans Project-Status.md

Peux-tu lire la documentation et confirmer ta compréhension avant de commencer ?
```

---

## 📋 Exemple : Task 3.1 (Session Management Frontend)

```
Je veux implémenter le FRONTEND de la tâche 3.1 (Session Management) du projet "Derot My Brain".

DOCUMENTATION À LIRE :
- ANTIGRAVITY_INSTRUCTIONS.md
- Docs/Planning/Implementation-Roadmap.md section 3.1
- Docs/Planning/Specifications-fonctionnelles.md section "1.4.5 Gestion des sessions"
- Docs/Technical/Frontend-Architecture.md

SCOPE FRONTEND :
- Page SessionPage.tsx (affichage questions/réponses)
- Composant QuestionCard.tsx
- Hook useSession pour gérer l'état
- Traductions pour tous les textes
- Tests des composants

⚠️ CONTRAINTES :
- Utiliser useSession hook (pas d'appels API directs)
- Timer visuel pour chaque question
- Affichage du score en temps réel
- Responsive (mobile + desktop)
- i18n pour tous les textes

WORKFLOW :
1. Lire la documentation de la tâche
2. Vérifier que le backend (Task 3.1) est complété
3. Créer SessionPage et QuestionCard
4. Implémenter useSession hook
5. Ajouter traductions (en.json + fr.json)
6. Tester les composants
7. Mettre à jour Project-Status.md

Peux-tu lire la documentation et confirmer ta compréhension avant de commencer ?
```
