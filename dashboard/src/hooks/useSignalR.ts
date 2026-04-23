import { useEffect, useRef } from 'react'
import * as signalR from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { eventKeys } from './useEvents'

export function useSignalR() {
  const qc = useQueryClient()
  const connectionRef = useRef<signalR.HubConnection | null>(null)

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_BASE_URL ?? ''}/hubs/webhookrelay`)
      .withAutomaticReconnect([0, 2_000, 5_000, 10_000, 30_000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    connection.on('EventReceived', (_data: { id: string }) => {
      qc.invalidateQueries({ queryKey: eventKeys.lists() })
    })

    connection.on('DeliveryAttempted', (data: {
      eventId: string
      status: string
      httpStatusCode: number | null
    }) => {
      qc.invalidateQueries({ queryKey: eventKeys.detail(data.eventId) })
      qc.invalidateQueries({ queryKey: ['deliveries'] })
    })

    connection.start().catch((err: unknown) =>
      console.warn('SignalR connection failed:', err),
    )

    connectionRef.current = connection
    return () => { void connection.stop() }
  }, [qc])

  return connectionRef
}
