import { Badge } from '@/components/ui/badge'
import type { DeliveryStatus } from '@/api/types'

const config: Record<DeliveryStatus, { label: string; className: string }> = {
  Pending: { label: 'Pending', className: 'bg-yellow-100 text-yellow-800 border-yellow-200' },
  Delivered: { label: 'Delivered', className: 'bg-green-100 text-green-800 border-green-200' },
  Failed: { label: 'Failed', className: 'bg-red-100 text-red-800 border-red-200' },
  DeadLettered: { label: 'Dead Letter', className: 'bg-gray-100 text-gray-700 border-gray-300' },
}

export function StatusBadge({ status }: { status: DeliveryStatus }) {
  const { label, className } = config[status] ?? config.Pending
  return <Badge variant="outline" className={className}>{label}</Badge>
}
