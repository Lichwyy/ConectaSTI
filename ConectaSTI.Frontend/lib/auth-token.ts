export const BIFROST_TOKEN_QUERY_KEYS = [
  "token",
  "access_token",
  "accessToken",
  "session_token",
  "jwt",
] as const

export function getBifrostToken(searchParams: URLSearchParams): string | null {
  for (const key of BIFROST_TOKEN_QUERY_KEYS) {
    const token = searchParams.get(key)?.trim()

    if (token) {
      return token
    }
  }

  return null
}

export function removeBifrostTokenFromUrl(url: URL): string {
  for (const key of BIFROST_TOKEN_QUERY_KEYS) {
    url.searchParams.delete(key)
  }

  return `${url.pathname}${url.search}${url.hash}`
}
