'use client'

import { FediverseLogoIcon } from '@phosphor-icons/react';
import { useState } from 'react'

const options: { id: string; label: string; href: string; icon: React.ComponentType<{ size?: number; className?: string }>; }[] = [
    {
        "id": "Workflows",
        "label": "Workflows",
        "href": "/workflows",
        "icon": FediverseLogoIcon
    },
    {
        "id": "Endpoints",
        "label": "Endpoints",
        "href": "/endpoints",
        "icon": FediverseLogoIcon
    }
]

export default function Sidebar() {
    const [selected, setSelected] = useState<string>('')

    return (
        <div className='flex flex-col items-center bg-red-100 p-5'>
            <div className='text-3xl font-bold mb-5 border-b-2 border-gray-300 pb-2 w-full text-center'>
                ConectaSTI
            </div>
            <div className='flex flex-col gap-4 w-full'>
                {options.map((option) => (
                    <div 
                        key={option.id} 
                        className={`flex items-center gap-3 py-2 px-5 hover:bg-gray-200 ${selected === option.id ? 'bg-gray-200' : ''}`}
                        onClick={() => setSelected(option.id)}
                    >
                        <option.icon />
                        <span>{option.label}</span>
                    </div>
                ))}
            </div>
        </div>
    )
}
