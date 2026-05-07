import React from 'react'

import { motion } from "motion/react"
import { FlowArrowIcon, EnvelopeSimpleIcon } from '@phosphor-icons/react';

import {
    Card,
    CardContent,

    CardHeader,
    CardTitle,
} from "@/components/ui/card"

const options: {id: string; label: string; description: string; icon: React.ComponentType<{ size?: number; className?: string }>}[] = [
    {
        id: "1",
        label: "Cadastrar uma Api",
        description: "Cadastre uma nova Api para usar no ConectaSTI",
        icon: EnvelopeSimpleIcon
    },
    {
        id: "2",
        label: "Criar um Workflow",
        description: "Crie um novo Workflow para usar no ConectaSTI",
        icon: FlowArrowIcon
    }
]

export default function QuickOptions() {
    return (
        <div className="flex md:px-5 justify-center gap-7 flex-wrap lg:flex-nowrap cursor-pointer">
        {options.map((option, index) => (
          <motion.div
            key={option.id}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: index * 0.1 }}
            whileHover={{ scale: 1.07, transition: { duration: 0.3 } }}
          >
            <Card className="min-w-[350px] rounded-xl text-center px-7">
              <CardHeader>
                <CardTitle className="text-xl w-full">{option.label}</CardTitle>
              </CardHeader>
              <CardContent className="flex flex-col items-center justify-center gap-3">
                <option.icon size={48} />
                <p className="text-muted-foreground text-sm">{option.description}</p>
              </CardContent>
            </Card>
          </motion.div>
        ))}
      </div>
    )
}
