import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createEndpoint,
  deleteEndpoint,
  getEndpoint,
  getEndpoints,
  updateEndpoint,
  type CreateEndpointPayload,
  type UpdateEndpointPayload,
} from '@/api/endpoints'

export const endpointKeys = {
  all: () => ['endpoints'] as const,
  lists: () => ['endpoints', 'list'] as const,
  detail: (id: string) => ['endpoints', 'detail', id] as const,
}

export function useEndpoints() {
  return useQuery({
    queryKey: endpointKeys.lists(),
    queryFn: getEndpoints,
    staleTime: 30_000,
  })
}

export function useEndpoint(id: string) {
  return useQuery({
    queryKey: endpointKeys.detail(id),
    queryFn: () => getEndpoint(id),
    staleTime: 10_000,
  })
}

export function useCreateEndpoint() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateEndpointPayload) => createEndpoint(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: endpointKeys.lists() }),
  })
}

export function useUpdateEndpoint(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: UpdateEndpointPayload) => updateEndpoint(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: endpointKeys.detail(id) })
      qc.invalidateQueries({ queryKey: endpointKeys.lists() })
    },
  })
}

export function useDeleteEndpoint() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => deleteEndpoint(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: endpointKeys.lists() }),
  })
}
