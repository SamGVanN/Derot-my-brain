/* @vitest-environment node */
import { spawn, ChildProcess } from 'child_process'
import fetch from 'node-fetch'
import { describe, it, expect } from 'vitest'

import { execSync } from 'child_process'

function killExistingApi() {
  try {
    // Relative path to script from src/frontend
    execSync('powershell.exe -ExecutionPolicy Bypass -File ../../../scripts/kill-derot-api.ps1', { stdio: 'inherit' })
  } catch (e) {
    console.error('Failed to kill existing API', e)
  }
}

function startApi(): Promise<{ proc?: ChildProcess; url: string }> {
  return new Promise(async (resolve, reject) => {
    // If a dev server is already running on a common port, reuse it
    const candidates = ['http://127.0.0.1:5077', 'http://localhost:5077', 'http://localhost:5005', 'http://127.0.0.1:5005']
    for (const candidate of candidates) {
      try {
        const r = await fetch(candidate + '/api/users')
        if (r.ok) {
          console.log(`Reusing existing API at ${candidate}`)
          return resolve({ url: candidate })
        }
      } catch (e) {
        // ignore
      }
    }

    // No existing API found, kill any rogue processes and start a new one
    console.log('No existing API found. Cleaning up and starting a new one...')
    killExistingApi()

    const url = 'http://127.0.0.1:5005'
    console.log(`Starting new API instance at ${url}`)
    const env = { ...process.env, ASPNETCORE_URLS: url, ASPNETCORE_ENVIRONMENT: 'Development' }
    // Use relative path to project from src/frontend (where vitest usually runs)
    const projectPath = '../backend/DerotMyBrain.API/DerotMyBrain.API.csproj'
    const proc = spawn('dotnet', ['run', '--project', projectPath, '--no-build'], { env, shell: true })

    proc.stdout?.on('data', (d) => process.stdout.write('[API] ' + d.toString()))
    proc.stderr?.on('data', (d) => process.stderr.write('[API-ERR] ' + d.toString()))

    const start = Date.now()
    const check = async () => {
      try {
        const res = await fetch(url + '/api/users')
        if (res.ok) {
          console.log('API is ready')
          return resolve({ proc, url })
        }
      } catch (e: any) { }
      if (Date.now() - start > 120000) return reject(new Error('API did not start in time'))
      setTimeout(check, 1000)
    }
    check()
  })
}

async function stopApi(proc?: ChildProcess) {
  if (!proc) return
  console.log('Stopping API...')
  return new Promise<void>((resolve) => {
    proc.kill('SIGTERM')
    // On Windows, kill() might not be enough for dotnet run because it spawns a child.
    // So we use our script as a backup.
    setTimeout(() => {
      killExistingApi()
      resolve()
    }, 2000)
    proc.on('exit', () => resolve())
  })
}

describe('DerotZone E2E against real API', () => {
  it('exercise explore -> backlog -> read flow end-to-end', async () => {
    const { proc, url } = await startApi()

    try {
      // 1. POST login/create user to get token
      const logRes = await fetch(url + '/api/users', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: 'E2ETester' })
      })
      expect(logRes.ok).toBe(true)
      const logJson: any = await logRes.json()
      const token = logJson.token
      const userId = logJson.user.id
      const authHeader = { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' }

      // 2. POST explore
      const ex = await fetch(url + '/api/wikipedia/explore', {
        method: 'POST',
        headers: authHeader,
        body: JSON.stringify({})
      })
      if (!ex.ok) {
        const t = await ex.text()
        console.error('Explore failed', ex.status, t)
      }
      expect(ex.ok).toBe(true)
      const exJson: any = await ex.json()
      expect(exJson.id).toBeTruthy()
      const exploreId = exJson.id

      // 3. POST backlog
      const article = { title: 'Intelligence artificielle', lang: 'fr', sourceUrl: 'https://fr.wikipedia.org/wiki/Intelligence_artificielle', summary: 'test' }
      const bl = await fetch(url + '/api/backlog', {
        method: 'POST',
        headers: authHeader,
        body: JSON.stringify(article)
      })
      expect(bl.ok).toBe(true)
      const blJson: any = await bl.json()
      expect(blJson.id || blJson).toBeTruthy()

      // 4. POST read with originExploreId
      const readReq = { title: article.title, language: article.lang, sourceUrl: article.sourceUrl, originExploreId: exploreId, backlogAddsCount: 1 }
      const rd = await fetch(url + '/api/wikipedia/read', {
        method: 'POST',
        headers: authHeader,
        body: JSON.stringify(readReq)
      })
      expect(rd.ok).toBe(true)
      const rdJson: any = await rd.json()
      expect(rdJson.activity).toBeTruthy()
      expect(rdJson.activity.id).toBeDefined()

      // 5. GET explore activity to verify linkage
      const getEx = await fetch(url + `/api/users/${userId}/activities/${exploreId}`, {
        headers: authHeader
      })
      expect(getEx.ok).toBe(true)
      const getExJson: any = await getEx.json()
      expect(getExJson.resultingReadActivityId).toBe(rdJson.activity.id)
    } finally {
      await stopApi(proc)
    }
  }, 180000)
})

