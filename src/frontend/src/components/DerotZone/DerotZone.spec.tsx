import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import DerotZone from './DerotZone'

// Mock the wikipediaApi
vi.mock('@/api/wikipediaApi', () => ({
  wikipediaApi: {
    explore: vi.fn().mockResolvedValue({ id: 'test-explore-id' }),
    addToBacklog: vi.fn().mockResolvedValue({ id: 'test-backlog-id' }),
    read: vi.fn().mockResolvedValue({ activity: { id: 'test-activity-id', userId: 'test-user-id' } })
  }
}))

describe('DerotZone', () => {
  it('renders article card and Read button', async () => {
    render(
      <MemoryRouter>
        <DerotZone
          articles={[{ title: 'Intelligence artificielle', summary: 'Bref résumé', lang: 'fr', sourceUrl: 'https://fr.wikipedia.org/wiki/Intelligence_artificielle' }]}
        />
      </MemoryRouter>
    )

    expect(await screen.findByText('Intelligence artificielle')).toBeInTheDocument()
    expect(await screen.findByRole('button', { name: /Read/i })).toBeInTheDocument()
  })
})
