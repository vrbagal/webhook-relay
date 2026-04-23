import { Badge } from '@/components/ui/badge'
import type { ProviderType } from '@/api/types'

const config: Record<ProviderType, { label: string; className: string }> = {
  Stripe: { label: 'Stripe', className: 'bg-purple-100 text-purple-800 border-purple-200' },
  GitHub: { label: 'GitHub', className: 'bg-gray-100 text-gray-800 border-gray-300' },
  Twilio: { label: 'Twilio', className: 'bg-red-100 text-red-700 border-red-200' },
  Generic: { label: 'Generic', className: 'bg-blue-100 text-blue-800 border-blue-200' },
}

export function ProviderBadge({ provider }: { provider: ProviderType }) {
  const { label, className } = config[provider] ?? config.Generic
  return <Badge variant="outline" className={className}>{label}</Badge>
}
