# Technical Constraints - Storage Policy

**Date:** 2026-01-18  
**Version:** Storage Policy V1 - JSON Only

---

## 🎯 Objectif

Clarifier la politique de stockage pour le POC/V1 de "Derot My Brain" et définir les alternatives acceptables si nécessaire.

---

## ⚠️ Règle Fondamentale

### JSON Files ONLY - No SQL Database

**Pour le POC/V1, l'application DOIT utiliser UNIQUEMENT des fichiers JSON.**

---

## ✅ Ce Qui Est Obligatoire

### Stockage JSON Local

```
/data/
├── seed/                          # Données de référence immuables
│   ├── categories.json            # 13 catégories Wikipedia
│   └── themes.json                # 5 thèmes de couleurs
├── config/                        # Configuration globale
│   └── app-config.json            # URL LLM, paramètres globaux
└── users/                         # Données utilisateurs
    ├── users.json                 # Profils et préférences
    ├── user-{id}-history.json     # Historique par utilisateur
    └── user-{id}-backlog.json     # Backlog par utilisateur
```

### Caractéristiques Requises

- ✅ **Portable** : Copier/coller le dossier `/data/` suffit pour migrer
- ✅ **Autonome** : Aucune installation externe requise
- ✅ **Offline-first** : Fonctionne sans connexion internet
- ✅ **Simple** : Pas de configuration utilisateur (connection string, etc.)
- ✅ **Self-contained** : Tout est dans l'application

---

## ❌ Ce Qui Est Interdit

### Bases de Données Nécessitant Installation/Configuration

**SQL Server** ❌
- Raison : Nécessite installation de SQL Server sur la machine utilisateur
- Alternative : Trop complexe pour un POC

**PostgreSQL** ❌
- Raison : Nécessite serveur externe
- Alternative : Pas adapté pour application locale

**MySQL / MariaDB** ❌
- Raison : Nécessite serveur externe
- Alternative : Pas adapté pour application locale

**MongoDB** ❌
- Raison : Nécessite serveur externe (sauf mode embedded)
- Alternative : Complexité non justifiée pour POC

**Bases de Données Cloud** ❌
- Raison : Nécessite connexion internet
- Alternative : Contraire au principe offline-first

---

## ✅ Alternatives Acceptables (Si Nécessaire)

### Quand Envisager une Alternative ?

Un agent IA peut proposer une alternative **UNIQUEMENT** si :
1. Les fichiers JSON deviennent trop volumineux (>100MB par fichier)
2. Les performances de lecture/écriture deviennent problématiques
3. La complexité des requêtes nécessite un système de requêtage
4. Les données nécessitent des transactions ACID

### 1. SQLite ✅ (Recommandé)

**Pourquoi c'est acceptable :**
- Base de données embarquée (fichier unique `.db`)
- Aucune installation requise
- Bibliothèque incluse dans .NET
- Fonctionne hors ligne
- Gérée de façon autonome par l'application

**Fichier :**
```
/data/derot-my-brain.db
```

**Utilisation :**
```csharp
// Aucune configuration utilisateur requise
var connectionString = $"Data Source={dataPath}/derot-my-brain.db";
using var connection = new SqliteConnection(connectionString);
```

**Migration depuis JSON :**
- Script de migration automatique
- Lecture des fichiers JSON existants
- Import dans SQLite
- Backup des fichiers JSON originaux

---

### 2. LiteDB ✅ (Recommandé pour .NET)

**Pourquoi c'est acceptable :**
- Base de données NoSQL embarquée
- DLL unique (NuGet package)
- Aucune installation requise
- Fonctionne hors ligne
- Natif .NET, très performant
- API similaire à MongoDB

**Fichier :**
```
/data/derot-my-brain.litedb
```

**Utilisation :**
```csharp
// Aucune configuration utilisateur requise
using var db = new LiteDatabase($"{dataPath}/derot-my-brain.litedb");
var users = db.GetCollection<User>("users");
```

**Avantages :**
- Pas de schéma rigide (NoSQL)
- Requêtes LINQ
- Transactions
- Indexes automatiques

---

### 3. RavenDB Embedded ✅

**Pourquoi c'est acceptable :**
- Base de données NoSQL embarquée
- Aucune installation requise
- Fonctionne hors ligne
- Très performant

**Fichier :**
```
/data/ravendb/
```

**Utilisation :**
```csharp
// Aucune configuration utilisateur requise
EmbeddedServer.Instance.StartServer(new ServerOptions
{
    DataDirectory = $"{dataPath}/ravendb"
});
```

**Note :** Plus complexe que LiteDB, à utiliser seulement si nécessaire

---

## 📊 Comparaison des Solutions

