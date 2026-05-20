import { cookies } from "next/headers"
import { NextResponse } from "next/server"
import { jwtVerify } from "jose"
import { TOKEN_KEY } from "@/lib/auth"

const secret = new TextEncoder().encode(process.env.JWT_SECRET)

export async function POST() {
  try {
    if (!process.env.JWT_SECRET) {
      return NextResponse.json({ valid: false, reason: "server_misconfig" }, { status: 500 })
    }

    const cookieStore = await cookies()
    const token = cookieStore.get(TOKEN_KEY)?.value

    if (!token) {
      return NextResponse.json({ valid: false, reason: "missing" }, { status: 401 })
    }

    await jwtVerify(token, secret)

    return NextResponse.json({ valid: true })
  } catch (error) {
    const reason =
      error instanceof Error && error.name === "JWTExpired" ? "expired" : "invalid"
    return NextResponse.json({ valid: false, reason }, { status: 401 })
  }
}
