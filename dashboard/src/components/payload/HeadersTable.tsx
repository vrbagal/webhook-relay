import { CopyButton } from '@/components/shared/CopyButton'

interface HeadersTableProps {
  headers: Record<string, string>
}

export function HeadersTable({ headers }: HeadersTableProps) {
  const entries = Object.entries(headers)

  if (entries.length === 0) {
    return <p className="text-sm text-muted-foreground">No headers recorded.</p>
  }

  return (
    <div className="rounded-md border overflow-hidden text-sm">
      <table className="w-full">
        <thead className="bg-muted/50">
          <tr>
            <th className="px-4 py-2 text-left font-medium text-muted-foreground w-2/5">Header</th>
            <th className="px-4 py-2 text-left font-medium text-muted-foreground">Value</th>
            <th className="px-2 py-2 w-10" />
          </tr>
        </thead>
        <tbody>
          {entries.map(([key, value]) => (
            <tr key={key} className="border-t">
              <td className="px-4 py-2 font-mono text-xs text-muted-foreground">{key}</td>
              <td className="px-4 py-2 font-mono text-xs truncate max-w-xs">{value}</td>
              <td className="px-2 py-2">
                <CopyButton text={value} className="h-6 w-6" />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
