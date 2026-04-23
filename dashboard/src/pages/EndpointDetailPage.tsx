import { useTransition, useState } from 'react'
import { useParams, Link } from 'react-router'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { useEndpoint, useUpdateEndpoint } from '@/hooks/useEndpoints'
import { addTarget, deleteTarget } from '@/api/endpoints'
import { useQueryClient } from '@tanstack/react-query'
import { endpointKeys } from '@/hooks/useEndpoints'
import { ProviderBadge } from '@/components/events/ProviderBadge'
import { CopyButton } from '@/components/shared/CopyButton'
import { RoutingRulesEditor } from '@/components/delivery/RoutingRulesEditor'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Separator } from '@/components/ui/separator'
import { Skeleton } from '@/components/ui/skeleton'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogFooter } from '@/components/ui/dialog'
import { ArrowLeft, Plus, Trash2 } from 'lucide-react'

const targetSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  targetUrl: z.string().url('Must be a valid URL'),
  timeoutSeconds: z.coerce.number().min(1).max(300),
})

type TargetForm = z.infer<typeof targetSchema>

export function EndpointDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: endpoint, isLoading } = useEndpoint(id!)
  const updateMutation = useUpdateEndpoint(id!)
  const qc = useQueryClient()
  const [isPending, startTransition] = useTransition()
  const [targetOpen, setTargetOpen] = useState(false)

  const targetForm = useForm<TargetForm>({
    resolver: zodResolver(targetSchema),
    defaultValues: { name: '', targetUrl: '', timeoutSeconds: 30 },
  })

  const handleToggleActive = (active: boolean) => {
    if (!endpoint) return
    void updateMutation.mutateAsync({
      name: endpoint.name,
      isActive: active,
      rejectUnverified: endpoint.rejectUnverified,
    }).then(() => toast.success(active ? 'Endpoint activated' : 'Endpoint deactivated'))
  }

  const handleAddTarget = (data: TargetForm) => {
    startTransition(async () => {
      try {
        await addTarget(id!, data)
        await qc.invalidateQueries({ queryKey: endpointKeys.detail(id!) })
        toast.success('Target added')
        targetForm.reset()
        setTargetOpen(false)
      } catch {
        toast.error('Failed to add target')
      }
    })
  }

  const handleDeleteTarget = (targetId: string, targetName: string) => {
    if (!confirm(`Delete target "${targetName}"?`)) return
    startTransition(async () => {
      try {
        await deleteTarget(id!, targetId)
        await qc.invalidateQueries({ queryKey: endpointKeys.detail(id!) })
        toast.success('Target deleted')
      } catch {
        toast.error('Failed to delete target')
      }
    })
  }

  if (isLoading) {
    return <div className="space-y-4"><Skeleton className="h-8 w-64" /><Skeleton className="h-40 w-full" /></div>
  }

  if (!endpoint) {
    return <p className="text-muted-foreground">Endpoint not found.</p>
  }

  return (
    <div className="space-y-6 max-w-3xl">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" asChild>
          <Link to="/endpoints"><ArrowLeft className="h-4 w-4" /></Link>
        </Button>
        <h1 className="text-xl font-bold">{endpoint.name}</h1>
        <ProviderBadge provider={endpoint.provider} />
      </div>

      <Card>
        <CardHeader><CardTitle className="text-base">Details</CardTitle></CardHeader>
        <CardContent className="space-y-3 text-sm">
          <div className="flex items-center justify-between">
            <span className="text-muted-foreground">Active</span>
            <Switch checked={endpoint.isActive} onCheckedChange={handleToggleActive} />
          </div>
          <div className="flex items-center justify-between">
            <span className="text-muted-foreground">Reject Unverified</span>
            <span>{endpoint.rejectUnverified ? 'Yes' : 'No'}</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-muted-foreground w-24 shrink-0">Ingest URL</span>
            <span className="font-mono text-xs truncate flex-1">{endpoint.ingestUrl}</span>
            <CopyButton text={endpoint.ingestUrl} />
          </div>
        </CardContent>
      </Card>

      <div>
        <div className="flex items-center justify-between mb-3">
          <h2 className="font-semibold">Delivery Targets</h2>
          <Dialog open={targetOpen} onOpenChange={setTargetOpen}>
            <DialogTrigger asChild>
              <Button size="sm" variant="outline">
                <Plus className="h-4 w-4 mr-1.5" />Add Target
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader><DialogTitle>Add Delivery Target</DialogTitle></DialogHeader>
              <form onSubmit={targetForm.handleSubmit(handleAddTarget)} className="space-y-4 pt-2">
                <div className="space-y-1.5">
                  <Label>Name</Label>
                  <Input {...targetForm.register('name')} placeholder="My Server" />
                </div>
                <div className="space-y-1.5">
                  <Label>Target URL</Label>
                  <Input {...targetForm.register('targetUrl')} placeholder="https://my-server.com/webhook" />
                  {targetForm.formState.errors.targetUrl && (
                    <p className="text-xs text-destructive">{targetForm.formState.errors.targetUrl.message}</p>
                  )}
                </div>
                <div className="space-y-1.5">
                  <Label>Timeout (seconds)</Label>
                  <Input {...targetForm.register('timeoutSeconds')} type="number" min={1} max={300} />
                </div>
                <DialogFooter>
                  <Button type="submit" disabled={isPending}>{isPending ? 'Adding…' : 'Add Target'}</Button>
                </DialogFooter>
              </form>
            </DialogContent>
          </Dialog>
        </div>

        {endpoint.targets.length === 0 && (
          <p className="text-sm text-muted-foreground">
            No delivery targets. Add one to start forwarding webhooks.
          </p>
        )}

        <div className="space-y-3">
          {endpoint.targets.map((t) => (
            <Card key={t.id}>
              <CardContent className="pt-4 text-sm">
                {/* Target header */}
                <div className="flex items-start justify-between gap-2">
                  <div className="flex-1 min-w-0 space-y-1">
                    <div className="flex items-center gap-2">
                      <span className="font-medium">{t.name}</span>
                      <span className={t.isActive ? 'text-green-600 text-xs' : 'text-muted-foreground text-xs'}>
                        {t.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </div>
                    <div className="flex items-center gap-1">
                      <span className="font-mono text-xs text-muted-foreground truncate">{t.targetUrl}</span>
                      <CopyButton text={t.targetUrl} className="h-5 w-5 shrink-0" />
                    </div>
                    <p className="text-xs text-muted-foreground">Timeout: {t.timeoutSeconds}s</p>
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-7 w-7 text-destructive hover:text-destructive shrink-0"
                    onClick={() => handleDeleteTarget(t.id, t.name)}
                    disabled={isPending}
                    title="Delete target"
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                </div>

                {/* Routing rules */}
                <Separator className="my-3" />
                <RoutingRulesEditor
                  endpointId={id!}
                  targetId={t.id}
                  rules={t.routingRules}
                />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </div>
  )
}
