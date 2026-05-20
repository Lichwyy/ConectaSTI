"use client"

import { useEffect, useState } from "react"
import { redirectToBifrost, validateSession } from "@/lib/auth"

type Status = "checking" | "authorized"

export default function AuthGuard({ children }: { children: React.ReactNode }) {
  const [status, setStatus] = useState<Status>("checking")

  useEffect(() => {
    let cancelled = false

    async function check() {
      const valid = await validateSession()

      if (cancelled) return

      if (!valid) {
        redirectToBifrost()
        return
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
