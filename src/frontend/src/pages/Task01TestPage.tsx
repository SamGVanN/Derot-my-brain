import { useCategories } from '../hooks/useCategories';
import { useThemes } from '../hooks/useThemes';
import { useAppConfig } from '../hooks/useAppConfig';
import { Card } from '../components/ui/card';

/**
 * Test page to verify Task 0.1 implementation
 * Displays categories, themes, and configuration from the API
 */
export function Task01TestPage() {
    const { categories, loading: categoriesLoading, error: categoriesError } = useCategories();
    const { themes, loading: themesLoading, error: themesError } = useThemes();
    const { config, loading: configLoading, error: configError } = useAppConfig();

    return (
        <div className="container mx-auto p-6 space-y-6">
            <h1 className="text-3xl font-bold">Task 0.1 - Initialization Test</h1>

            {/* Categories Section */}
            <Card className="p-6">
                <h2 className="text-2xl font-semibold mb-4">Wikipedia Categories (13)</h2>
                {categoriesLoading && <p>Loading categories...</p>}
                {categoriesError && <p className="text-red-500">Error: {categoriesError}</p>}
                {!categoriesLoading && !categoriesError && (
                    <div className="grid grid-cols-2 gap-4">
                        {categories.map((cat) => (
                            <div key={cat.id} className="border p-3 rounded">
                                <p className="font-medium">{cat.name}</p>
                                <p className="text-sm text-gray-500">{cat.nameFr}</p>
                                <p className="text-xs text-gray-400">Order: {cat.order}</p>
                            </div>
                        ))}
                    </div>
                )}
                <p className="mt-4 text-sm text-gray-600">Total: {categories.length} categories</p>
            </Card>

            {/* Themes Section */}
            <Card className="p-6">
                <h2 className="text-2xl font-semibold mb-4">UI Themes (5)</h2>
                {themesLoading && <p>Loading themes...</p>}
                {themesError && <p className="text-red-500">Error: {themesError}</p>}
                {!themesLoading && !themesError && (
                    <div className="grid grid-cols-1 gap-4">
                        {themes.map((theme) => (
                            <div key={theme.id} className="border p-3 rounded">
                                <p className="font-medium">{theme.name} {theme.isDefault && '(Default)'}</p>
                                <p className="text-sm text-gray-500">{theme.description}</p>
                            </div>
                        ))}
                    </div>
                )}
                <p className="mt-4 text-sm text-gray-600">Total: {themes.length} themes</p>
            </Card>

            {/* Configuration Section */}
            <Card className="p-6">
                <h2 className="text-2xl font-semibold mb-4">Global Configuration</h2>
                {configLoading && <p>Loading configuration...</p>}
                {configError && <p className="text-red-500">Error: {configError}</p>}
                {!configLoading && !configError && config && (
                    <div className="space-y-2">
                        <p><strong>Config ID:</strong> {config.id}</p>
                        <p><strong>Last Updated:</strong> {new Date(config.lastUpdated).toLocaleString()}</p>
                        <div className="mt-4 border-t pt-4">
                            <h3 className="font-semibold mb-2">LLM Configuration:</h3>
                            <p><strong>URL:</strong> {config.llm.url}</p>
                            <p><strong>Port:</strong> {config.llm.port}</p>
                            <p><strong>Provider:</strong> {config.llm.provider}</p>
                            <p><strong>Default Model:</strong> {config.llm.defaultModel}</p>
                            <p><strong>Timeout:</strong> {config.llm.timeoutSeconds}s</p>
                        </div>
                    </div>
                )}
            </Card>
        </div>
    );
}
