import { cookies } from "next/headers"
import { type NextRequest, NextResponse } from "next/server"
import { TOKEN_KEY } from "@/lib/auth"
import { getTokenMaxAge, verifyAuthToken } from "@/lib/auth-server"

type ValidateRequestBody = {
  token?: string
}

function shouldUseSecureCookie(request: NextRequest) {
  const configured = process.env.AUTH_COOKIE_SECURE?.trim().toLowerCase()

  if (configured) {
    return configured === "true" || configured === "1" || configured === "yes"
  }

  return request.nextUrl.protocol === "https:"
}

async function getRequestToken(request: NextRequest): Promise<string | null> {
  try {
    const body = (await request.json()) as ValidateRequestBody
    return body.token?.trim() || null
  } catch {
    return null
  }
}

export async function POST(request: NextRequest) {
  try {
    const cookieStore = await cookies()
    const requestToken = await getRequestToken(request)
    const cookieToken = cookieStore.get(TOKEN_KEY)?.value
    const token = requestToken ?? cookieToken

    if (!token) {
      return NextResponse.json({ valid: false, reason: "missing" }, { status: 401 })
    }

    const payload = await verifyAuthToken(token)
    const response = NextResponse.json({ valid: true })

    if (requestToken || !cookieToken) {
      const maxAge = getTokenMaxAge(payload)

      response.cookies.set({
        name: TOKEN_KEY,
        value: token,
        path: "/",
        httpOnly: true,
        sameSite: "lax",
        secure: shouldUseSecureCookie(request),
        ...(maxAge ? { maxAge } : {}),
      })
    }

    return response
  } catch (error) {
    const reason =
      error instanceof Error && error.message === "server_misconfig"
        ? "server_misconfig"
        : error instanceof Error && error.name === "JWTExpired"
          ? "expired"
          : "invalid"
    const status = reason === "server_misconfig" ? 500 : 401

    return NextResponse.json({ valid: false, reason }, { status })
  }
}
