import { type NextRequest, NextResponse } from "next/server"
import { TOKEN_KEY } from "@/lib/auth"
import { getBifrostToken, getTokenMaxAge, verifyAuthToken } from "@/lib/auth-server"

const DEFAULT_REDIRECT_PATH = "/"

function redirectToApp(request: NextRequest) {
  return new URL(DEFAULT_REDIRECT_PATH, request.url)
}

function shouldUseSecureCookie(request: NextRequest) {
  const configured = process.env.AUTH_COOKIE_SECURE?.trim().toLowerCase()

  if (configured) {
    return configured === "true" || configured === "1" || configured === "yes"
  }

  return request.nextUrl.protocol === "https:"
}

export async function handleBifrostCallback(request: NextRequest) {
  const token = getBifrostToken(request.nextUrl.searchParams)
  const redirectUrl = redirectToApp(request)

  if (!token) {
    const response = NextResponse.redirect(redirectUrl)
    response.cookies.delete(TOKEN_KEY)
    return response
  }

  try {
    const payload = await verifyAuthToken(token)
    const maxAge = getTokenMaxAge(payload)
    const response = NextResponse.redirect(redirectUrl)

    response.cookies.set({
      name: TOKEN_KEY,
      value: token,
      path: "/",
      httpOnly: true,
      sameSite: "lax",
      secure: shouldUseSecureCookie(request),
      ...(maxAge ? { maxAge } : {}),
    })

    return response
  } catch {
    const response = NextResponse.redirect(redirectUrl)
    response.cookies.delete(TOKEN_KEY)
    return response
  }
}
