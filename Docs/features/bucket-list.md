

# Page d'historique
A droite de la liste d'historique, une section calendrier de l'activité:
- Date de dernière activité (avec nom de la page wikipedia)
- Nombre total d'activités
- Meilleur score personnel (score + nom de l'activité)
- une vue un peu graphique un peu comme l'historique d'un utilisateur gitlab.

# Rendre l'application déployable
- Compatible Windoss + macOS + Linux
- Faut que lke résultat soit accessible à tous (pas de manipulation terminale sur linux ou cmd sur windows) pour accéder à l'application 

# Requirements LLM
Selon la taille de l'article qui a été rendvoyé par l'API wikipedia, estimer  la capacité CPU/RAM nécessaire :
Phase 1 : la lecture et l'analyse de l'article par le LLM
Phase 2: la génération des questions (donc contexte IA = prompt + article wikipédia)
Phase 3: la correction des réponses (donc contexte IA = prompt + article wikipédia + questions + réponses + validation des réponses)


