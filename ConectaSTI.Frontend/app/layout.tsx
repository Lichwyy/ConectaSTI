import { Geist, JetBrains_Mono } from "next/font/google"

import "./globals.css"
import { ThemeProvider } from "@/components/theme-provider"
import { cn } from "@/lib/utils";
import Sidebar from "@/components/Sidebar";

const fontSans = Geist({
  subsets: ["latin"],
  variable: "--font-sans",
})

const jetbrainsMono = JetBrains_Mono({subsets:['latin'],variable:'--font-mono'})

const gridBackground = "absolute -z-10 inset-0 h-full w-full bg-[radial-gradient(circle,#73737350_1px,transparent_1px)] bg-[size:10px_10px]"

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <html
      lang="pt-br"
      suppressHydrationWarning
      className={cn("antialiased", fontSans.variable, "font-mono", jetbrainsMono.variable)}
    >
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <body className={"h-screen overflow-hidden flex " + gridBackground}>
        <ThemeProvider
          attribute="class"
          defaultTheme="light"
          disableTransitionOnChange
        >
          <Sidebar />
          {children}
        </ThemeProvider>
      </body>
    </html>
  )
}
