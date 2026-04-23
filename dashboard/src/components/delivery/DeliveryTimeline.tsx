import type { DeliveryAttempt } from '@/api/types'
import { AttemptCard } from './AttemptCard'
import { EmptyState } from '@/components/shared/EmptyState'

interface DeliveryTimelineProps {
  attempts: DeliveryAttempt[]
}

export function DeliveryTimeline({ attempts }: DeliveryTimelineProps) {
  if (attempts.length === 0) {
    return <EmptyState title="No delivery attempts" description="This event has not been delivered yet." />
  }

  return (
    <div className="space-y-3">
      {attempts.map((attempt) => (
        <AttemptCard key={attempt.id} attempt={attempt} />
      ))}
    </div>
  )
}
