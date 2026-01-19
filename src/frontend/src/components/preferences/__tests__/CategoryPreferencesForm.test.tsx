import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CategoryPreferencesForm } from '../CategoryPreferencesForm';
import type { WikipediaCategory } from '@/models/SeedData';

// Mock ResizeObserver (required for ScrollArea component)
(globalThis as any).ResizeObserver = class ResizeObserver {
    observe() { }
    unobserve() { }
    disconnect() { }
};

// Mock dependencies
vi.mock('react-i18next', () => ({
    useTranslation: () => ({
        t: (key: string, defaultValue?: string) => defaultValue || key,
        i18n: {
            language: 'en',
        },
    }),
}));

vi.mock('@/hooks/useCategories', () => ({
    useCategories: vi.fn(),
}));

import { useCategories } from '@/hooks/useCategories';

const mockCategories: WikipediaCategory[] = [
    { id: 'culture-arts', name: 'Culture and the arts', nameFr: 'Culture et arts', order: 1, isActive: true },
    { id: 'history-events', name: 'History and events', nameFr: 'Histoire et événements', order: 2, isActive: true },
    { id: 'natural-sciences', name: 'Natural and physical sciences', nameFr: 'Sciences naturelles', order: 3, isActive: true },
    { id: 'technology-sciences', name: 'Technology and applied sciences', nameFr: 'Technologie et sciences appliquées', order: 4, isActive: true },
];

