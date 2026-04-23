import { useTransition, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { useQueryClient } from '@tanstack/react-query'
import { addRoutingRule, deleteRoutingRule, type RuleOperator } from '@/api/endpoints'
import { endpointKeys } from '@/hooks/useEndpoints'
import type { RoutingRule } from '@/api/types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogFooter } from '@/components/ui/dialog'
import { Trash2, Plus, Filter } from 'lucide-react'

const OPERATORS: { value: RuleOperator; label: string; needsValue: boolean }[] = [
  { value: 'equals',       label: 'equals',        needsValue: true  },
  { value: 'not_equals',   label: 'not equals',    needsValue: true  },
  { value: 'contains',     label: 'contains',      needsValue: true  },
  { value: 'not_contains', label: 'not contains',  needsValue: true  },
  { value: 'starts_with',  label: 'starts with',   needsValue: true  },
  { value: 'ends_with',    label: 'ends with',     needsValue: true  },
  { value: 'exists',       label: 'exists',        needsValue: false },
  { value: 'not_exists',   label: 'does not exist',needsValue: false },
]

const schema = z.object({
  jsonPath: z.string().min(1, 'JSON path is required').startsWith('$', 'Must start with $ e.g. $.type'),
  operator: z.enum(['equals','not_equals','contains','not_contains','exists','not_exists','starts_with','ends_with']),
  value: z.string().optional(),
})

type FormValues = z.infer<typeof schema>

interface RoutingRulesEditorProps {
  endpointId: string
  targetId: string
  rules: RoutingRule[]
}

export function RoutingRulesEditor({ endpointId, targetId, rules }: RoutingRulesEditorProps) {
  const qc = useQueryClient()
  const [open, setOpen] = useState(false)
  const [isPending, startTransition] = useTransition()

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { jsonPath: '$.', operator: 'equals', value: '' },
  })

  const selectedOperator = form.watch('operator')
  const needsValue = OPERATORS.find(o => o.value === selectedOperator)?.needsValue ?? true

  const handleAdd = (data: FormValues) => {
    startTransition(async () => {
      try {
        await addRoutingRule(endpointId, targetId, {
          jsonPath: data.jsonPath,
          operator: data.operator,
          value: needsValue ? data.value : undefined,
        })
        await qc.invalidateQueries({ queryKey: endpointKeys.detail(endpointId) })
        toast.success('Routing rule added')
        form.reset({ jsonPath: '$.', operator: 'equals', value: '' })
        setOpen(false)
      } catch {
        toast.error('Failed to add routing rule')
      }
    })
  }

  const handleDelete = (ruleId: string) => {
    startTransition(async () => {
      try {
        await deleteRoutingRule(endpointId, targetId, ruleId)
        await qc.invalidateQueries({ queryKey: endpointKeys.detail(endpointId) })
        toast.success('Rule removed')
      } catch {
        toast.error('Failed to remove rule')
      }
    })
  }

  return (
    <div className="mt-3 space-y-2">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <Filter className="h-3.5 w-3.5" />
          <span className="font-medium">Routing rules</span>
          {rules.length > 0 && (
            <span className="ml-1 rounded-full bg-muted px-1.5 py-0.5 text-xs">{rules.length}</span>
          )}
        </div>
        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger asChild>
            <Button variant="ghost" size="sm" className="h-6 px-2 text-xs">
              <Plus className="h-3 w-3 mr-1" />Add rule
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-md">
            <DialogHeader>
              <DialogTitle>Add Routing Rule</DialogTitle>
            </DialogHeader>
            <p className="text-sm text-muted-foreground -mt-2">
              All rules are ANDed — the event must match every rule to be delivered to this target.
            </p>
            <form onSubmit={form.handleSubmit(handleAdd)} className="space-y-4 pt-1">
              <div className="space-y-1.5">
                <Label>JSON Path</Label>
                <Input
                  {...form.register('jsonPath')}
                  placeholder="$.type"
                  className="font-mono text-sm"
                />
                {form.formState.errors.jsonPath && (
                  <p className="text-xs text-destructive">{form.formState.errors.jsonPath.message}</p>
                )}
                <p className="text-xs text-muted-foreground">
                  Dot-notation path into the payload, e.g. <code className="bg-muted px-1 rounded">$.data.object.amount</code>
                </p>
              </div>

              <div className="space-y-1.5">
                <Label>Operator</Label>
                <Select
                  defaultValue="equals"
                  onValueChange={(v) => form.setValue('operator', v as RuleOperator)}
                >
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {OPERATORS.map(op => (
                      <SelectItem key={op.value} value={op.value}>{op.label}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {needsValue && (
                <div className="space-y-1.5">
                  <Label>Value</Label>
                  <Input {...form.register('value')} placeholder="payment_intent.created" />
                </div>
              )}

              <DialogFooter>
                <Button type="submit" disabled={isPending}>
                  {isPending ? 'Adding…' : 'Add Rule'}
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {rules.length === 0 ? (
        <p className="text-xs text-muted-foreground pl-1">
          No rules — all events are delivered to this target.
        </p>
      ) : (
        <ul className="space-y-1">
          {rules.map((rule) => (
            <li
              key={rule.id}
              className="flex items-center justify-between gap-2 rounded border bg-muted/40 px-3 py-1.5 text-xs font-mono"
            >
              <span className="text-blue-600 dark:text-blue-400">{rule.jsonPath}</span>
              <span className="text-muted-foreground">{rule.operator}</span>
              {rule.value !== null && rule.value !== undefined && (
                <span className="text-green-700 dark:text-green-400">"{rule.value}"</span>
              )}
              <Button
                variant="ghost"
                size="icon"
                className="h-5 w-5 ml-auto text-destructive hover:text-destructive shrink-0"
                onClick={() => handleDelete(rule.id)}
                disabled={isPending}
              >
                <Trash2 className="h-3 w-3" />
              </Button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
