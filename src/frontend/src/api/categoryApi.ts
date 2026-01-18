import { client } from './client';
// Use local interface definition if the model file is not easily accessible or causes circular deps,
// but ideally we should import from a shared models folder.
// Checking previous view_file of UserPreferencesPage might show imports.
// Assuming same models usage as hooks.

// We need to define or import WikipediaCategory.
// Based on useCategories.ts (which likely has the interface locally or imported), let's assume we need to define it or import.
// I will import it if I spot it in models, otherwise define it.

export interface WikipediaCategory {
    id: string;
    name: string;
    nameFr: string;
    order: number;
    isActive: boolean;
}

export const categoryApi = {
    getAllCategories: async (): Promise<WikipediaCategory[]> => {
        const response = await client.get<WikipediaCategory[]>('/categories');
        return response.data;
    }
};
