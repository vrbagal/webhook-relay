import {
  flexRender,
  getCoreRowModel,
  useReactTable,
  type ColumnDef,
} from '@tanstack/react-table'
import { useNavigate } from 'react-router'
import type { WebhookEvent } from '@/api/types'
import { StatusBadge } from './StatusBadge'
import { RelativeTime } from '@/components/shared/RelativeTime'
import { Skeleton } from '@/components/ui/skeleton'

const columns: ColumnDef<WebhookEvent>[] = [
  {
    accessorKey: 'eventType',
    header: 'Event Type',
    cell: ({ row }) => (
      <span className="font-mono text-xs">{row.original.eventType ?? '—'}</span>
    ),
  },
  {
    accessorKey: 'endpointName',
    header: 'Endpoint',
  },
  {
    id: 'status',
    header: 'Status',
    cell: ({ row }) => {
      const latest = row.original.deliveryAttempts.at(-1)
      return latest ? <StatusBadge status={latest.status} /> : <span className="text-muted-foreground text-xs">No attempts</span>
    },
  },
  {
    accessorKey: 'signatureVerified',
    header: 'Verified',
    cell: ({ row }) => (
      <span className={row.original.signatureVerified ? 'text-green-600' : 'text-red-500'}>
        {row.original.signatureVerified ? '✓' : '✗'}
      </span>
    ),
  },
  {
    accessorKey: 'receivedAt',
    header: 'Received',
    cell: ({ row }) => <RelativeTime date={row.original.receivedAt} />,
  },
]

interface EventTableProps {
  events: WebhookEvent[]
  isLoading?: boolean
}

export function EventTable({ events, isLoading }: EventTableProps) {
  const navigate = useNavigate()

  const table = useReactTable({
    data: events,
    columns,
    getCoreRowModel: getCoreRowModel(),
  })

  if (isLoading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    )
  }

  return (
    <div className="rounded-md border overflow-hidden">
      <table className="w-full text-sm">
        <thead className="bg-muted/50">
          {table.getHeaderGroups().map((hg) => (
            <tr key={hg.id}>
              {hg.headers.map((header) => (
                <th key={header.id} className="px-4 py-3 text-left font-medium text-muted-foreground">
                  {flexRender(header.column.columnDef.header, header.getContext())}
                </th>
              ))}
            </tr>
          ))}
        </thead>
        <tbody>
          {table.getRowModel().rows.map((row) => (
            <tr
              key={row.id}
              className="border-t cursor-pointer hover:bg-muted/30 transition-colors"
              onClick={() => void navigate(`/events/${row.original.id}`)}
            >
              {row.getVisibleCells().map((cell) => (
                <td key={cell.id} className="px-4 py-3">
                  {flexRender(cell.column.columnDef.cell, cell.getContext())}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
