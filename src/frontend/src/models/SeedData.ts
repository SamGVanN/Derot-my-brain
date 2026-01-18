/**
 * Wikipedia category for article filtering.
 * These are the 13 official Wikipedia main categories.
 */
export interface WikipediaCategory {
    /** Unique identifier (e.g., "culture-arts") */
    id: string;
    /** English name (e.g., "Culture and the arts") */
    name: string;
    /** French name (e.g., "Culture et arts") */
    nameFr: string;
    /** Display order (1-13) */
    order: number;
    /** Whether this category is active */
    isActive: boolean;
}

/**
 * UI theme/color palette
 */
export interface Theme {
    /** Unique identifier (e.g., "derot-brain") */
    id: string;
    /** Display name (e.g., "Derot Brain") */
    name: string;
    /** Theme description */
    description: string;
    /** Whether this is the default theme */
    isDefault: boolean;
    /** Whether this theme is active */
    isActive: boolean;
}
