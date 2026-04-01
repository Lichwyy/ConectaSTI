import React from 'react'

import { motion } from "motion/react"
import { FediverseLogoIcon } from '@phosphor-icons/react';

import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"

const options: {id: string; label: string; description: string; icon: React.ComponentType<{ size?: number; className?: string }>}[] = [
    {
        id: "1",
        label: "Funcionalidade 1",
        description: "Descrição da funcionalidade 1",
        icon: FediverseLogoIcon
    },
    {
        id: "2",
        label: "Funcionalidade 2",
        description: "Descrição da funcionalidade 2",
        icon: FediverseLogoIcon
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