describe('CategoryPreferencesForm', () => {
    const mockOnSave = vi.fn();

    beforeEach(() => {
        vi.clearAllMocks();
        (useCategories as any).mockReturnValue({
            categories: mockCategories,
            loading: false,
            error: null,
        });
    });

    describe('Rendering', () => {
        it('renders category list correctly', () => {
            render(
                <CategoryPreferencesForm
                    selectedCategories={[]}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            expect(screen.getByText('Culture and the arts')).toBeInTheDocument();
            expect(screen.getByText('History and events')).toBeInTheDocument();
            expect(screen.getByText('Natural and physical sciences')).toBeInTheDocument();
            expect(screen.getByText('Technology and applied sciences')).toBeInTheDocument();
        });

        it('shows loading state while fetching categories', () => {
            (useCategories as any).mockReturnValue({
                categories: [],
                loading: true,
                error: null,
            });

            render(
                <CategoryPreferencesForm
                    selectedCategories={[]}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            // When loading, categories should not be rendered
            expect(screen.queryByText('Culture and the arts')).not.toBeInTheDocument();
        });

        it('shows error state if fetch fails', () => {
            const errorMessage = 'Failed to load categories';
            (useCategories as any).mockReturnValue({
                categories: [],
                loading: false,
                error: errorMessage,
            });

            render(
                <CategoryPreferencesForm
                    selectedCategories={[]}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            expect(screen.getByText(errorMessage)).toBeInTheDocument();
        });

        it('displays categories in English by default', () => {
            render(
                <CategoryPreferencesForm
                    selectedCategories={[]}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            expect(screen.getByText('Culture and the arts')).toBeInTheDocument();
            expect(screen.queryByText('Culture et arts')).not.toBeInTheDocument();
        });
    });

    describe('Category Selection', () => {
        it('can check individual categories', async () => {
            const user = userEvent.setup();
            render(
                <CategoryPreferencesForm
                    selectedCategories={[]}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const checkbox = screen.getByRole('checkbox', { name: /culture and the arts/i });
            await user.click(checkbox);

            expect(checkbox).toBeChecked();
        });

        it('can uncheck individual categories', async () => {
            const user = userEvent.setup();
            render(
                <CategoryPreferencesForm
                    selectedCategories={['culture-arts']}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const checkbox = screen.getByRole('checkbox', { name: /culture and the arts/i });
            expect(checkbox).toBeChecked();

            await user.click(checkbox);

            expect(checkbox).not.toBeChecked();
        });

        it('checked state reflects selectedCategories prop', () => {
            render(
                <CategoryPreferencesForm
                    selectedCategories={['culture-arts', 'history-events']}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            expect(screen.getByRole('checkbox', { name: /culture and the arts/i })).toBeChecked();
            expect(screen.getByRole('checkbox', { name: /history and events/i })).toBeChecked();
            expect(screen.getByRole('checkbox', { name: /natural and physical sciences/i })).not.toBeChecked();
            expect(screen.getByRole('checkbox', { name: /technology and applied sciences/i })).not.toBeChecked();
        });

        it('enables save button when selection changes', async () => {
            const user = userEvent.setup();
            render(
                <CategoryPreferencesForm
                    selectedCategories={['culture-arts']}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeDisabled();

            const checkbox = screen.getByRole('checkbox', { name: /history and events/i });
            await user.click(checkbox);

            expect(saveButton).toBeEnabled();
        });

        it('calls onSave with correct category IDs when saved', async () => {
            const user = userEvent.setup();
            render(
                <CategoryPreferencesForm
                    selectedCategories={['culture-arts']}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const checkbox = screen.getByRole('checkbox', { name: /history and events/i });
            await user.click(checkbox);

            const saveButton = screen.getByRole('button', { name: /save/i });
            await user.click(saveButton);

            expect(mockOnSave).toHaveBeenCalledWith(['culture-arts', 'history-events']);
        });
    });

    describe('Select All/Deselect All', () => {
        it('select all checks all categories', async () => {
            const user = userEvent.setup();
            render(
                <CategoryPreferencesForm
                    selectedCategories={[]}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const selectAllButton = screen.getByRole('button', { name: /all/i });
            await user.click(selectAllButton);

            mockCategories.forEach(cat => {
                const checkbox = screen.getByRole('checkbox', { name: new RegExp(cat.name, 'i') });
                expect(checkbox).toBeChecked();
            });
        });

        it('deselect all unchecks all categories', async () => {
            const user = userEvent.setup();
            const allCategoryIds = mockCategories.map(c => c.id);
            render(
                <CategoryPreferencesForm
                    selectedCategories={allCategoryIds}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const deselectAllButton = screen.getByRole('button', { name: /none/i });
            await user.click(deselectAllButton);

            mockCategories.forEach(cat => {
                const checkbox = screen.getByRole('checkbox', { name: new RegExp(cat.name, 'i') });
                expect(checkbox).not.toBeChecked();
            });
        });

        it('save button enables after select all', async () => {
            const user = userEvent.setup();
            render(
                <CategoryPreferencesForm
                    selectedCategories={[]}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeDisabled();

            const selectAllButton = screen.getByRole('button', { name: /all/i });
            await user.click(selectAllButton);

            expect(saveButton).toBeEnabled();
        });

        it('save button enables after deselect all', async () => {
            const user = userEvent.setup();
            render(
                <CategoryPreferencesForm
                    selectedCategories={['culture-arts']}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeDisabled();

            const deselectAllButton = screen.getByRole('button', { name: /none/i });
            await user.click(deselectAllButton);

            expect(saveButton).toBeEnabled();
        });
    });

    describe('Save/Cancel Functionality', () => {
        it('save button is disabled when no changes', () => {
            render(
                <CategoryPreferencesForm
                    selectedCategories={['culture-arts']}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeDisabled();
        });

        it('cancel button only shows when changes exist', async () => {
            const user = userEvent.setup();
            render(
                <CategoryPreferencesForm
                    selectedCategories={['culture-arts']}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            // Initially no cancel button
            expect(screen.queryByRole('button', { name: /cancel/i })).not.toBeInTheDocument();

            // Make a change
            const checkbox = screen.getByRole('checkbox', { name: /history and events/i });
            await user.click(checkbox);

            // Cancel button should appear
            expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
        });

        it('cancel reverts selection to initial state', async () => {
            const user = userEvent.setup();
            render(
                <CategoryPreferencesForm
                    selectedCategories={['culture-arts']}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            // Make changes
            const historyCheckbox = screen.getByRole('checkbox', { name: /history and events/i });
            await user.click(historyCheckbox);

            const scienceCheckbox = screen.getByRole('checkbox', { name: /natural and physical sciences/i });
            await user.click(scienceCheckbox);

            // Click cancel
            const cancelButton = screen.getByRole('button', { name: /cancel/i });
            await user.click(cancelButton);

            // Selection should revert
            expect(screen.getByRole('checkbox', { name: /culture and the arts/i })).toBeChecked();
            expect(historyCheckbox).not.toBeChecked();
            expect(scienceCheckbox).not.toBeChecked();

            // Save button should be disabled again
            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeDisabled();
        });

        it('disables buttons when saving', () => {
            render(
                <CategoryPreferencesForm
                    selectedCategories={['culture-arts']}
                    onSave={mockOnSave}
                    isSaving={true}
                />
            );

            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeDisabled();
        });

        it('hasChanges resets after save', async () => {
            const user = userEvent.setup();
            mockOnSave.mockResolvedValue(undefined);

            render(
                <CategoryPreferencesForm
                    selectedCategories={['culture-arts']}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            // Make a change
            const checkbox = screen.getByRole('checkbox', { name: /history and events/i });
            await user.click(checkbox);

            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeEnabled();

            // Save
            await user.click(saveButton);

            // Wait for save to complete
            await waitFor(() => {
                expect(saveButton).toBeDisabled();
            });
        });
    });

    describe('Edge Cases', () => {
        it('handles empty initial selection', () => {
            render(
                <CategoryPreferencesForm
                    selectedCategories={[]}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            mockCategories.forEach(cat => {
                const checkbox = screen.getByRole('checkbox', { name: new RegExp(cat.name, 'i') });
                expect(checkbox).not.toBeChecked();
            });
        });

        it('handles all categories selected initially', () => {
            const allCategoryIds = mockCategories.map(c => c.id);
            render(
                <CategoryPreferencesForm
                    selectedCategories={allCategoryIds}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            mockCategories.forEach(cat => {
                const checkbox = screen.getByRole('checkbox', { name: new RegExp(cat.name, 'i') });
                expect(checkbox).toBeChecked();
            });
        });

        it('handles single category selection', async () => {
            const user = userEvent.setup();
            render(
                <CategoryPreferencesForm
                    selectedCategories={[]}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const checkbox = screen.getByRole('checkbox', { name: /culture and the arts/i });
            await user.click(checkbox);

            const saveButton = screen.getByRole('button', { name: /save/i });
            await user.click(saveButton);

            expect(mockOnSave).toHaveBeenCalledWith(['culture-arts']);
        });

        it('allows saving with empty selection', async () => {
            const user = userEvent.setup();
            render(
                <CategoryPreferencesForm
                    selectedCategories={['culture-arts']}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const checkbox = screen.getByRole('checkbox', { name: /culture and the arts/i });
            await user.click(checkbox);

            const saveButton = screen.getByRole('button', { name: /save/i });
            await user.click(saveButton);

            expect(mockOnSave).toHaveBeenCalledWith([]);
        });
    });
});
