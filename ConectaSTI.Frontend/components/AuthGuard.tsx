"use client"

import { useEffect, useState } from "react"
import {
  clearBifrostTokenFromCurrentUrl,
  getBifrostTokenFromCurrentUrl,
  redirectToBifrost,
  validateSession,
} from "@/lib/auth"

type Status = "checking" | "authorized"

export default function AuthGuard({ children }: { children: React.ReactNode }) {
  const [status, setStatus] = useState<Status>("checking")

  useEffect(() => {
    let cancelled = false

    async function check() {
      const token = getBifrostTokenFromCurrentUrl()
      const valid = await validateSession(token)

      if (cancelled) return

      if (!valid) {
        redirectToBifrost()
        return
      }

      if (token) {
        clearBifrostTokenFromCurrentUrl()
      }

      setStatus("authorized")
    }

    check()

    return () => {
      cancelled = true
    }
  }, [])

  if (status === "checking") {
    return (
      <div className="flex h-screen w-full items-center justify-center bg-background">
        <div className="text-sm text-muted-foreground">Verificando autenticação...</div>
      </div>
    )
  }

  return <>{children}</>
}
