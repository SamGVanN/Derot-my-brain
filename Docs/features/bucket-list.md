# BUCKET-LIST (FEATURES)

## Homepage
- Date de dernière activité (avec nom de la page wikipedia)
- Nombre total d'activités
- Meilleur score personnel (score + nom de l'activité)
- une vue un peu graphique un peu comme l'historique d'un utilisateur gitlab
- 3 derniers articles explorés
- 3 derniers articles lus
- résultats des 3 derniers quiz passés

## Rendre l'application déployable
- Compatible Windoss + macOS + Linux
- Faut que lke résultat soit accessible à tous (pas de manipulation terminale sur linux ou cmd sur windows) pour accéder à l'application 

## Requirements LLM
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
- **Deletes:** All UserActivity, UserFocus, Documents, backlog etc. : all data user related EXCEPT the user, the language and theme preferences.
- **Use case:** User wants to start over with a clean slate.
- **UI:** Confirmation modal with warning about data loss


### Export PDF with his stats (same as shown in dashboard)
**Priority:** LOW  
**Target:** Post-V1 (NOT in initial release)

Allow any logged user to export their stats in a PDF file:
- **Export format:** PDF file containing user, preferences, and all activities
- **File naming:** `derot-stats-{username}-{timestamp}.pdf`
- **UI:** File explorer dialog to choose destination folder
- **Use case:** User wants to export their stats for archival or sharing
- **Implementation:** Serialize user data to PDF and save to user-selected location