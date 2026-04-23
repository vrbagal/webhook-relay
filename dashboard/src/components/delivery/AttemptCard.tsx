import type { DeliveryAttempt } from '@/api/types'
import { StatusBadge } from '@/components/events/StatusBadge'
import { RelativeTime } from '@/components/shared/RelativeTime'
import { Card, CardContent } from '@/components/ui/card'

interface AttemptCardProps {
  attempt: DeliveryAttempt
}

export function AttemptCard({ attempt }: AttemptCardProps) {
  return (
    <Card className="text-sm">
      <CardContent className="pt-4 space-y-2">
        <div className="flex items-center justify-between">
          <span className="font-medium">Attempt #{attempt.attemptNumber}</span>
          <div className="flex items-center gap-2">
            {attempt.isReplay && (
              <span className="text-xs text-muted-foreground border rounded px-1.5 py-0.5">Replay</span>
            )}
            <StatusBadge status={attempt.status} />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-xs text-muted-foreground">
          <span>Target</span>
          <span className="font-mono truncate">{attempt.targetName}</span>
          <span>URL</span>
          <span className="font-mono truncate">{attempt.targetUrl}</span>
          <span>Duration</span>
          <span>{attempt.durationMs}ms</span>
          {attempt.httpStatusCode !== null && (
            <>
              <span>HTTP Status</span>
              <span className={attempt.httpStatusCode >= 200 && attempt.httpStatusCode < 300 ? 'text-green-600' : 'text-red-500'}>
                {attempt.httpStatusCode}
              </span>
            </>
          )}
          <span>Attempted</span>
          <RelativeTime date={attempt.attemptedAt} />
          {attempt.nextRetryAt && (
            <>
              <span>Next Retry</span>
              <RelativeTime date={attempt.nextRetryAt} />
            </>
          )}
        </div>

        {attempt.errorMessage && (
          <div className="rounded bg-red-50 border border-red-200 px-3 py-2 text-xs text-red-700 font-mono">
            {attempt.errorMessage}
          </div>
        )}

        {attempt.responseBody && (
          <details className="text-xs">
            <summary className="cursor-pointer text-muted-foreground hover:text-foreground">Response body</summary>
            <pre className="mt-2 overflow-auto rounded bg-muted p-2 text-xs">{attempt.responseBody}</pre>
          </details>
        )}
      </CardContent>
    </Card>
  )
}