| Critère | JSON Files | SQLite | LiteDB | RavenDB Embedded |
|---------|-----------|--------|--------|------------------|
| **Installation** | ✅ Aucune | ✅ Aucune | ✅ Aucune | ✅ Aucune |
| **Offline** | ✅ Oui | ✅ Oui | ✅ Oui | ✅ Oui |
| **Simplicité** | ✅✅✅ Très simple | ✅✅ Simple | ✅✅ Simple | ✅ Moyen |
| **Performance** | ⚠️ Limitée | ✅✅ Bonne | ✅✅✅ Excellente | ✅✅✅ Excellente |
| **Requêtes** | ❌ Limitées | ✅✅ SQL | ✅✅ LINQ | ✅✅✅ Advanced |
| **Transactions** | ❌ Non | ✅ Oui | ✅ Oui | ✅ Oui |
| **Taille Fichier** | ⚠️ Peut grossir | ✅ Optimisé | ✅ Optimisé | ✅ Optimisé |
| **Recommandé pour** | POC/V1 | V1.5+ | V2+ | V3+ |

---

## 🔄 Chemin de Migration

### Phase 1 : JSON Files (POC/V1) ✅ **ACTUEL**
- Fichiers JSON dans `/data/`
- Simple, rapide à implémenter
- Suffisant pour POC

### Phase 2 : SQLite (V1.5) - Si Nécessaire
**Déclencheurs :**
- Fichiers JSON > 50MB
- Plus de 1000 utilisateurs
- Performances dégradées

**Migration :**
1. Créer script de migration JSON → SQLite
2. Exécuter au démarrage si JSON détecté
3. Backup automatique des JSON
4. Basculer sur SQLite

### Phase 3 : LiteDB (V2) - Si Nécessaire
**Déclencheurs :**
- Besoin de requêtes complexes
- Besoin de transactions
- Besoin d'indexes

**Migration :**
1. Script de migration SQLite → LiteDB
2. Ou JSON → LiteDB directement
3. Backup automatique
4. Basculer sur LiteDB

### Phase 4 : RavenDB (V3+) - Optionnel
**Déclencheurs :**
- Besoin de fonctionnalités avancées
- Réplication
- Clustering

---

## 📝 Instructions pour Agents IA

### Règle par Défaut

**TOUJOURS utiliser JSON files sauf indication contraire explicite.**

### Quand Proposer une Alternative

Un agent IA peut proposer SQLite/LiteDB **UNIQUEMENT** si :
1. Il détecte un problème de performance concret
2. Il identifie une limitation technique des JSON
3. Il justifie clairement le besoin

### Comment Proposer

```markdown
⚠️ **Recommandation : Migration vers [SQLite/LiteDB]**

**Problème détecté :**
- [Description du problème]

**Justification :**
- [Pourquoi JSON n'est plus suffisant]

**Solution proposée :**
- [SQLite ou LiteDB]
- [Script de migration fourni]
- [Backward compatibility assurée]

**Impact utilisateur :**
- Aucune action requise (migration automatique)
- Backup automatique des données JSON
```

### Ce Qu'il NE FAUT PAS Proposer

❌ "Utilisons SQL Server pour plus de robustesse"  
❌ "PostgreSQL serait mieux pour les performances"  
❌ "MongoDB Cloud pour la scalabilité"  
❌ Toute solution nécessitant installation/configuration utilisateur

---

## 🎯 Résumé

### Pour le POC/V1 (Actuel)

**Utiliser :** JSON Files uniquement  
**Stockage :** `/data/` directory  
**Aucune exception**

### Si Complexité le Nécessite (Futur)

**Alternatives acceptables :**
1. SQLite (fichier `.db`)
2. LiteDB (fichier `.litedb`) - **Recommandé**
3. RavenDB Embedded (dossier)

**Critères :**
- ✅ Embedded (pas de serveur externe)
- ✅ Aucune installation requise
- ✅ Fonctionne offline
- ✅ Autonome (pas de config utilisateur)

### Jamais Acceptable

❌ SQL Server, PostgreSQL, MySQL, MongoDB (serveur)  
❌ Bases de données cloud  
❌ Toute solution nécessitant installation/configuration

---

## 📚 Documents Mis à Jour

| Document | Section Ajoutée | Statut |
|----------|-----------------|--------|
| **Implementation-Roadmap.md** | Technical Constraints & Storage Policy | ✅ |
| **Specifications-fonctionnelles.md** | Section 1.2.1 - Contraintes Techniques | ✅ |
| **TECHNICAL-CONSTRAINTS-Storage.md** | Document complet | ✅ |

---

## 🚀 Action Requise

**Pour tous les agents IA travaillant sur ce projet :**

1. ✅ Lire cette politique de stockage
2. ✅ Utiliser JSON files par défaut
3. ✅ Ne proposer SQLite/LiteDB que si justifié
4. ✅ Ne JAMAIS proposer SQL Server, PostgreSQL, etc.
5. ✅ Documenter toute proposition d'alternative

---

**Fin de la Politique de Stockage**
