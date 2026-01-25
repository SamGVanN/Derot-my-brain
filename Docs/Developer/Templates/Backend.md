# Prompt Template - Backend Only

## 🎯 Template

```
Je veux implémenter le BACKEND de la tâche [TASK_NUMBER] du projet "Derot My Brain".

DOCUMENTATION À LIRE :
- ANTIGRAVITY_INSTRUCTIONS.md (workflow général)
- Docs/README.md (organisation)
- Docs/Planning/Project-Status.md (état actuel)
- Docs/Planning/Implementation-Roadmap.md (tâche spécifique)
- Docs/Technical/Storage-Policy.md (SQLite)
- Docs/Technical/Backend-Guidelines.md (architecture backend)
- Docs/Technical/Testing-Strategy.md (méthodologie TDD)

SCOPE BACKEND :
- Services et logique métier
- Controllers et endpoints API
- Modèles et DTOs
- Gestion du stockage SQLite (EF Core)
- Tests unitaires backend

⚠️ CONTRAINTES :
- Respecter l'architecture existante
- Suivre les conventions de nommage du projet

WORKFLOW :
1. Lire la documentation de la tâche
2. Vérifier les dépendances
3. Implémenter en TDD avec WebApplicationFactory
4. Tester les endpoints
5. Mettre à jour Project-Status.md

NE PAS :
- Implémenter le frontend
- Modifier les autres tâches dans Project-Status.md

Peux-tu lire la documentation et confirmer ta compréhension avant de commencer ?
```

---

## 📋 Exemple : Task 3.1 (Session Management Backend)

```
Je veux implémenter le BACKEND de la tâche 3.1 (Session Management) du projet "Derot My Brain".

DOCUMENTATION À LIRE :
- ANTIGRAVITY_INSTRUCTIONS.md
- Docs/Planning/Implementation-Roadmap.md section 3.1
- Docs/Planning/functional_specifications_derot_my_brain.md section "1.4.5 Gestion des sessions"
- Docs/Technical/Storage-Policy.md

SCOPE BACKEND :
- SessionService pour créer/gérer les sessions
- SessionController avec endpoints CRUD
- Modèles Session, Question, Answer
- Stockage SQLite
- Tests unitaires pour SessionService

⚠️ CONTRAINTES :
- Stockage SQLite via Entity Framework Core
- Génération de questions via LLM
- Calcul des scores selon les règles métier

WORKFLOW :
1. Lire la documentation de la tâche
2. Vérifier que Task 0.1 et 2.1 sont complétées
3. Créer SessionService et SessionController en TDD
4. Implémenter la génération de questions
5. Implémenter le calcul des scores
6. Tester les endpoints API avec WebApplicationFactory
7. Mettre à jour Project-Status.md

Peux-tu lire la documentation et confirmer ta compréhension avant de commencer ?
```
