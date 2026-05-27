import { cookies } from "next/headers"
import { type NextRequest, NextResponse } from "next/server"
import { TOKEN_KEY } from "@/lib/auth"

const BACKEND_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5055"
const HOP_BY_HOP_HEADERS = new Set([
  "connection",
  "content-length",
  "host",
  "keep-alive",
  "proxy-authenticate",
  "proxy-authorization",
  "te",
  "trailer",
  "transfer-encoding",
  "upgrade",
])

type RouteContext = {
  params: Promise<{ path?: string[] }>
}

function backendUrl(path: string[], request: NextRequest) {
  const backendPath = path[0] === "_root"
    ? `/${path.slice(1).join("/")}`
    : `/api/${path.join("/")}`
  const url = new URL(backendPath, BACKEND_URL)
  request.nextUrl.searchParams.forEach((value, key) => {
    url.searchParams.append(key, value)
  })

  return url
}

function forwardHeaders(request: NextRequest, token?: string) {
  const headers = new Headers()

  request.headers.forEach((value, key) => {
    if (!HOP_BY_HOP_HEADERS.has(key.toLowerCase()) && key.toLowerCase() !== "cookie") {
      headers.set(key, value)
    }
  })

  if (token) {
    headers.set("Authorization", `Bearer ${token}`)
  }

  return headers
}

function responseHeaders(headers: Headers) {
  const forwarded = new Headers()

  headers.forEach((value, key) => {
    if (!HOP_BY_HOP_HEADERS.has(key.toLowerCase())) {
      forwarded.set(key, value)
    }
  })

  return forwarded
}

async function proxy(request: NextRequest, context: RouteContext) {
  const { path = [] } = await context.params
  const cookieStore = await cookies()
  const token = cookieStore.get(TOKEN_KEY)?.value
  const method = request.method.toUpperCase()
  const hasBody = method !== "GET" && method !== "HEAD"

  try {
    const upstream = await fetch(backendUrl(path, request), {
      method,
      headers: forwardHeaders(request, token),
      body: hasBody ? await request.text() : undefined,
      cache: "no-store",
    })

    return new NextResponse(upstream.body, {
      status: upstream.status,
      statusText: upstream.statusText,
      headers: responseHeaders(upstream.headers),
    })
  } catch {
    return NextResponse.json(
      { message: "Não foi possível conectar ao backend ConectaSTI." },
      { status: 502 },
    )
  }
}

export const GET = proxy
export const POST = proxy
export const PUT = proxy
export const PATCH = proxy
export const DELETE = proxy
