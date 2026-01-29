import type { SourceType } from "@/models/Enums";

/**
 * Maps a frontend SourceType string to its corresponding backend integer ID.
 * @param type The SourceType string
 * @returns The integer ID (1: Wikipedia, 2: Document, 3: Custom)
 */
export const mapSourceTypeToNumber = (type?: string | SourceType): number => {
    switch (type) {
        case 'Wikipedia': return 1;
        case 'Document': return 2;
        case 'Custom': return 3;
        default: return 1;
    }
};
