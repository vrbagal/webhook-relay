import { useTransition } from 'react'
import { useParams, Link } from 'react-router'
import { toast } from 'sonner'
import { useEvent, useReplayEvent } from '@/hooks/useEvents'
import { PayloadViewer } from '@/components/payload/PayloadViewer'
import { HeadersTable } from '@/components/payload/HeadersTable'
import { DeliveryTimeline } from '@/components/delivery/DeliveryTimeline'
import { StatusBadge } from '@/components/events/StatusBadge'
import { RelativeTime } from '@/components/shared/RelativeTime'
import { Button } from '@/components/ui/button'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Skeleton } from '@/components/ui/skeleton'
import { ArrowLeft, RefreshCw } from 'lucide-react'

export function EventDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: event, isLoading } = useEvent(id!)
  const replayMutation = useReplayEvent()
  const [isPending, startTransition] = useTransition()

  const handleReplay = () => {
    startTransition(async () => {
      try {
        await replayMutation.mutateAsync({ id: id! })
        toast.success('Event replayed successfully')
      } catch {
        toast.error('Failed to replay event')
      }
    })
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-48 w-full" />
      </div>
    )
  }

  if (!event) {
    return <p className="text-muted-foreground">Event not found.</p>
  }

  const latestAttempt = event.deliveryAttempts.at(-1)

  return (
    <div className="space-y-6 max-w-5xl">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" asChild>
          <Link to="/events"><ArrowLeft className="h-4 w-4" /></Link>
        </Button>
        <h1 className="text-xl font-bold font-mono truncate">{event.id}</h1>
      </div>

      <div className="flex flex-wrap gap-3 items-center text-sm">
        {latestAttempt && <StatusBadge status={latestAttempt.status} />}
        {event.eventType && <span className="font-mono bg-muted px-2 py-0.5 rounded text-xs">{event.eventType}</span>}
        {event.signatureVerified
          ? <span className="text-green-600 text-xs">✓ Signature verified</span>
          : <span className="text-red-500 text-xs">✗ Signature unverified</span>}
        {event.isDuplicate && <span className="text-yellow-600 text-xs border border-yellow-300 rounded px-1.5 py-0.5">Duplicate</span>}
        <span className="text-muted-foreground"><RelativeTime date={event.receivedAt} /></span>
        <div className="ml-auto">
          <Button
            variant="outline"
            size="sm"
            onClick={handleReplay}
            disabled={isPending || replayMutation.isPending}
          >
            <RefreshCw className="h-3.5 w-3.5 mr-1.5" />
            {isPending || replayMutation.isPending ? 'Replaying…' : 'Replay'}
          </Button>
        </div>
      </div>

      <Tabs defaultValue="payload">
        <TabsList>
          <TabsTrigger value="payload">Payload</TabsTrigger>
          <TabsTrigger value="headers">Headers</TabsTrigger>
          <TabsTrigger value="deliveries">Deliveries ({event.deliveryAttempts.length})</TabsTrigger>
        </TabsList>
        <TabsContent value="payload" className="mt-4">
          <PayloadViewer payload={event.rawPayload} />
        </TabsContent>
        <TabsContent value="headers" className="mt-4">
          <HeadersTable headers={event.headers} />
        </TabsContent>
        <TabsContent value="deliveries" className="mt-4">
          <DeliveryTimeline attempts={event.deliveryAttempts} />
        </TabsContent>
      </Tabs>
    </div>
  )
}
