import { useMutation, useQuery, useQueryClient, queryOptions } from '@tanstack/react-query'
import { getEvent, getEvents, replayEvent } from '@/api/events'
import type { EventQueryParams, ReplayRequest } from '@/api/types'

export const eventKeys = {
  all: () => ['events'] as const,
  lists: () => ['events', 'list'] as const,
  list: (p: EventQueryParams) => ['events', 'list', p] as const,
  detail: (id: string) => ['events', 'detail', id] as const,
}

export const eventDetailOptions = (id: string) =>
  queryOptions({
    queryKey: eventKeys.detail(id),
    queryFn: () => getEvent(id),
    staleTime: 5_000,
  })

export function useEvents(params: EventQueryParams) {
  return useQuery({
    queryKey: eventKeys.list(params),
    queryFn: () => getEvents(params),
    staleTime: 10_000,
    placeholderData: (prev) => prev,
  })
}

export function useEvent(id: string) {
  return useQuery(eventDetailOptions(id))
}

export function useReplayEvent() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req?: ReplayRequest }) =>
      replayEvent(id, req),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: eventKeys.detail(id) })
      qc.invalidateQueries({ queryKey: ['deliveries'] })
    },
  })
}
