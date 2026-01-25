# Derot Zone — Wikipedia Integration Progress

Status: In progress — handoff-ready (stop point: E2E stabilization)

Résumé (court)
---------------
- Conception et spécification : `Docs/Technical/Wikipedia-Integration.md` et mise à jour des specs fonctionnelles.
- Backend : modifications d'entité/DTO, service `ActivityService` ajusté, migration EF créée/appliquée, contrôleurs mis à jour, tests backend unit/integration verts.
- Frontend : composant `DerotZone` ajouté + tests unitaires et API-mock passés ; test E2E qui lance l'API ajouté mais encore instable.

Fait (détaillé)
---------------
- Ajout des champs `ResultingReadActivityId` et `BacklogAddsCount` sur `UserActivity`.
- Mise à jour des DTOs `CreateActivityDto`, `UserActivityDto`, `UpdateActivityDto` pour exposer ces champs.
- Implémentation partielle du lien Explore→Read : `ActivityService` accepte `OriginExploreId` et met à jour `ResultingReadActivityId` quand applicable.
- Scaffolding et application d'une migration EF (nommée `AutoPendingModelChanges_20260125` / `20260125_AddExploreFieldsToUserActivity`) et index/FK créés.
- Tests backend (xUnit) : 19 tests exécutés — tous réussis.
- Frontend : `src/frontend/src/components/DerotZone/DerotZone.tsx` ajouté ; tests Vitest ciblés (`DerotZone.spec.tsx`, `DerotZone.api.spec.tsx`) passés.
- Script d'utilitaire `scripts/kill-derot-api.ps1` ajouté pour tuer les processus `DerotMyBrain.API`/`dotnet` bloquants.

État en cours / blocages connus
-------------------------------
- Test E2E (`DerotZone.e2e.spec.ts`) qui spawn l'API : instable — symptômes observés : 404 pour `POST /api/wikipedia/explore`, timeouts, ou "address already in use" quand des instances antérieures restent en mémoire.
- Suite frontend complète : plusieurs tests non liés (preferences) échouent; pour validation rapide, utiliser les tests ciblés Derot Zone.

Reproduction locale — étapes pour reprendre et stabiliser
-------------------------------------------------------
1) Assurer qu'aucune instance de l'API ne tourne (exécuter depuis la racine du repo) :

```powershell
./scripts/kill-derot-api.ps1
```

2) Build & appliquer migrations (depuis la racine du repo) :

```powershell
cd src/backend
dotnet restore
dotnet build
dotnet ef database update --project DerotMyBrain.Infrastructure --startup-project DerotMyBrain.API
```

3) Lancer l'API pour E2E (si vous voulez que le test réutilise un serveur existant) :

```powershell
dotnet run --project DerotMyBrain.API --urls "http://localhost:5005;http://localhost:5077"
```

4) Lancer le test E2E isolé (frontend) — depuis `src/frontend` :

```bash
npx vitest run src/components/DerotZone/DerotZone.e2e.spec.ts --run
```

Notes pratiques :
- Le test E2E actuel cherche plusieurs URLs candidates (`http://localhost:5005`, `http://localhost:5077`, etc.). Préfixer `--urls` au démarrage de l'API aide la découverte.
- Toujours appeler `./scripts/kill-derot-api.ps1` avant de spawn un serveur depuis un test pour éviter les conflits de port / verrous d'exécutable.

Fichiers-clés / où regarder
---------------------------
- Backend entity/DTO/migration :
	- src/backend/DerotMyBrain.Core/Entities/UserActivity.cs
	- src/backend/DerotMyBrain.Core/DTOs/CreateActivityDto.cs
	- src/backend/DerotMyBrain.Infrastructure/Migrations/*20260125*
	- src/backend/DerotMyBrain.API/Controllers/ActivitiesController.cs
- Backend service :
	- src/backend/DerotMyBrain.Core/Services/ActivityService.cs
- Frontend DerotZone :
	- src/frontend/src/components/DerotZone/DerotZone.tsx
	- src/frontend/src/components/DerotZone/DerotZone.spec.tsx
	- src/frontend/src/components/DerotZone/DerotZone.api.spec.tsx
	- src/frontend/src/components/DerotZone/DerotZone.e2e.spec.ts
- Scripts utilitaires :
	- scripts/kill-derot-api.ps1

Etat des tests
---------------
- Backend: `dotnet test` (src/backend) — 19 passed, 0 failed.
- Frontend focused: `npx vitest run src/components/DerotZone/DerotZone.spec.tsx` & `DerotZone.api.spec.tsx` — passed.
- Frontend full suite: plusieurs tests non liés échouent; ne pas exécuter la suite complète pendant la stabilisation E2E.

Prochaines étapes recommandées (priorisées pour reprise)
-------------------------------------------------------
1. Stabiliser l'E2E (haute priorité)
	 - S'assurer que `scripts/kill-derot-api.ps1` est exécuté avant de spawn l'API.
	 - Modifier le test E2E pour :
		 - Réutiliser un serveur existant si trouvé (cible préférée).
		 - Sinon, spawn le serveur avec `--urls` explicites et attendre la route `/health` ou `/swagger` pour readiness.
		 - Garantir l'arrêt propre du processus spawné en `afterAll`.

2. Finaliser frontend UX (moyenne)
	 - Ajouter états `loading` / `error` pour `DerotZone`.
	 - Ajouter navigation vers la page Read après création `UserActivity` Type=`Read`.

3. Nettoyage tests & CI (moyenne)
	 - Isoler les tests Derot Zone dans un groupe ou pattern pour CI afin d'éviter blocage par tests preferences.

4. Documentation & PR (basse)
	 - Préparer un PR contenant la migration et les changements DTO/service + notes de déploiement.

Handoff rapide pour le prochain agent
-----------------------------------
- Point d'arrêt : E2E instable — prioriser stabilité du test qui spawn le backend.
- Valeurs utiles : `TestUser` id = `test-user-id-001` (seed conventionnelle utilisée par les tests).
- Ports candidats : `5005`, `5077` (ajouter d'autres si besoin dans le test).
- Comportement attendu : POST `api/wikipedia/explore` crée une `UserActivity` Type=`Explore` avec `SourceType/SourceId/SourceHash` non nuls ; POST `api/wikipedia/read` avec `OriginExploreId` doit créer `Read` et mettre à jour l'Explore `ResultingReadActivityId`.

Si vous voulez, je peux maintenant :
- (A) stabiliser immédiatement le test E2E (modifier le spec pour reuse/spawn/teardown propre), ou
- (B) ouvrir un PR draft avec la migration + notes pour relecteur.

---
Document mise à jour le: 2026-01-25

