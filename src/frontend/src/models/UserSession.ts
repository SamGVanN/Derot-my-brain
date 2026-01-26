import type { SourceType } from './Enums';
import type { UserActivity } from './UserActivity';

export interface Source {
    id: string;
    type: SourceType;
    externalId: string;
    displayTitle: string;
    url?: string;
}

export type SessionStatus = 'Active' | 'Stopped';

export interface UserSession {
    id: string;
    userId: string;
    sourceId?: string;
    source?: Source;
    startedAt: string;
    endedAt?: string;
    status: SessionStatus;
    activities: UserActivity[];
}
