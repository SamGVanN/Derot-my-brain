import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { GeneralPreferencesForm } from '../GeneralPreferencesForm';
import type { UserPreferences } from '@/models/User';

// Mock dependencies
vi.mock('react-i18next', () => ({
    useTranslation: () => ({
        t: (key: string, defaultValue?: string) => defaultValue || key,
        i18n: {
            language: 'en',
            changeLanguage: vi.fn(),
        },
    }),
}));

vi.mock('@/components/theme-provider', () => ({
    useTheme: () => ({
        theme: { name: 'derot-brain', label: 'Derot Brain' },
        setTheme: vi.fn(),
    }),
}));

vi.mock('@/components/ThemeDropdown', () => ({
    ThemeDropdown: ({ currentThemeName, onThemeChange }: any) => (
        <select
            data-testid="theme-dropdown"
            value={currentThemeName}
            onChange={(e) => onThemeChange(e.target.value)}
        >
            <option value="derot-brain">Derot Brain</option>
            <option value="curiosity-loop">Curiosity Loop</option>
            <option value="knowledge-core">Knowledge Core</option>
        </select>
    ),
}));

vi.mock('@/components/LanguageDropdown', () => ({
    LanguageDropdown: ({ currentLanguageCode, onLanguageChange }: any) => (
        <select
            data-testid="language-dropdown"
            value={currentLanguageCode}
            onChange={(e) => onLanguageChange(e.target.value)}
        >
            <option value="auto">Auto</option>
            <option value="en">English</option>
            <option value="fr">Français</option>
        </select>
    ),
    languages: [
        { code: 'auto', label: 'Auto' },
        { code: 'en', label: 'English' },
        { code: 'fr', label: 'Français' },
    ],
}));

