import { mockRequest } from './mock/router'

const BASE_URL = `${process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'}/api`
const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK === 'true'

export class ApiError extends Error {
  constructor(public errors: string[], public status: number) {
    super(errors.join(', '))
    this.name = 'ApiError'
  }
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  if (USE_MOCK) {
    const body = options?.body ? JSON.parse(options.body as string) : undefined
    return mockRequest<T>(options?.method ?? 'GET', path, body)
  }

  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  })
  if (!res.ok) {
    let errors: string[]
    try {
      const body = await res.json()
      errors = Array.isArray(body) ? body : [body?.message ?? res.statusText]
    } catch {
      errors = [res.statusText || `HTTP ${res.status}`]
    }
    throw new ApiError(errors, res.status)
  }
  const text = await res.text()
  return text ? (JSON.parse(text) as T) : (undefined as T)
}

export const client = {
  get:    <T>(path: string)                    => request<T>(path),
  post:   <T>(path: string, body: unknown)     => request<T>(path, { method: 'POST',   body: JSON.stringify(body) }),
  put:    <T>(path: string, body: unknown)     => request<T>(path, { method: 'PUT',    body: JSON.stringify(body) }),
  delete: <T>(path: string)                    => request<T>(path, { method: 'DELETE' }),
}
