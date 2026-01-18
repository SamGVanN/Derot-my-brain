import { describe, it, expect, vi, beforeEach } from 'vitest';
import { UserService } from '../UserService';

// Hoist mocks to be available in vi.mock factory
const mocks = vi.hoisted(() => ({
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn(),
}));

// Mock axios
vi.mock('axios', () => {
    return {
        default: {
            create: () => ({
                get: mocks.get,
                post: mocks.post,
                put: mocks.put,
                patch: mocks.patch,
            }),
        },
    };
});

describe('UserService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('getAllUsers calls GET /users', async () => {
        const mockUsers = [{ id: '1', name: 'User 1' }];
        mocks.get.mockResolvedValueOnce({ data: mockUsers });

        const result = await UserService.getAllUsers();

        expect(mocks.get).toHaveBeenCalledWith('/users');
        expect(result).toEqual(mockUsers);
    });

    it('getUserById calls GET /users/:id', async () => {
        const mockUser = { id: '1', name: 'User 1' };
        mocks.get.mockResolvedValueOnce({ data: mockUser });

        const result = await UserService.getUserById('1');

        expect(mocks.get).toHaveBeenCalledWith('/users/1');
        expect(result).toEqual(mockUser);
    });

    it('createOrSelectUser calls POST /users', async () => {
        const mockUser = { id: '2', name: 'New User' };
        mocks.post.mockResolvedValueOnce({ data: mockUser });

        const result = await UserService.createOrSelectUser('New User');

        expect(mocks.post).toHaveBeenCalledWith('/users', { name: 'New User' });
        expect(result).toEqual(mockUser);
    });

    it('getHistory calls GET /users/:id/history', async () => {
        const mockHistory = [{ id: 'act-1', activityType: 'read' }];
        mocks.get.mockResolvedValueOnce({ data: mockHistory });

        const result = await UserService.getHistory('1');

        expect(mocks.get).toHaveBeenCalledWith('/users/1/history');
        expect(result).toEqual(mockHistory);
    });

    it('updatePreferences calls PUT /users/:id/preferences', async () => {
        const mockUser = { id: '1', preferences: { questionCount: 20 } };
        mocks.put.mockResolvedValueOnce({ data: mockUser });

        const result = await UserService.updatePreferences('1', { questionCount: 20 });

        expect(mocks.put).toHaveBeenCalledWith('/users/1/preferences', { questionCount: 20 });
        expect(result).toEqual(mockUser);
    });
});
