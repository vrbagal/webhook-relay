import { formatDistanceToNow } from 'date-fns'

interface RelativeTimeProps {
  date: string | Date
  className?: string
}

export function RelativeTime({ date, className }: RelativeTimeProps) {
  const d = typeof date === 'string' ? new Date(date) : date
  return (
    <time dateTime={d.toISOString()} title={d.toLocaleString()} className={className}>
      {formatDistanceToNow(d, { addSuffix: true })}
    </time>
  )
}
