# Guide de Compilation et d'Exécution

Ce guide explique comment compiler et lancer les applications Backend et Frontend du projet Derot-my-brain.

## Prérequis

Assurez-vous d'avoir installé les outils suivants :
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (Compatible Windows, Linux, macOS).
- [Node.js](https://nodejs.org/) (version LTS recommandée)

## Backend (.NET)

Pour compiler et lancer le backend, suivez ces étapes :

1.  **Ouvrez un terminal** (PowerShell, Bash, Zsh, etc.).
2.  Naviguez vers le dossier racine du projet (si ce n'est pas déjà fait).
3.  Déplacez-vous dans le dossier du backend :
    ```bash
    cd src/backend
    ```
4.  Restaurez les dépendances :
    ```bash
    dotnet restore
    ```
5.  Compilez le projet :
    ```bash
    dotnet build
    ```
6.  Lancez l'application :
    ```bash
    dotnet watch run --project DerotMyBrain.API
    ```

> [!NOTE]
> `dotnet watch` permet le rechargement à chaud (Hot Reload) et tentera d'ouvrir le navigateur.
> Si le navigateur ne s'ouvre pas, cliquez sur le lien affiché dans le terminal (ou Ctrl+Clic).

> [!NOTE]
> Le backend ouvrira automatiquement votre navigateur par défaut à l'adresse du Swagger UI : `http://localhost:<port>/swagger/index.html` (exemple : `http://localhost:5077/swagger/index.html`).
> C'est ici que vous pourrez tester les API.

### Exécution des tests

Pour lancer les tests unitaires et d'intégration :

```bash
dotnet test
```

## Frontend (React/Vite)

Pour lancer le frontend, suivez ces étapes dans un **nouveau terminal** :

1.  **Ouvrez un NOUVEAU terminal** (ne fermez pas celui du backend).
2.  Naviguez vers le dossier racine du projet.
3.  Déplacez-vous dans le dossier du frontend :
    ```bash
    cd src/frontend
    ```
4.  Installez les dépendances npm (à faire uniquement la première fois ou après modification du `package.json`) :
    ```bash
    npm install
    ```
5.  Lancez le serveur de développement :
    ```bash
    npm run dev
    ```

> [!NOTE]
> Le frontend sera accessible à l'adresse indiquée dans le terminal (généralement `http://localhost:5173`).

### Exécution des tests

Pour lancer les tests unitaires (Vitest) :

```bash
npm run test
```

Ou pour lancer les tests avec une interface graphique :

```bash
npm run test:ui
```

## Résumé des commandes

| Composant | Action | Terminal | Répertoire cible | Commande |
| :--- | :--- | :--- | :--- | :--- |
| **Backend** | Installation | Terminal A | `src/backend` | `dotnet restore` |
| **Backend** | Compilation | Terminal A | `src/backend` | `dotnet build` |
| **Backend** | Lancement | Terminal A | `src/backend` | `dotnet watch run --project DerotMyBrain.API` |
| **Backend** | Tests | Terminal A | `src/backend` | `dotnet test` |
| **Frontend** | Installation | Terminal B | `src/frontend` | `npm install` |
| **Frontend** | Lancement | Terminal B | `src/frontend` | `npm run dev` |
| **Frontend** | Tests | Terminal B | `src/frontend` | `npm run test` |

> [!TIP]
> Pour changer de répertoire, utilisez la commande `cd` suivie du chemin.
> Exemple : `cd src/backend` depuis la racine du projet.
