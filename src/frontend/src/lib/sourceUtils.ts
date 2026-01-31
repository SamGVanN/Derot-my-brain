import { SourceTypes, type SourceType } from "@/models/Enums";

/**
 * Maps a frontend SourceType string (or numeric enum value from backend) to its corresponding backend integer ID.
 * @param type The SourceType string or numeric value
 * @returns The integer ID (1: Wikipedia, 2: Document, 3: Custom)
 */
export const mapSourceTypeToNumber = (type?: string | SourceType | number): number => {
    if (typeof type === 'number') return type;

    switch (type) {
        case 'Wikipedia': return SourceTypes.Wikipedia;
        case 'Document': return SourceTypes.Document;
        case 'Custom': return SourceTypes.Custom;
        case 'WebLink': return SourceTypes.WebLink;
        default: return SourceTypes.Wikipedia;
    }
};

/**
 * Returns a human-readable label for a source type.
 * @param type The SourceType string or numeric value from backend
 * @returns A string label (e.g., 'Wikipedia', 'Document')
 */
export const getSourceTypeLabel = (type?: string | SourceType | number): string => {
    if (typeof type === 'number') {
        switch (type) {
            case SourceTypes.Wikipedia: return 'Wikipedia';
            case SourceTypes.Document: return 'Document';
            case SourceTypes.Custom: return 'Custom';
            case SourceTypes.WebLink: return 'Link';
            default: return 'Wikipedia';
        }
    }

    switch (type) {
        case 'Wikipedia': return 'Wikipedia';
        case 'Document': return 'Document';
        case 'Custom': return 'Custom';
        case 'WebLink': return 'Link';
        default: return 'Wikipedia';
    }
};
