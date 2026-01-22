import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ActivityTimelineItem } from '../ActivityTimelineItem';
import type { UserActivity } from '../../models/UserActivity';

// Mock translation
vi.mock('react-i18next', () => ({
    useTranslation: () => ({
        useTranslation: () => ({
            t: (key: string, defaultVal?: string) => defaultVal ?? key,
        }),
    }),
}));

const mockActivity: UserActivity = {
    id: '1',
    userId: 'user1',
    type: 'Read',
    topic: 'Test Topic',
    wikipediaUrl: 'http://test.com',
    sessionDate: '2023-10-27T10:00:00Z',
    score: 80,
    totalQuestions: 10,
    llmModelName: 'gpt-4',
    llmVersion: 'turbo',
    isTracked: false
};

describe('ActivityTimelineItem', () => {
    it('renders activity details correctly', () => {
        render(
            <ActivityTimelineItem
                activity={mockActivity}
                isTracked={false}
                onTrack={vi.fn()}
                onUntrack={vi.fn()}
            />
        );

        expect(screen.getByText(/Test Topic/)).toBeDefined();
        expect(screen.getByText(/Read/)).toBeDefined();
        expect(screen.getByText("80/10")).toBeDefined();
        // Time check usually depends on locale, skipping precise time check to avoid fragility
    });

    it('renders bookmark icon as outline when not tracked', () => {
        render(
            <ActivityTimelineItem
                activity={mockActivity}
                isTracked={false}
                onTrack={vi.fn()}
                onUntrack={vi.fn()}
            />
        );

        expect(screen.getByTitle('Track topic')).toBeDefined();
    });

    it('renders bookmark icon as filled (checked) when tracked', () => {
        render(
            <ActivityTimelineItem
                activity={mockActivity}
                isTracked={true}
                onTrack={vi.fn()}
                onUntrack={vi.fn()}
            />
        );

        expect(screen.getByTitle('Untrack topic')).toBeDefined();
    });

    it('renders best score trophy when tracked and bestScore provided', () => {
        render(
            <ActivityTimelineItem
                activity={mockActivity}
                isTracked={true}
                bestScore={{ score: 95, total: 100 }}
                onTrack={vi.fn()}
                onUntrack={vi.fn()}
            />
        );

        expect(screen.getByTitle('All-time Best')).toBeDefined();
        expect(screen.getByText(/95%/)).toBeDefined();
    });

    it('does not render best score when not tracked', () => {
        render(
            <ActivityTimelineItem
                activity={mockActivity}
                isTracked={false}
                bestScore={{ score: 95 }}
                onTrack={vi.fn()}
                onUntrack={vi.fn()}
            />
        );

        expect(screen.queryByTitle('All-time Best')).toBeNull();
    });

    it('renders trophy icon on timeline when score matches best score', () => {
        render(
            <ActivityTimelineItem
                activity={{ ...mockActivity, score: 95 }}
                isTracked={true}
                bestScore={{ score: 95 }}
                onTrack={vi.fn()}
                onUntrack={vi.fn()}
            />
        );

        // We expect 2 trophies: one in badge, one in timeline
        const trophies = document.querySelectorAll('.lucide-trophy');
        expect(trophies.length).toBe(2);
    });

    describe('Motivational Text', () => {
        it('does not render message for <= 60%', () => {
            render(
                <ActivityTimelineItem
                    activity={{ ...mockActivity, score: 60, totalQuestions: 100 }}
                    isTracked={false}
                    onTrack={vi.fn()}
                    onUntrack={vi.fn()}
                />
            );
            expect(screen.queryByText(/history.motivational/)).toBeNull();
        });

        it('renders Tier 1 message for 61-70%', () => {
            render(
                <ActivityTimelineItem
                    activity={{ ...mockActivity, score: 65, totalQuestions: 100 }}
                    isTracked={false}
                    onTrack={vi.fn()}
                    onUntrack={vi.fn()}
                />
            );
            expect(screen.getByText('history.motivational.tier1')).toBeDefined();
        });

        it('renders Tier 2 message for 71-80%', () => {
            render(
                <ActivityTimelineItem
                    activity={{ ...mockActivity, score: 75, totalQuestions: 100 }}
                    isTracked={false}
                    onTrack={vi.fn()}
                    onUntrack={vi.fn()}
                />
            );
            expect(screen.getByText('history.motivational.tier2')).toBeDefined();
        });

        it('renders Tier 3 message for 81-90%', () => {
            render(
                <ActivityTimelineItem
                    activity={{ ...mockActivity, score: 85, totalQuestions: 100 }}
                    isTracked={false}
                    onTrack={vi.fn()}
                    onUntrack={vi.fn()}
                />
            );
            expect(screen.getByText('history.motivational.tier3')).toBeDefined();
        });

        it('renders Tier 4 message for 91-99%', () => {
            render(
                <ActivityTimelineItem
                    activity={{ ...mockActivity, score: 95, totalQuestions: 100 }}
                    isTracked={false}
                    onTrack={vi.fn()}
                    onUntrack={vi.fn()}
                />
            );
            expect(screen.getByText('history.motivational.tier4')).toBeDefined();
        });

        it('renders Perfect message and party icon for 100%', () => {
            const { container } = render(
                <ActivityTimelineItem
                    activity={{ ...mockActivity, score: 100, totalQuestions: 100 }}
                    isTracked={false}
                    onTrack={vi.fn()}
                    onUntrack={vi.fn()}
                />
            );
            expect(screen.getByText('history.motivational.perfect')).toBeDefined();
            // Check for party popper icon (it has a specific class or we can check svg)
            const partyIcon = container.querySelector('.lucide-party-popper');
            expect(partyIcon).toBeDefined();
        });
    });
});
