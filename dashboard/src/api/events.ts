import { apiClient } from './client'
import type { EventQueryParams, PagedResult, ReplayRequest, WebhookEvent } from './types'

export async function getEvents(params: EventQueryParams): Promise<PagedResult<WebhookEvent>> {
  const res = await apiClient.get<PagedResult<WebhookEvent>>('/api/events', { params })
  return res.data
}

export async function getEvent(id: string): Promise<WebhookEvent> {
  const res = await apiClient.get<WebhookEvent>(`/api/events/${id}`)
  return res.data
}

export async function replayEvent(id: string, req?: ReplayRequest): Promise<void> {
  await apiClient.post(`/api/events/${id}/replay`, req ?? {})
}
