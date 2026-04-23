import { useTransition } from 'react'
import { Link } from 'react-router'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { useEndpoints, useCreateEndpoint, useDeleteEndpoint } from '@/hooks/useEndpoints'
import { EmptyState } from '@/components/shared/EmptyState'
import { ProviderBadge } from '@/components/events/ProviderBadge'
import { RelativeTime } from '@/components/shared/RelativeTime'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogFooter } from '@/components/ui/dialog'
import { Card, CardContent } from '@/components/ui/card'
import { CopyButton } from '@/components/shared/CopyButton'
import { Skeleton } from '@/components/ui/skeleton'
import { Plus, Trash2 } from 'lucide-react'
import type { ProviderType } from '@/api/types'
import { useState } from 'react'

const schema = z.object({
  name: z.string().min(1, 'Name is required'),
  provider: z.enum(['Stripe', 'GitHub', 'Twilio', 'Generic']),
  signingSecret: z.string().min(1, 'Signing secret is required'),
  rejectUnverified: z.boolean(),
})

type FormValues = z.infer<typeof schema>

export function EndpointsPage() {
  const { data: endpoints, isLoading } = useEndpoints()
  const createMutation = useCreateEndpoint()
  const deleteMutation = useDeleteEndpoint()
  const [open, setOpen] = useState(false)
  const [isPending, startTransition] = useTransition()

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', provider: 'Stripe', signingSecret: '', rejectUnverified: false },
  })

  const onSubmit = (data: FormValues) => {
    startTransition(async () => {
      try {
        await createMutation.mutateAsync(data)
        toast.success('Endpoint created')
        form.reset()
        setOpen(false)
      } catch {
        toast.error('Failed to create endpoint')
      }
    })
  }

  const handleDelete = (id: string, name: string) => {
    if (!confirm(`Delete endpoint "${name}"?`)) return
    void deleteMutation.mutateAsync(id).then(() => toast.success('Endpoint deleted'))
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Endpoints</h1>
        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger asChild>
            <Button size="sm"><Plus className="h-4 w-4 mr-1.5" />New Endpoint</Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-md">
            <DialogHeader>
              <DialogTitle>Create Endpoint</DialogTitle>
            </DialogHeader>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4 pt-2">
              <div className="space-y-1.5">
                <Label>Name</Label>
                <Input {...form.register('name')} placeholder="My Stripe Endpoint" />
                {form.formState.errors.name && (
                  <p className="text-xs text-destructive">{form.formState.errors.name.message}</p>
                )}
              </div>
              <div className="space-y-1.5">
                <Label>Provider</Label>
                <Select
                  defaultValue="Stripe"
                  onValueChange={(v) => form.setValue('provider', v as ProviderType)}
                >
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Stripe">Stripe</SelectItem>
                    <SelectItem value="GitHub">GitHub</SelectItem>
                    <SelectItem value="Twilio">Twilio</SelectItem>
                    <SelectItem value="Generic">Generic</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-1.5">
                <Label>Signing Secret</Label>
                <Input {...form.register('signingSecret')} type="password" placeholder="whsec_..." />
                {form.formState.errors.signingSecret && (
                  <p className="text-xs text-destructive">{form.formState.errors.signingSecret.message}</p>
                )}
              </div>
              <div className="flex items-center gap-3">
                <Switch
                  id="rejectUnverified"
                  onCheckedChange={(v) => form.setValue('rejectUnverified', v)}
                />
                <Label htmlFor="rejectUnverified">Reject unverified webhooks</Label>
              </div>
              <DialogFooter>
                <Button type="submit" disabled={isPending}>
                  {isPending ? 'Creating…' : 'Create Endpoint'}
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {isLoading && (
        <div className="space-y-3">
          {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-24 w-full" />)}
        </div>
      )}

      {!isLoading && endpoints?.length === 0 && (
        <EmptyState title="No endpoints" description="Create your first endpoint to start receiving webhooks." />
      )}

      <div className="space-y-3">
        {endpoints?.map((ep) => (
          <Card key={ep.id}>
            <CardContent className="pt-4">
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1 min-w-0 space-y-1.5">
                  <div className="flex items-center gap-2">
                    <Link to={`/endpoints/${ep.id}`} className="font-medium hover:underline truncate">
                      {ep.name}
                    </Link>
                    <ProviderBadge provider={ep.provider} />
                    {!ep.isActive && (
                      <span className="text-xs text-muted-foreground border rounded px-1.5 py-0.5">Inactive</span>
                    )}
                  </div>
                  <div className="flex items-center gap-2 text-xs text-muted-foreground font-mono">
                    <span className="truncate max-w-sm">{ep.ingestUrl}</span>
                    <CopyButton text={ep.ingestUrl} className="h-5 w-5" />
                  </div>
                  <p className="text-xs text-muted-foreground">
                    {ep.targets.length} target(s) · Created <RelativeTime date={ep.createdAt} />
                  </p>
                </div>
                <Button
                  variant="ghost"
                  size="icon"
                  className="text-destructive hover:text-destructive shrink-0"
                  onClick={() => handleDelete(ep.id, ep.name)}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
