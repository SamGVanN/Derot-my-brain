

# Page utilisateur
On a déja une page pour ses préférences.
Il manque la page où m'utilisateur peut changer son nom d'affichage.
Il faut donc qu'en BDD l'Id lors de la création soit unique, et lorsque l'utilisateur change son nom l'Id reste pour garder le lien vers les autres données.

# Page des préférences
Il faut ajouter dans les "General Settings", avant le choix du nombre de questions, la section de configuration du LLM:
- {champ de saisi url de l'LLM} : {champ de saisi port utilisé}
- un champ qui affiche la valeur "finale" url:port qui sera utilisé par l'application (celui-étant non modifiable car seulement à titre indicatif, il montre le résultat de la configuration des 2 champs précédents).
- si c'est plus simple : un seul champ pour saisie de l'utl complete (http://localhost:11434) et un bouton disquette pour valider: le back doit vérifier que l'URL est accessible et que le port est ouvert et afficher une modale préxcisant si le LLM a été atteint (url validée) ou si une erreur est survenue (vérifier votre url et votre port).

# Page d'historique
A droite de la liste d'historique, une section calendrier de l'activité:
- Date de dernière activité (avec nom de la page wikipedia)
- Nombre total d'activités
- Meilleur score personnel (score + nom de l'activité)
- une vue un peu graphique un peu comme l'historique d'un utilisateur gitlab.

# Navigation
Je ne sais pas si on a déja évoqué ça dans les features / roadmap mais il faut gérer une navigation : menu à gauche et URL différentes selon page affichée (actuellement peu importe où je clique je reste sur http://localhost:5173 il n'y a pas de /user/preferences par exemple et il le faut)
Il faut que le header affiche les choix langue et theme dans le header quand on est pas identifé, tandis que une fois identifié, il faut un bouton avec le nom de l'utilisateur (icone user) qui permet d'accéder à:
- son profil (formulaire donnée utilisateur comme nom affichage. Info affichée non modifiable comme date de création du compte. Un bouton pour supprimer son compte avec confirmation via modale)
- son historique
- son backlog
- se deconnecter
- Si on a prévu que l'historique et le backlog soient une seule et même page, alors un seul lien pour les 2.
Un 2e bouton d'icone roue crentée poura accéder à :
- sa page de préférences
Un 3e bouton de déconnexion

Le menu de gauche doit afficher les pages suivantes:
- accueil
- jouer
- historique
- backlog
- préférences
- profil
- se deconnecter

Ordre des boutons : préférence (roue crentée grise), user (icone user), logout (icone logout).
Le Titre de l'appli (logo+nom) doit être cliquable et doit ramener à l'accueil.
La page d'acceuil d'un utilisateur connecté est sa page d'historique.

# Rendre l'application déployable
- Compatible Windoss + macOS + Linux
- Faut que lke résultat soit accessible à tous (pas de manipulation terminale sur linux ou cmd sur windows) pour accéder à l'application 

# Requirements LLM
Selon la taille de l'article qui a été rendvoyé par l'API wikipedia, estimer  la capacité CPU/RAM nécessaire :
Phase 1 : la lecture et l'analyse de l'article par le LLM
Phase 2: la génération des questions (donc contexte IA = prompt + article wikipédia)
Phase 3: la correction des réponses (donc contexte IA = prompt + article wikipédia + questions + réponses + validation des réponses)



---

## User Data Management

### Restore Fresh (V1)
**Priority:** MEDIUM  
**Target:** V1

Allow any logged user to "restore fresh" their account:
- **Keeps:** User profile and preferences
- **Deletes:** All history, tracked topics, and activity data
- **Use case:** User wants to start over without creating a new account
- **UI:** Confirmation modal with warning about data loss
- **Backend:** DELETE all activities for user, keep user and preferences

### Backup User Data (Post-V1)
**Priority:** LOW  
**Target:** Post-V1 (NOT in initial release)

Allow any logged user to backup their data:
- **Export format:** JSON file containing user, preferences, and all activities
- **File naming:** `derot-backup-{username}-{timestamp}.json`
- **UI:** File explorer dialog to choose destination folder
- **Use case:** User wants to backup before major changes or for archival
- **Implementation:** Serialize user data to JSON and save to user-selected location

### Import User Data (Post-V1)
**Priority:** LOW  
**Target:** Post-V1 (NOT in initial release)

Allow any logged user to import previously backed up data:
- **Import format:** JSON file (from backup feature)
- **Strategy:** Add/Update based on IDs, never delete existing data
- **Conflict resolution:** If ID exists, update; if new, add
- **UI:** File explorer dialog to select backup file
- **Use case:** Restore from backup or merge data from another instance
- **Validation:** Verify JSON structure before import
