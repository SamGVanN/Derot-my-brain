export const ContentExtractionStatus = {
    Pending: 'Pending',
    Processing: 'Processing',
    Completed: 'Completed',
    Failed: 'Failed'
} as const;

export type ContentExtractionStatus = typeof ContentExtractionStatus[keyof typeof ContentExtractionStatus];
