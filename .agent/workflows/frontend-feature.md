---
description: Steps to implement a new frontend feature
---
1. **Model**: Define TypeScript interfaces in `src/frontend/src/models`.
2. **API**: Add API endpoints in `src/frontend/src/api`.
3. **Store**: Add state and actions to a specialized Zustand store in `src/frontend/src/stores`.
4. **Hook**: Create a Custom Hook in `src/frontend/src/hooks` to wrap the store and API logic.
5. **Components**: Build "dumb" UI components in `src/frontend/src/components/features` using shadcn/ui.
6. **Page**: Create or update page in `src/frontend/src/pages`.
7. **I18n**: Add translations in `src/frontend/public/locales/en.json` and `fr.json`.
// turbo
8. **Verify**: Run `npm run test` or `npm run dev` to check the result.
