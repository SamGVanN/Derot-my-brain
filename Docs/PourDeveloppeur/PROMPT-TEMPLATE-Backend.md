# Prompt Template - Backend Only

## 🎯 Template

```
Je veux implémenter le BACKEND de la tâche [TASK_NUMBER] du projet "Derot My Brain".

DOCUMENTATION À LIRE :
- ANTIGRAVITY_INSTRUCTIONS.md (workflow général)
- Docs/Implementation-Roadmap.md section [TASK_NUMBER]
- Docs/Specifications-fonctionnelles.md (section correspondante)
- Docs/TECHNICAL-CONSTRAINTS-Storage.md (JSON UNIQUEMENT)

SCOPE BACKEND :
- Services et logique métier
- Controllers et endpoints API
- Modèles et DTOs
- Gestion du stockage JSON
- Tests unitaires backend

⚠️ CONTRAINTES :
- Stockage JSON uniquement (pas de SQL Server/PostgreSQL)
- Respecter l'architecture existante
- Suivre les conventions de nommage du projet

WORKFLOW :
1. Lire la documentation de la tâche
2. Vérifier les dépendances
3. Implémenter les services et controllers
4. Créer/mettre à jour les fichiers JSON
5. Tester les endpoints
6. Mettre à jour Project-Status.md

NE PAS :
- Implémenter le frontend
- Modifier les autres tâches dans Project-Status.md
- Utiliser une base de données SQL

Peux-tu lire la documentation et confirmer ta compréhension avant de commencer ?
```

---

## 📋 Exemple : Task 3.1 (Session Management Backend)

```
Je veux implémenter le BACKEND de la tâche 3.1 (Session Management) du projet "Derot My Brain".

DOCUMENTATION À LIRE :
- ANTIGRAVITY_INSTRUCTIONS.md
- Docs/Implementation-Roadmap.md section 3.1
- Docs/Specifications-fonctionnelles.md section "1.4.5 Gestion des sessions"
- Docs/TECHNICAL-CONSTRAINTS-Storage.md

SCOPE BACKEND :
- SessionService pour créer/gérer les sessions
- SessionController avec endpoints CRUD
- Modèles Session, Question, Answer
- Stockage dans /data/users/{userId}/sessions/
- Tests unitaires pour SessionService

⚠️ CONTRAINTES :
- Fichiers JSON : session-{sessionId}.json
- Génération de questions via LLM (config dans app-config.json)
- Calcul des scores selon les règles métier

WORKFLOW :
1. Lire la documentation de la tâche
2. Vérifier que Task 0.1 et 2.1 sont complétées
3. Créer SessionService et SessionController
4. Implémenter la génération de questions
5. Implémenter le calcul des scores
6. Tester les endpoints API
7. Mettre à jour Project-Status.md

Peux-tu lire la documentation et confirmer ta compréhension avant de commencer ?
```
