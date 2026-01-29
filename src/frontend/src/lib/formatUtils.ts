/**
 * Formats a score as a percentage.
 * @param score The raw score
 * @param total The total possible score
 * @returns The rounded percentage (0-100)
 */
export const calculatePercentage = (score: number, total: number): number => {
    if (!total) return 0;
    return Math.round((score / total) * 100);
};

/**
 * Formats a score for display.
 * @param score The raw score
 * @param total The total possible score (optional)
 * @returns formatted string (e.g. "9/10" or "90%")
 */
export const formatScoreDisplay = (score: number, total?: number): string => {
    if (total) {
        return `${score}/${total}`;
    }
    return `${score}%`;
};

/**
 * Checks if the current activity represents the best score for a tracked topic.
 * @param score The current activity score
 * @param bestScore The best score object from statistics
 * @param isTracked Whether the topic is tracked
 * @returns boolean
 */
export const isBestScore = (
    score: number | undefined | null,
    bestScore: { score: number } | undefined,
    isTracked: boolean
): boolean => {
    if (!isTracked || !bestScore || score === undefined || score === null) {
        return false;
    }
    return score === bestScore.score;
};
/**
 * Formats a duration in seconds into a human-readable string.
 * @param seconds The duration in seconds
 * @returns Formatted string (e.g. "5m 30s" or "1h 20m")
 */
export const formatDuration = (seconds: number): string => {
    if (!seconds || seconds < 0) return '0s';

    const h = Math.floor(seconds / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = seconds % 60;

    if (h > 0) {
        return `${h}h ${m}m`;
    }
    if (m > 0) {
        return `${m}m ${s}s`;
    }
    return `${s}s`;
};
