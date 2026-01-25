import React from 'react'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import DerotZone from './DerotZone'

const article = { title: 'AI', summary: 'Short', lang: 'en', sourceUrl: 'https://en.wikipedia.org/wiki/AI' }

describe('DerotZone API flow', () => {
  beforeEach(() => {
    ;(global as any).fetch = vi.fn()
  })

  afterEach(() => {
    vi.clearAllMocks()
  })

  it('creates Explore on mount, Add to Backlog calls backlog endpoint and Read posts Read with OriginExploreId', async () => {
    // use userEvent directly for compatibility with installed version

    // Mock responses: first call -> explore created, second call -> backlog created, third call -> read created
    (global as any).fetch
      .mockResolvedValueOnce({ ok: true, json: async () => ({ id: 'explore-1' }) })
      .mockResolvedValueOnce({ ok: true, json: async () => ({ id: 'backlog-1' }) })
      .mockResolvedValueOnce({ ok: true, json: async () => ({ id: 'read-1', type: 'Read' }) })

    render(<DerotZone articles={[article]} userId="test-user" />)

    // Explore POST should have been called on mount
    expect((global as any).fetch).toHaveBeenCalledWith('/api/wikipedia/explore', expect.any(Object))

    // Click Add to Backlog
    const addBtn = screen.getByRole('button', { name: /Add to Backlog/i })
    await userEvent.click(addBtn)

    expect((global as any).fetch).toHaveBeenCalledWith('/api/backlog', expect.objectContaining({ method: 'POST' }))

    // Click Read
    const readBtn = screen.getByRole('button', { name: /Read/i })
    await userEvent.click(readBtn)

    // Read POST should include originExploreId and backlogAddsCount in body
    const lastCall = (global as any).fetch.mock.calls.slice(-1)[0]
    expect(lastCall[0]).toBe('/api/wikipedia/read')
    const body = JSON.parse(lastCall[1].body)
    expect(body.originExploreId).toBe('explore-1')
    expect(body.backlogAddsCount).toBe(1)
  })
})
