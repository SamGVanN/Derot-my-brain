# MyFocusAreaPage

## Description
Il y a déja le bouton dans le menu de l'application. C'est lui qui va permettre d'accéder à la page "My Focus Area".
Cette page permet à l'utilisateur de suivre ces topic préféré : meilleur score, comparaison de son meilleur score avec sa dernière tentative de quiz, avec les dates de meileur score et date de dernière session.
Tout comme pour la page d'historique, le user peut enlever des UserFocus directement depuis la card (ici nouveau composant UserFocusCard).
MAIS sur cette page il faut que l'action soit réversible pendant quelques secondes puis si pas d'annulation un fade-out cache la card du UserFocus.
Les UserFocus sont triés par "catégories" (utiliser les mêmes catégories que disponibles dans les préférences utilisateur)
- Ne modifie pas la page d'historique, qui elle affiche tous les UserActivity peu importe s'ils sont tracké ou pas. Cependant, il faudra peut etre renommer une ou deux DTOs pour plus de cohérence.  si tu penses que tu dois modifier certaines DTOs, dis moi avant de commencer.
 
## Spécifité de la card UserFocus:
Elle prend toute une ligne et elle a 2 boutons dans une colonne à droite de la card : un séparateur vertical sépare les infos de UserFocus qui doivent prendre les 4/5 de la largeur de la card, des 2 actions ci-dessous:
- "View Stats" : Déplie la card en une card plus grande qui affiche dans son header les infos de la UserFocus activites qu'on voyait déjà MAIS le body de la card c'est la timeline de toutes les activités de cette catégorie (reprendre la timemine de la page d'historique en gros mais seulemnt avec les activité du UserFocus). Prévoir dans le header de la card un bouton précédent qui replie la Card pour rafficher tous les UserFocus. (Selon meileur UI/UX et faisabilité, proposer entre une modal ou autre solution : il faut que ce soit des tranditions visible, pas un gros changement de page qui reharcge tout un container)
- Try again: Quand on clique sur le bouton on a 2 choix : Recommencer | Skip to Quiz
    - "Recommencer" crée une nouvelle activité de type Read -> l'utilisateur arrive sur la Derot Zone en lecture de l'article.
    - "Skip to Quiz" crée une nouvelle activité directement de type Quizz -> l'utilisateur arrive sur la Derot Zone directement en mode Quiz.
Dans les 2 cas on amène sur la page DerotZone et le temps que l'activité se charge on affiche un chargement

Si tu as des doutes sur le fonctionnel, pose moi tes questions avant d'implémenter. Il me faut ta confirmation que tu as bien saisi la différence entre la page "Historique" et la page "My Focus Area".