import { useEffect, useState } from 'react'
import { codeToHtml } from 'shiki'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs'
import { CopyButton } from '@/components/shared/CopyButton'

interface PayloadViewerProps {
  payload: string
}

export function PayloadViewer({ payload }: PayloadViewerProps) {
  const [highlighted, setHighlighted] = useState('')

  const formatted = (() => {
    try { return JSON.stringify(JSON.parse(payload), null, 2) }
    catch { return payload }
  })()

  useEffect(() => {
    codeToHtml(formatted, { lang: 'json', theme: 'github-dark' })
      .then(setHighlighted)
      .catch(() => setHighlighted(`<pre>${formatted}</pre>`))
  }, [formatted])

  return (
    <div className="relative rounded-md border bg-[#0d1117] text-sm">
      <div className="absolute right-3 top-3 z-10">
        <CopyButton text={formatted} />
      </div>
      <Tabs defaultValue="highlighted">
        <TabsList className="m-3 mb-0">
          <TabsTrigger value="highlighted">Formatted</TabsTrigger>
          <TabsTrigger value="raw">Raw</TabsTrigger>
        </TabsList>
        <TabsContent value="highlighted">
          <div
            className="overflow-auto p-4 font-mono text-xs"
            dangerouslySetInnerHTML={{ __html: highlighted }}
          />
        </TabsContent>
        <TabsContent value="raw">
          <pre className="overflow-auto p-4 font-mono text-xs text-gray-300">
            {payload}
          </pre>
        </TabsContent>
      </Tabs>
    </div>
  )
}
