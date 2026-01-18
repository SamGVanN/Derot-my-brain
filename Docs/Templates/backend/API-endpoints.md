
# Découpage des endpoints API

## Auth / Utilisateur

POST   /api/users
GET    /api/users

## Wikipédia
GET /api/wiki/random?categories=history,science
GET /api/wiki/page/{wikiPageId}

## Quiz
POST /api/quiz/generate
POST /api/quiz/evaluate


### Quiz / Génération

{
  "wikiPageId": "Révolution_française",
  "articleText": "..."
}


### Quiz / Évaluation

{
  "expectedAnswer": "...",
  "userAnswer": "..."
}

## Historique
GET  /api/history/{username}
POST /api/history/{username}

## Backlog
GET    /api/backlog/{username}
POST   /api/backlog/{username}
DELETE /api/backlog/{username}/{wikiPageId}