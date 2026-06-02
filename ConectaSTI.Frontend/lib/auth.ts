import { getBifrostToken, removeBifrostTokenFromUrl } from "@/lib/auth-token"

export const TOKEN_KEY = "token"

export function clearToken(): void {
  if (typeof document === "undefined") return
  document.cookie = `${TOKEN_KEY}=; Path=/; Max-Age=0; SameSite=Lax`
}

export async function logout(): Promise<void> {
  try {
    await fetch("/api/auth/logout", {
      method: "POST",
      credentials: "include",
    })
  } finally {
    clearToken()
    redirectToBifrost()
  }
}

export function redirectToBifrost(): void {
  const bifrost = process.env.NEXT_PUBLIC_BIFROST_URL
  const clientId = process.env.NEXT_PUBLIC_CLIENT_ID
  const redirect = process.env.NEXT_PUBLIC_REDIRECT_URL

  if (!bifrost || !clientId || !redirect) {
    console.error("Bifrost env vars ausentes")
    return
  }

  const responseType = process.env.NEXT_PUBLIC_BIFROST_RESPONSE_TYPE ?? "token"
  const url = `${bifrost}/authorize?client_id=${encodeURIComponent(
    clientId,
  )}&redirect_uri=${encodeURIComponent(redirect)}&response_type=${encodeURIComponent(responseType)}`

  window.location.href = url
}

export function getBifrostTokenFromCurrentUrl(): string | null {
  if (typeof window === "undefined") {
    return null
  }

  return getBifrostToken(new URL(window.location.href).searchParams)
}

export function clearBifrostTokenFromCurrentUrl(): void {
  if (typeof window === "undefined") {
    return
  }

  const url = new URL(window.location.href)
  const sanitizedUrl = removeBifrostTokenFromUrl(url)

  window.history.replaceState(null, "", sanitizedUrl)
}

export async function validateSession(token?: string | null): Promise<boolean> {
  try {
    const res = await fetch("/api/auth/validate", {
      method: "POST",
      credentials: "include",
      headers: token ? { "Content-Type": "application/json" } : undefined,
      body: token ? JSON.stringify({ token }) : undefined,
    })
    const data = (await res.json()) as { valid: boolean }
    return data.valid === true
  } catch {
    return false
  }
}
