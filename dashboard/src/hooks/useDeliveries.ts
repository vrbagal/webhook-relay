import { useQuery } from '@tanstack/react-query'
import { getDeliveriesByEvent } from '@/api/deliveries'

export function useDeliveries(eventId: string) {
  return useQuery({
    queryKey: ['deliveries', 'event', eventId],
    queryFn: () => getDeliveriesByEvent(eventId),
    staleTime: 5_000,
    enabled: !!eventId,
  })
}
