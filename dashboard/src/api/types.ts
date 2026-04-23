export type DeliveryStatus = 'Pending' | 'Delivered' | 'Failed' | 'DeadLettered'
export type ProviderType = 'Stripe' | 'GitHub' | 'Twilio' | 'Generic'

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface WebhookEndpoint {
  id: string
  name: string
  provider: ProviderType
  isActive: boolean
  rejectUnverified: boolean
  ingestUrl: string
  createdAt: string
  targets: DeliveryTarget[]
}

export interface WebhookEvent {
  id: string
  endpointId: string
  endpointName: string
  rawPayload: string
  headers: Record<string, string>
  providerEventId: string | null
  eventType: string | null
  signatureVerified: boolean
  isDuplicate: boolean
  receivedAt: string
  deliveryAttempts: DeliveryAttempt[]
}

export interface DeliveryAttempt {
  id: string
  eventId: string
  targetId: string
  targetName: string
  targetUrl: string
  attemptNumber: number
  isReplay: boolean
  status: DeliveryStatus
  httpStatusCode: number | null
  responseBody: string | null
  errorMessage: string | null
  durationMs: number
  attemptedAt: string
  nextRetryAt: string | null
}

export interface DeliveryTarget {
  id: string
  endpointId: string
  name: string
  targetUrl: string
  isActive: boolean
  timeoutSeconds: number
  routingRules: RoutingRule[]
}

export interface RoutingRule {
  id: string
  targetId: string
  jsonPath: string
  operator: string
  value: string | null
}

export interface EventQueryParams {
  endpointId?: string
  status?: DeliveryStatus
  eventType?: string
  providerEventId?: string
  from?: string
  to?: string
  page?: number
  pageSize?: number
}

export interface ReplayRequest {
  overrideTargetUrl?: string
  stripOriginalHeaders?: boolean
}
