import { Inbox } from 'lucide-react'

interface EmptyStateProps {
  title?: string
  description?: string
}

export function EmptyState({
  title = 'Nothing here yet',
  description = 'No data to display.',
}: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-20 text-center text-muted-foreground">
      <Inbox className="mb-4 h-12 w-12 opacity-30" />
      <p className="text-base font-medium">{title}</p>
      <p className="text-sm">{description}</p>
    </div>
  )
}
