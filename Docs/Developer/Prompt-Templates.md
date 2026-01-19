# Index des Prompt Templates

Ce dossier contient des templates de prompts spécialisés pour différents types de tâches de développement.

## 📚 Templates Disponibles

### 🔧 [Implementation.md](./Templates/Implementation.md)
**Usage :** Implémentation complète d'une tâche (backend + frontend)
- Template de base le plus complet
- Inclut tous les aspects d'une tâche
- Exemples concrets pour différentes phases
- **Utiliser quand :** Vous implémentez une tâche complète du roadmap

---

### ⚙️ [Backend.md](./Templates/Backend.md)
**Usage :** Implémentation backend uniquement
- Focus sur API, services, et stockage JSON
- Controllers et endpoints
- Logique métier
- **Utiliser quand :** Vous voulez implémenter seulement la partie serveur

---

### 🎨 [Frontend.md](./Templates/Frontend.md)
**Usage :** Implémentation frontend uniquement
- Focus sur composants React et UI
- Hooks personnalisés
- State management (Zustand)
- i18n et traductions
- **Utiliser quand :** Le backend est prêt et vous voulez créer l'interface

---

### 🎭 [UI-UX.md](./Templates/UI-UX.md)
**Usage :** Améliorations visuelles et UX
- Design et cohérence visuelle
- Responsive design
- Animations et transitions
- Accessibilité
- **Utiliser quand :** Vous voulez améliorer l'apparence sans toucher à la logique

---

### 🔄 [Migration.md](./Templates/Migration.md)
**Usage :** Migrations et refactoring majeurs
- Changements d'architecture
- Migration de technologies
- Refactoring structurel
- **Utiliser quand :** Vous devez migrer du code existant (ex: Context → Zustand)

---

### 🐛 [QuickFix.md](./Templates/QuickFix.md)
**Usage :** Corrections de bugs rapides
- Fix minimal et ciblé
- Pas de refactoring
- Tests de non-régression
- **Utiliser quand :** Vous avez un bug spécifique à corriger rapidement

---

## 🎯 Guide de Sélection

```
┌─────────────────────────────────────────────────────────┐
│ Quelle est votre situation ?                           │
└─────────────────────────────────────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
    Nouvelle          Bug Fix         Amélioration
     Tâche                              Existante
        │                 │                 │
        │                 │                 │
        ▼                 ▼                 ▼
  ┌──────────┐      ┌──────────┐      ┌──────────┐
  │ Full ou  │      │ QuickFix │      │ UI/UX ou │
  │ Partiel? │      └──────────┘      │ Migration│
  └──────────┘                        └──────────┘
        │                                   │
   ┌────┴────┐                         ┌───┴───┐
   │         │                         │       │
Full      Partiel                   Visual  Structural
   │         │                         │       │
   ▼         ▼                         ▼       ▼
Implementation  Backend             UI/UX   Migration
              ou Frontend
```

## 💡 Conseils d'Utilisation

### Pour une Nouvelle Feature
1. **Commencer par Implementation** (template de base)
2. Si la tâche est grande, **séparer en Backend puis Frontend**
3. Terminer par **UI/UX** pour le polish

### Pour du Refactoring
1. Utiliser **Migration** pour les changements structurels
2. Utiliser **QuickFix** pour les petites corrections

### Pour l'Amélioration Continue
1. **UI/UX** pour les améliorations visuelles
2. **Migration** pour moderniser le code
3. **QuickFix** pour les bugs découverts

## 📝 Structure Commune

Tous les templates suivent cette structure :
```
1. Documentation à lire (pointeurs, pas de répétition)
2. Scope précis (ce qui est inclus)
3. Contraintes critiques
4. Workflow étape par étape
5. Ce qu'il NE FAUT PAS faire
6. Question de confirmation
```

## ⚠️ Principes Importants

- **Concis :** Les templates pointent vers la documentation, ne la répètent pas
- **Focalisés :** Chaque template a un objectif clair et limité
- **Sécurisés :** Tous incluent des garde-fous (contraintes, NE PAS faire)
- **Vérifiables :** Tous demandent confirmation avant de commencer

---

**Dernière mise à jour :** 2026-01-19
