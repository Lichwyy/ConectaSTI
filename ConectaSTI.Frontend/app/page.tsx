'use client'

import QuickOptions from "@/components/QuickOptions";

export default function Page() {
  return (
    <div className="flex h-screen w-full items-center justify-center">
      <div className="flex flex-col items-center">
        <h1 className="text-5xl font-bold">
          Seja Bem Vindo!
        </h1>
        <span className="my-3">
          Selecione uma das opções abaixo para começar a usar o ConectaSTI
        </span>

        {/* Cards rápidos */}
        <QuickOptions />
      </div>
    </div>
  )
}
