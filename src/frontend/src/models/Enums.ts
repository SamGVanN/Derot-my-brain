export type SourceType = 'Wikipedia' | 'Document' | 'Custom';

export const SourceTypes = {
    Wikipedia: 'Wikipedia' as SourceType,
    Document: 'Document' as SourceType,
    Custom: 'Custom' as SourceType
} as const;

export type ActivityType = 'Explore' | 'Read' | 'Quiz' | 'Study';

export const ActivityTypes = {
    Explore: 'Explore' as ActivityType,
    Read: 'Read' as ActivityType,
    Quiz: 'Quiz' as ActivityType,
    Study: 'Study' as ActivityType
} as const;
