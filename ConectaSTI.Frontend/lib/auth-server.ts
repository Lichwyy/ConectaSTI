import { jwtVerify, type JWTPayload } from "jose"
export { getBifrostToken } from "@/lib/auth-token"

export async function verifyAuthToken(token: string): Promise<JWTPayload> {
  const jwtSecret = process.env.JWT_SECRET

  if (!jwtSecret) {
    throw new Error("server_misconfig")
  }

  const { payload } = await jwtVerify(token, new TextEncoder().encode(jwtSecret))

  return payload
}

export function getTokenMaxAge(payload: JWTPayload): number | undefined {
  if (typeof payload.exp !== "number") {
    return undefined
  }

  const secondsUntilExpiration = payload.exp - Math.floor(Date.now() / 1000)

  return secondsUntilExpiration > 0 ? secondsUntilExpiration : undefined
}
