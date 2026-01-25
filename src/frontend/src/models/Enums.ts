export type SourceType = 'Wikipedia' | 'Document';

export const SourceTypes = {
    Wikipedia: 'Wikipedia' as SourceType,
    Document: 'Document' as SourceType
} as const;

export type ActivityType = 'Read' | 'Quiz';

export const ActivityTypes = {
    Read: 'Read' as ActivityType,
    Quiz: 'Quiz' as ActivityType
} as const;
