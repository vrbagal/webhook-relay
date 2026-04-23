import { useEvents } from '@/hooks/useEvents'
import { useEndpoints } from '@/hooks/useEndpoints'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { EventTable } from '@/components/events/EventTable'
import { Activity, CheckCircle, Globe, XCircle } from 'lucide-react'

export function DashboardPage() {
  const { data: eventsData, isLoading: eventsLoading } = useEvents({ page: 1, pageSize: 10 })
  const { data: endpoints } = useEndpoints()

  const events = eventsData?.items ?? []
  const delivered = events.filter(e => e.deliveryAttempts.at(-1)?.status === 'Delivered').length
  const failed = events.filter(e => e.deliveryAttempts.at(-1)?.status === 'Failed').length

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Dashboard</h1>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard title="Total Events" value={eventsData?.totalCount ?? 0} icon={<Activity className="h-4 w-4" />} />
        <StatCard title="Endpoints" value={endpoints?.length ?? 0} icon={<Globe className="h-4 w-4" />} />
        <StatCard title="Delivered" value={delivered} icon={<CheckCircle className="h-4 w-4 text-green-500" />} />
        <StatCard title="Failed" value={failed} icon={<XCircle className="h-4 w-4 text-red-500" />} />
      </div>

      <div>
        <h2 className="text-lg font-semibold mb-3">Recent Events</h2>
        <EventTable events={events} isLoading={eventsLoading} />
      </div>
    </div>
  )
}

function StatCard({ title, value, icon }: { title: string; value: number; icon: React.ReactNode }) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">{title}</CardTitle>
        {icon}
      </CardHeader>
      <CardContent>
        <p className="text-3xl font-bold">{value}</p>
      </CardContent>
    </Card>
  )
}
