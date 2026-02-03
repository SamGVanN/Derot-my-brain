export const SourceTypes = {
    Wikipedia: 1,
    Document: 2,
    Custom: 3,
    WebLink: 4
} as const;

export type SourceType = typeof SourceTypes[keyof typeof SourceTypes];

export type ActivityType = 'Explore' | 'Read' | 'Quiz' | 'Study';

export const ActivityTypes = {
    Explore: 'Explore' as ActivityType,
    Read: 'Read' as ActivityType,
    Quiz: 'Quiz' as ActivityType,
    Study: 'Study' as ActivityType
} as const;
