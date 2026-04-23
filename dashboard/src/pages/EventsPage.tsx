import { useState } from 'react'
import { useEvents } from '@/hooks/useEvents'
import { EventTable } from '@/components/events/EventTable'
import { EmptyState } from '@/components/shared/EmptyState'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import type { DeliveryStatus, EventQueryParams } from '@/api/types'
import { ChevronLeft, ChevronRight } from 'lucide-react'

const PAGE_SIZE = 20

export function EventsPage() {
  const [page, setPage] = useState(1)
  const [eventType, setEventType] = useState('')
  const [status, setStatus] = useState<DeliveryStatus | ''>('')

  const params: EventQueryParams = {
    page,
    pageSize: PAGE_SIZE,
    eventType: eventType || undefined,
    status: status || undefined,
  }

  const { data, isLoading } = useEvents(params)
  const events = data?.items ?? []
  const totalPages = data ? Math.ceil(data.totalCount / PAGE_SIZE) : 1

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Events</h1>

      <div className="flex gap-3">
        <Input
          placeholder="Filter by event type..."
          value={eventType}
          onChange={(e) => { setEventType(e.target.value); setPage(1) }}
          className="max-w-xs"
        />
        <Select value={status} onValueChange={(v) => { setStatus(v as DeliveryStatus | ''); setPage(1) }}>
          <SelectTrigger className="w-40">
            <SelectValue placeholder="All statuses" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value=" ">All</SelectItem>
            <SelectItem value="Pending">Pending</SelectItem>
            <SelectItem value="Delivered">Delivered</SelectItem>
            <SelectItem value="Failed">Failed</SelectItem>
            <SelectItem value="DeadLettered">Dead Letter</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {!isLoading && events.length === 0
        ? <EmptyState title="No events" description="Webhook events will appear here once received." />
        : <EventTable events={events} isLoading={isLoading} />
      }

      {totalPages > 1 && (
        <div className="flex items-center justify-end gap-2 pt-2">
          <Button variant="outline" size="icon" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="text-sm text-muted-foreground">Page {page} of {totalPages}</span>
          <Button variant="outline" size="icon" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      )}
    </div>
  )
}
