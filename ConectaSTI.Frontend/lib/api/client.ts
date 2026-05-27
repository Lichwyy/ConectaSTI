import { mockRequest } from './mock/router'

const BASE_URL = '/api/backend'
const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK === 'true'

export class ApiError extends Error {
  constructor(public errors: string[], public status: number) {
    super(errors.join(', '))
    this.name = 'ApiError'
  }
}

function normalizeResponse<T>(body: unknown): T {
  if (
    body &&
    typeof body === 'object' &&
    'entidade' in body
  ) {
    return (body as { entidade: T }).entidade
  }

  return body as T
}

function normalizeErrors(body: unknown, fallback: string): string[] {
  if (Array.isArray(body)) {
    return body.map(item => {
      if (typeof item === 'string') return item
      if (item && typeof item === 'object' && 'mensagem' in item) {
        return String((item as { mensagem: unknown }).mensagem)
      }
      return JSON.stringify(item)
    })
  }

  if (body && typeof body === 'object') {
    if ('message' in body) return [String((body as { message: unknown }).message)]
    if ('mensagem' in body) return [String((body as { mensagem: unknown }).mensagem)]
    if ('errors' in body) {
      const errors = (body as { errors: unknown }).errors

      if (Array.isArray(errors)) return errors.map(String)
      if (errors && typeof errors === 'object') {
        return Object.values(errors as Record<string, unknown>)
          .flatMap(value => Array.isArray(value) ? value.map(String) : String(value))
      }
    }
  }

  return [fallback]
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  if (USE_MOCK) {
    const body = options?.body ? JSON.parse(options.body as string) : undefined
    return mockRequest<T>(options?.method ?? 'GET', path, body)
  }

  const res = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers: { 'Content-Type': 'application/json', ...options?.headers },
  })
  if (!res.ok) {
    let errors: string[]
    try {
      const body = await res.json()
      errors = normalizeErrors(body, res.statusText)
    } catch {
      errors = [res.statusText || `HTTP ${res.status}`]
    }
    throw new ApiError(errors, res.status)
  }
  const text = await res.text()
  return text ? normalizeResponse<T>(JSON.parse(text)) : (undefined as T)
}

export const client = {
  get:    <T>(path: string)                    => request<T>(path),
  post:   <T>(path: string, body: unknown)     => request<T>(path, { method: 'POST',   body: JSON.stringify(body) }),
  put:    <T>(path: string, body: unknown)     => request<T>(path, { method: 'PUT',    body: JSON.stringify(body) }),
  delete: <T>(path: string)                    => request<T>(path, { method: 'DELETE' }),
}
