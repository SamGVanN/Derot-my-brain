# Prompt Template - Migration & Refactoring

## 🎯 Template

```
Je veux effectuer la migration/refactoring [MIGRATION_NAME] du projet "Derot My Brain".

DOCUMENTATION À LIRE :
- Docs/Implementation-Roadmap.md (tâche correspondante)
- Docs/frontend_guidelines.md (si frontend)
- Docs/TECHNICAL-CONSTRAINTS-Storage.md (si backend)

MIGRATION À EFFECTUER :
[DESCRIPTION_DÉTAILLÉE]

SCOPE :
- Code à migrer/refactorer
- Tests à adapter
- Documentation à mettre à jour

⚠️ CONTRAINTES CRITIQUES :
- ZÉRO régression fonctionnelle
- Tous les tests existants doivent passer
- Migration incrémentale si possible
- Backup/rollback plan

WORKFLOW :
1. Analyser le code existant
2. Créer un plan de migration détaillé
3. Migrer par petits incréments testables
4. Vérifier les tests après chaque incrément
5. Mettre à jour la documentation
6. Validation finale complète

NE PAS :
- Changer le comportement fonctionnel
- Casser les tests existants
- Modifier plusieurs systèmes en parallèle

Peux-tu analyser le code actuel et proposer un plan de migration ?
```

---

## 📋 Exemple : Migration vers Zustand

```
Je veux effectuer la migration vers Zustand (Task -1.2) du projet "Derot My Brain".

DOCUMENTATION À LIRE :
- Docs/Implementation-Roadmap.md section -1.2
- Docs/frontend_guidelines.md section "State Management"

MIGRATION À EFFECTUER :
- Remplacer React Context par Zustand
- Migrer authStore, userStore, preferencesStore
- Adapter tous les composants utilisant les contexts
- Maintenir la compatibilité avec les hooks existants

SCOPE :
- Création des stores Zustand
- Migration des composants
- Adaptation des tests
- Suppression des anciens contexts

⚠️ CONTRAINTES CRITIQUES :
- ZÉRO régression : l'app doit fonctionner exactement pareil
- Migrer store par store (auth → user → preferences)
- Tous les tests doivent passer après chaque store
- Vérifier le dev server après chaque étape

WORKFLOW :
1. Analyser les contexts existants
2. Créer le plan de migration (ordre des stores)
3. Migrer authStore en premier
4. Tester complètement
5. Migrer userStore
6. Tester complètement
7. Migrer preferencesStore
8. Tester complètement
9. Supprimer les anciens contexts
10. Mettre à jour Project-Status.md

Peux-tu analyser les contexts actuels et proposer un plan de migration détaillé ?
```
