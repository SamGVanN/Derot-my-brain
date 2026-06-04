# Derot My Brain - Guide d'Installation

Bienvenue ! Ce guide vous explique comment installer et utiliser l'application Derot My Brain sur votre ordinateur.

## Qu'est-ce que Derot My Brain ?

Derot My Brain est une application de gestion de connaissances qui vous permet de:
- Organiser vos ressources d'apprentissage
- Créer des quiz pour tester vos connaissances
- Suivre vos activités de lecture et d'apprentissage

## Installation

### Windows

1. **Télécharger** le fichier `DerotMyBrain-win-x64.zip`
2. **Extraire** le fichier ZIP dans un dossier de votre choix (ex: `C:\DerotMyBrain\`)
3. **Lancer** l'application:
   - Double-cliquez sur `DerotMyBrain.exe`
   - Une fenêtre de console peut s'afficher (c'est normal)
4. **Ouvrir votre navigateur** et aller sur `http://localhost:5000`

> **Note**: Windows Defender peut afficher un avertissement pour les applications non reconnues. Cliquez sur "Plus d'infos" puis "Exécuter quand même" si vous faites confiance à l'application.

### macOS

1. **Télécharger** le bon fichier:
   - Mac avec puce Apple (M1/M2/M3): `DerotMyBrain-osx-arm64.zip`
   - Mac avec processeur Intel: `DerotMyBrain-osx-x64.zip`
2. **Extraire** le fichier ZIP
3. **Ouvrir le Terminal** et naviguer vers le dossier:
   ```bash
   cd ~/Downloads/DerotMyBrain  # Ajustez le chemin selon votre dossier
   ```
4. **Rendre l'exécutable** (première fois seulement):
   ```bash
   chmod +x DerotMyBrain
   ```
5. **Lancer** l'application:
   ```bash
   ./DerotMyBrain
   ```
6. **Ouvrir votre navigateur** et aller sur `http://localhost:5000`

> **Note**: macOS peut bloquer l'application car elle n'est pas signée. Allez dans Préférences Système > Sécurité et Confidentialité pour autoriser l'exécution.

### Linux

1. **Télécharger** le fichier `DerotMyBrain-linux-x64.zip`
2. **Extraire** le fichier ZIP:
   ```bash
   unzip DerotMyBrain-linux-x64.zip -d ~/DerotMyBrain
   ```
3. **Naviguer** vers le dossier:
   ```bash
   cd ~/DerotMyBrain
   ```
4. **Rendre l'exécutable**:
   ```bash
   chmod +x DerotMyBrain
   ```
5. **Lancer** l'application:
   ```bash
   ./DerotMyBrain
   ```
6. **Ouvrir votre navigateur** et aller sur `http://localhost:5000`

## Premier Lancement

Au premier lancement, l'application va:
1. Créer une base de données SQLite dans le dossier d'installation
2. Initialiser les données de base
3. Démarrer le serveur web

Vous verrez des messages dans la console indiquant le démarrage de l'application.

## Configuration Initiale

1. **Créer un compte**: Lors de votre première visite, créez un compte utilisateur
2. **Configurer le LLM** (optionnel): Si vous souhaitez utiliser les fonctionnalités d'IA, configurez votre clé API dans les paramètres

## Utilisation Quotidienne

### Démarrer l'Application

**Windows**: Double-cliquez sur `DerotMyBrain.exe`

**macOS/Linux**:
```bash
cd /chemin/vers/DerotMyBrain
./DerotMyBrain
```

### Accéder à l'Interface

Ouvrez votre navigateur préféré et allez sur: `http://localhost:5000`

> **Astuce**: Créez un favori dans votre navigateur pour accéder rapidement à l'application!

### Arrêter l'Application

- Fermez la fenêtre de console (Windows)
- Appuyez sur `Ctrl+C` dans le terminal (macOS/Linux)

### Arrêter l'Application

Puisque l'application n'affiche pas de console, voici comment l'arrêter proprement :

**Méthode 1: Gestionnaire des Tâches (Recommandé)**
1. Appuyez sur `Ctrl + Shift + Esc` pour ouvrir le Gestionnaire des tâches
2. Cherchez "DerotMyBrain.API" dans la liste des processus
3. Cliquez dessus puis cliquez sur "Fin de tâche"

**Méthode 2: PowerShell**
```powershell
# Trouver le processus
Get-Process | Where-Object {$_.ProcessName -like "*DerotMyBrain*"}

# L'arrêter (remplacez ID_DU_PROCESSUS par le numéro affiché)
Stop-Process -Name "DerotMyBrain.API" -Force
```

> **Astuce**: Fermer le navigateur ne ferme PAS l'application. Le serveur continue de tourner en arrière-plan.

## Emplacement des Données

Toutes vos données sont stockées dans le dossier où vous avez installé l'application:

- **Base de données**: `DerotMyBrain.db`
- **Fichiers uploadés**: Dans le sous-dossier `uploads/`
- **Logs**: Dans le sous-dossier `Logs/`

> **Sauvegarde**: Pour sauvegarder vos données, copiez simplement le fichier `DerotMyBrain.db` et le dossier `uploads/`

## Mise à Jour

Pour mettre à jour l'application:

1. **Sauvegarder** vos données (`DerotMyBrain.db` et `uploads/`)
2. **Télécharger** la nouvelle version
3. **Extraire** dans le même dossier (ou un nouveau)
4. **Copier** vos fichiers de données sauvegardés dans le nouveau dossier
5. **Lancer** la nouvelle version

## Dépannage

### L'application ne démarre pas

- Vérifiez que le port 5000 n'est pas déjà utilisé par une autre application
- Vérifiez les permissions d'exécution (macOS/Linux: `chmod +x DerotMyBrain`)
- Consultez les logs dans le dossier `Logs/`

### Impossible d'accéder à l'interface

- Vérifiez que l'application est bien démarrée (console affichant des messages)
- Essayez `http://127.0.0.1:5000` au lieu de `localhost`
- Vérifiez votre pare-feu

### Erreur "Port déjà utilisé"

Une autre application utilise le port 5000. Pour changer le port:

**Windows**:
```powershell
$env:ASPNETCORE_URLS="http://localhost:5001"
.\DerotMyBrain.exe
```

**macOS/Linux**:
```bash
export ASPNETCORE_URLS="http://localhost:5001"
./DerotMyBrain
```

### Perte de données

Vos données sont dans le fichier `DerotMyBrain.db`. Assurez-vous de:
- Ne pas supprimer ce fichier
- Le sauvegarder régulièrement
- Le conserver lors des mises à jour

## Questions Fréquentes

### Ai-je besoin d'une connexion Internet ?

Non, l'application fonctionne entièrement en local. Une connexion est nécessaire uniquement si vous utilisez les fonctionnalités d'IA (LLM).

### Puis-je utiliser l'application sur plusieurs ordinateurs ?

Oui, mais chaque installation aura sa propre base de données. Pour synchroniser, copiez le fichier `DerotMyBrain.db` entre les machines.

### L'application est-elle sécurisée ?

L'application fonctionne uniquement sur votre machine locale. Vos données ne sont pas envoyées sur Internet (sauf si vous utilisez un LLM externe).

## Support

Pour toute question ou problème:
- Consultez la documentation dans le dossier `Docs/`
- Ouvrez une issue sur le dépôt GitHub
- Consultez les logs dans `Logs/log-[date].txt`

## Désinstallation

Pour désinstaller l'application:
1. Arrêtez l'application
2. Supprimez le dossier d'installation
3. C'est tout ! Aucun fichier système n'a été modifié

---

**Bon apprentissage avec Derot My Brain ! 📚✨**
