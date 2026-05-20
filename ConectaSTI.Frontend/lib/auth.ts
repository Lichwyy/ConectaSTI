export const TOKEN_KEY = "token"

export function clearToken(): void {
  if (typeof document === "undefined") return
  document.cookie = `${TOKEN_KEY}=; Path=/; Max-Age=0; SameSite=Lax`
}

export function redirectToBifrost(): void {
  const bifrost = process.env.NEXT_PUBLIC_BIFROST_URL
  const clientId = process.env.NEXT_PUBLIC_CLIENT_ID
  const redirect = process.env.NEXT_PUBLIC_REDIRECT_URL

  if (!bifrost || !clientId || !redirect) {
    console.error("Bifrost env vars ausentes")
    return
  }

  const url = `${bifrost}/authorize?client_id=${encodeURIComponent(
    clientId,
  )}&redirect_url=${encodeURIComponent(redirect)}&response_type=code`

  window.location.href = url
}

export async function validateSession(): Promise<boolean> {
  try {
    const res = await fetch("/api/auth/validate", {
      method: "POST",
      credentials: "include",
    })
    const data = (await res.json()) as { valid: boolean }
    return data.valid === true
  } catch {
    return false
  }
}
