import { apiClient } from './client'
import type { DeliveryAttempt } from './types'

export async function getDeliveriesByEvent(eventId: string): Promise<DeliveryAttempt[]> {
  const res = await apiClient.get<DeliveryAttempt[]>(`/api/deliveries/event/${eventId}`)
  return res.data
}
