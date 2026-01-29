import { describe, it, expect, vi } from 'vitest';
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ActivityTimelineItem } from '../ActivityTimelineItem';
import type { UserActivity } from '../../models/UserActivity';
import { MemoryRouter } from 'react-router';
import { TooltipProvider } from '../ui/tooltip';
import { activityApi } from '../../api/activityApi';

// Mock translation
vi.mock('react-i18next', () => ({
    useTranslation: () => ({
        t: (key: string, defaultVal?: string) => defaultVal ?? key,
        i18n: { language: 'en' }
    }),
}));

// Mock activityApi
vi.mock('../../api/activityApi', () => ({
    activityApi: {
        read: vi.fn(),
    },
}));

const mockActivity: UserActivity = {
    id: '1',
    userId: 'user1',
    userSessionId: 'session1',
    type: 'Read',
    title: 'Test Topic',
    description: 'Test Description',
    externalId: 'http://test.com',
    sourceId: 'src1',
    sourceType: 'Wikipedia',
    sessionDateStart: '2023-10-27T10:00:00Z',
    score: 80,
    questionCount: 10,
    scorePercentage: 80,
    durationSeconds: 120,
    totalDurationSeconds: 120,
    isNewBestScore: false,
    isBaseline: false,
    isCurrentBest: false,
    isCompleted: true,
    isTracked: false
};

const renderWithProviders = (ui: React.ReactElement) => {
    return render(
        <MemoryRouter>
            <TooltipProvider>
                {ui}
            </TooltipProvider>
        </MemoryRouter>
    );
};

describe('ActivityTimelineItem', () => {
    it('renders activity details correctly', () => {
        renderWithProviders(
            <ActivityTimelineItem
                activity={mockActivity}
                isTracked={false}
                onTrack={vi.fn()}
                onUntrack={vi.fn()}
            />
        );

        expect(screen.getByText(/Test Topic/)).toBeDefined();
        expect(screen.getByText(/Read/)).toBeDefined();
        // formatDuration(120) should show "2m 0s"
        expect(screen.getByText(/2m 0s/)).toBeDefined();
    });

    it('displays action buttons for Read activity', () => {
        renderWithProviders(
            <ActivityTimelineItem
                activity={mockActivity}
                isTracked={false}
                onTrack={vi.fn()}
                onUntrack={vi.fn()}
            />
        );

        expect(screen.getByText('history.reRead')).toBeDefined();
        expect(screen.getByText('history.takeQuiz')).toBeDefined();
    });

    it('triggers activityApi.read when Re-read is clicked', async () => {
        vi.mocked(activityApi.read).mockResolvedValue({ id: 'new-act' } as any);

        renderWithProviders(
            <ActivityTimelineItem
                activity={mockActivity}
                isTracked={false}
                onTrack={vi.fn()}
                onUntrack={vi.fn()}
            />
        );

        const reReadBtn = screen.getByText('history.reRead');
        fireEvent.click(reReadBtn);

        await waitFor(() => {
            expect(activityApi.read).toHaveBeenCalledWith('user1', expect.objectContaining({
                type: 'Read',
                sourceId: 'src1'
            }));
        });
    });

    it('triggers activityApi.read with type Quiz when Take Quiz is clicked', async () => {
        vi.mocked(activityApi.read).mockResolvedValue({ id: 'new-act' } as any);

        renderWithProviders(
            <ActivityTimelineItem
                activity={mockActivity}
                isTracked={false}
                onTrack={vi.fn()}
                onUntrack={vi.fn()}
            />
        );

        const quizBtn = screen.getByText('history.takeQuiz');
        fireEvent.click(quizBtn);

        await waitFor(() => {
            expect(activityApi.read).toHaveBeenCalledWith('user1', expect.objectContaining({
                type: 'Quiz',
                sourceId: 'src1'
            }));
        });
    });

    describe('Motivational Text', () => {
        it('does not render message for <= 60%', () => {
            renderWithProviders(
                <ActivityTimelineItem
                    activity={{ ...mockActivity, type: 'Quiz', score: 6, questionCount: 10 }}
                    isTracked={false}
                    onTrack={vi.fn()}
                    onUntrack={vi.fn()}
                />
            );
            expect(screen.queryByText(/history.motivational/)).toBeNull();
        });

        it('renders Tier 1 message for 61-70%', () => {
            renderWithProviders(
                <ActivityTimelineItem
                    activity={{ ...mockActivity, type: 'Quiz', score: 65, questionCount: 100 }}
                    isTracked={false}
                    onTrack={vi.fn()}
                    onUntrack={vi.fn()}
                />
            );
            expect(screen.getByText('history.motivational.tier1')).toBeDefined();
        });

        it('renders Perfect message and party icon for 100%', () => {
            const { container } = renderWithProviders(
                <ActivityTimelineItem
                    activity={{ ...mockActivity, type: 'Quiz', score: 10, questionCount: 10 }}
                    isTracked={false}
                    onTrack={vi.fn()}
                    onUntrack={vi.fn()}
                />
            );
            expect(screen.getByText('history.motivational.perfect')).toBeDefined();
            const partyIcon = container.querySelector('.lucide-party-popper');
            expect(partyIcon).toBeDefined();
        });
    });
});
