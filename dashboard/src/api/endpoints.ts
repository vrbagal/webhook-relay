import { apiClient } from './client'
import type { WebhookEndpoint } from './types'

export interface CreateEndpointPayload {
  name: string
  provider: string
  signingSecret: string
  rejectUnverified: boolean
}

export interface UpdateEndpointPayload {
  name: string
  isActive: boolean
  rejectUnverified: boolean
  signingSecret?: string
}

export interface CreateTargetPayload {
  name: string
  targetUrl: string
  timeoutSeconds?: number
}

export async function getEndpoints(): Promise<WebhookEndpoint[]> {
  const res = await apiClient.get<WebhookEndpoint[]>('/api/endpoints')
  return res.data
}

export async function getEndpoint(id: string): Promise<WebhookEndpoint> {
  const res = await apiClient.get<WebhookEndpoint>(`/api/endpoints/${id}`)
  return res.data
}

export async function createEndpoint(payload: CreateEndpointPayload): Promise<WebhookEndpoint> {
  const res = await apiClient.post<WebhookEndpoint>('/api/endpoints', payload)
  return res.data
}

export async function updateEndpoint(id: string, payload: UpdateEndpointPayload): Promise<WebhookEndpoint> {
  const res = await apiClient.put<WebhookEndpoint>(`/api/endpoints/${id}`, payload)
  return res.data
}

export async function deleteEndpoint(id: string): Promise<void> {
  await apiClient.delete(`/api/endpoints/${id}`)
}

export async function addTarget(endpointId: string, payload: CreateTargetPayload): Promise<void> {
  await apiClient.post(`/api/endpoints/${endpointId}/targets`, payload)
}

export async function deleteTarget(endpointId: string, targetId: string): Promise<void> {
  await apiClient.delete(`/api/endpoints/${endpointId}/targets/${targetId}`)
}

// ── Routing rules ─────────────────────────────────────────────────────────────

export type RuleOperator =
  | 'equals' | 'not_equals'
  | 'contains' | 'not_contains'
  | 'exists' | 'not_exists'
  | 'starts_with' | 'ends_with'

export interface CreateRoutingRulePayload {
  jsonPath: string
  operator: RuleOperator
  value?: string
}

export async function addRoutingRule(
  endpointId: string,
  targetId: string,
  payload: CreateRoutingRulePayload,
): Promise<void> {
  await apiClient.post(
    `/api/endpoints/${endpointId}/targets/${targetId}/rules`,
    payload,
  )
}

export async function deleteRoutingRule(
  endpointId: string,
  targetId: string,
  ruleId: string,
): Promise<void> {
  await apiClient.delete(
    `/api/endpoints/${endpointId}/targets/${targetId}/rules/${ruleId}`,
  )
}