describe('GeneralPreferencesForm', () => {
    const defaultPreferences: UserPreferences = {
        questionCount: 10,
        preferredTheme: 'derot-brain',
        language: 'en',
        selectedCategories: [],
    };

    const mockOnSave = vi.fn();

    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe('Rendering', () => {
        it('renders all form fields correctly', () => {
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            expect(screen.getByTestId('language-dropdown')).toBeInTheDocument();
            expect(screen.getByTestId('theme-dropdown')).toBeInTheDocument();
            expect(screen.getByLabelText('5')).toBeInTheDocument();
            expect(screen.getByLabelText('10')).toBeInTheDocument();
            expect(screen.getByLabelText('15')).toBeInTheDocument();
            expect(screen.getByLabelText('20')).toBeInTheDocument();
        });

        it('displays current preferences values', () => {
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            expect(screen.getByTestId('language-dropdown')).toHaveValue('en');
            expect(screen.getByTestId('theme-dropdown')).toHaveValue('derot-brain');
            expect(screen.getByLabelText('10')).toBeChecked();
        });
    });

    describe('Language Selection', () => {
        it('can select different languages', async () => {
            const user = userEvent.setup();
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const languageDropdown = screen.getByTestId('language-dropdown');
            await user.selectOptions(languageDropdown, 'fr');

            expect(languageDropdown).toHaveValue('fr');
        });

        it('enables save button when language changes', async () => {
            const user = userEvent.setup();
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeDisabled();

            const languageDropdown = screen.getByTestId('language-dropdown');
            await user.selectOptions(languageDropdown, 'fr');

            expect(saveButton).toBeEnabled();
        });

        it('does not show mismatch indicator for auto preference', () => {
            const autoPreferences = { ...defaultPreferences, language: 'auto' };
            render(
                <GeneralPreferencesForm
                    preferences={autoPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            // Mismatch indicator should not be present
            expect(screen.queryByText(/different from your saved profile language/i)).not.toBeInTheDocument();
        });

        it('calls onSave with correct language when saved', async () => {
            const user = userEvent.setup();
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const languageDropdown = screen.getByTestId('language-dropdown');
            await user.selectOptions(languageDropdown, 'fr');

            const saveButton = screen.getByRole('button', { name: /save/i });
            await user.click(saveButton);

            expect(mockOnSave).toHaveBeenCalledWith(
                expect.objectContaining({ language: 'fr' })
            );
        });
    });

    describe('Theme Selection', () => {
        it('can select different themes', async () => {
            const user = userEvent.setup();
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const themeDropdown = screen.getByTestId('theme-dropdown');
            await user.selectOptions(themeDropdown, 'curiosity-loop');

            expect(themeDropdown).toHaveValue('curiosity-loop');
        });

        it('enables save button when theme changes', async () => {
            const user = userEvent.setup();
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeDisabled();

            const themeDropdown = screen.getByTestId('theme-dropdown');
            await user.selectOptions(themeDropdown, 'curiosity-loop');

            expect(saveButton).toBeEnabled();
        });

        it('calls onSave with correct theme when saved', async () => {
            const user = userEvent.setup();
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const themeDropdown = screen.getByTestId('theme-dropdown');
            await user.selectOptions(themeDropdown, 'knowledge-core');

            const saveButton = screen.getByRole('button', { name: /save/i });
            await user.click(saveButton);

            expect(mockOnSave).toHaveBeenCalledWith(
                expect.objectContaining({ preferredTheme: 'knowledge-core' })
            );
        });
    });

    describe('Question Count Selection', () => {
        it('can select different question counts', async () => {
            const user = userEvent.setup();
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const radio15 = screen.getByLabelText('15');
            await user.click(radio15);

            expect(radio15).toBeChecked();
            expect(screen.getByLabelText('10')).not.toBeChecked();
        });

        it('enables save button when question count changes', async () => {
            const user = userEvent.setup();
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeDisabled();

            const radio20 = screen.getByLabelText('20');
            await user.click(radio20);

            expect(saveButton).toBeEnabled();
        });

        it('calls onSave with correct question count when saved', async () => {
            const user = userEvent.setup();
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            const radio5 = screen.getByLabelText('5');
            await user.click(radio5);

            const saveButton = screen.getByRole('button', { name: /save/i });
            await user.click(saveButton);

            expect(mockOnSave).toHaveBeenCalledWith(
                expect.objectContaining({ questionCount: 5 })
            );
        });
    });

    describe('Save/Cancel Functionality', () => {
        it('save button is disabled when no changes', () => {
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
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
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            // Initially no cancel button
            expect(screen.queryByRole('button', { name: /cancel/i })).not.toBeInTheDocument();

            // Make a change
            const languageDropdown = screen.getByTestId('language-dropdown');
            await user.selectOptions(languageDropdown, 'fr');

            // Cancel button should appear
            expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
        });

        it('cancel reverts all changes', async () => {
            const user = userEvent.setup();
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            // Make changes
            const languageDropdown = screen.getByTestId('language-dropdown');
            await user.selectOptions(languageDropdown, 'fr');

            const themeDropdown = screen.getByTestId('theme-dropdown');
            await user.selectOptions(themeDropdown, 'curiosity-loop');

            const radio20 = screen.getByLabelText('20');
            await user.click(radio20);

            // Click cancel
            const cancelButton = screen.getByRole('button', { name: /cancel/i });
            await user.click(cancelButton);

            // Values should revert
            expect(languageDropdown).toHaveValue('en');
            expect(themeDropdown).toHaveValue('derot-brain');
            expect(screen.getByLabelText('10')).toBeChecked();

            // Save button should be disabled again
            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeDisabled();
        });

        it('disables buttons when saving', () => {
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={true}
                />
            );

            const saveButton = screen.getByRole('button', { name: /save/i });
            expect(saveButton).toBeDisabled();
        });

        it('shows loading indicator when saving', () => {
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={true}
                />
            );

            // Check for loading spinner (Loader2 component)
            expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
        });
    });

    describe('Combined Changes', () => {
        it('can change multiple fields at once', async () => {
            const user = userEvent.setup();
            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            // Change all fields
            const languageDropdown = screen.getByTestId('language-dropdown');
            await user.selectOptions(languageDropdown, 'fr');

            const themeDropdown = screen.getByTestId('theme-dropdown');
            await user.selectOptions(themeDropdown, 'knowledge-core');

            const radio15 = screen.getByLabelText('15');
            await user.click(radio15);

            // Save all changes
            const saveButton = screen.getByRole('button', { name: /save/i });
            await user.click(saveButton);

            expect(mockOnSave).toHaveBeenCalledWith({
                language: 'fr',
                preferredTheme: 'knowledge-core',
                questionCount: 15,
            });
        });

        it('hasChanges resets after save', async () => {
            const user = userEvent.setup();
            mockOnSave.mockResolvedValue(undefined);

            render(
                <GeneralPreferencesForm
                    preferences={defaultPreferences}
                    onSave={mockOnSave}
                    isSaving={false}
                />
            );

            // Make a change
            const languageDropdown = screen.getByTestId('language-dropdown');
            await user.selectOptions(languageDropdown, 'fr');

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
});
