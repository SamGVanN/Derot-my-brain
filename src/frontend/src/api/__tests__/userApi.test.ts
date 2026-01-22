import { describe, it, expect, vi, beforeEach } from 'vitest';
import { userApi } from '../userApi';

// Mock the client
const mocks = vi.hoisted(() => ({
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn(),
}));

vi.mock('../client', () => ({
    client: {
        get: mocks.get,
        post: mocks.post,
        put: mocks.put,
        patch: mocks.patch,
    },
}));

describe('userApi', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('getAllUsers calls GET /users', async () => {
        const mockUsers = [{ id: '1', name: 'User 1' }];
        mocks.get.mockResolvedValueOnce({ data: mockUsers });

        const result = await userApi.getAllUsers();

        expect(mocks.get).toHaveBeenCalledWith('/users');
        expect(result).toEqual(mockUsers);
    });

    it('getUserById calls GET /users/:id', async () => {
        const mockUser = { id: '1', name: 'User 1', preferences: {} };
        mocks.get.mockResolvedValueOnce({ data: mockUser });

        const result = await userApi.getUserById('1');

        expect(mocks.get).toHaveBeenCalledWith('/users/1');
        expect(result).toEqual(mockUser);
    });

    it('createOrSelectUser calls POST /users', async () => {
        const mockUser = { id: '2', name: 'New User', preferences: {} };
        mocks.post.mockResolvedValueOnce({ data: mockUser });

        const result = await userApi.createOrSelectUser('New User');

        expect(mocks.post).toHaveBeenCalledWith('/users', { name: 'New User' });
        expect(result).toEqual(mockUser);
    });



    it('updatePreferences calls PUT /users/:id/preferences', async () => {
        const mockUser = { id: '1', preferences: { questionCount: 20 } };
        mocks.put.mockResolvedValueOnce({ data: mockUser });

        const result = await userApi.updatePreferences('1', { questionCount: 20 });

        expect(mocks.put).toHaveBeenCalledWith('/users/1/preferences', { questionCount: 20 });
        expect(result).toEqual(mockUser);
    });

    it('updateGeneralPreferences calls PATCH /users/:id/preferences/general', async () => {
        const mockUser = { id: '1', preferences: { questionCount: 20 } };
        mocks.patch.mockResolvedValueOnce({ data: mockUser });

        const prefs = { language: 'fr', preferredTheme: 'dark', questionCount: 20 };
        const result = await userApi.updateGeneralPreferences('1', prefs);

        expect(mocks.patch).toHaveBeenCalledWith('/users/1/preferences/general', prefs);
        expect(result).toEqual(mockUser);
    });

    it('updateCategoryPreferences calls PATCH /users/:id/preferences/categories', async () => {
        const mockUser = { id: '1', preferences: { selectedCategories: ['history'] } };
        mocks.patch.mockResolvedValueOnce({ data: mockUser });

        const result = await userApi.updateCategoryPreferences('1', ['history']);

        expect(mocks.patch).toHaveBeenCalledWith('/users/1/preferences/categories', { selectedCategories: ['history'] });
        expect(result).toEqual(mockUser);
    });
});
