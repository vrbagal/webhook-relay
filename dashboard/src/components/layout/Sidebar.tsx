import { Link, useLocation } from 'react-router'
import { Activity, Globe, LayoutDashboard, Settings, Zap } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useUiStore } from '@/stores/uiStore'
import { Separator } from '@/components/ui/separator'

const navItems = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/events', label: 'Events', icon: Activity },
  { to: '/endpoints', label: 'Endpoints', icon: Globe },
  { to: '/settings', label: 'Settings', icon: Settings },
]

export function Sidebar() {
  const location = useLocation()
  const { sidebarOpen } = useUiStore()

  if (!sidebarOpen) return null

  return (
    <aside className="w-56 shrink-0 border-r bg-background flex flex-col">
      <div className="flex items-center gap-2 px-4 py-5">
        <Zap className="h-5 w-5 text-primary" />
        <span className="font-semibold text-base tracking-tight">WebhookRelay</span>
      </div>
      <Separator />
      <nav className="flex-1 py-4 space-y-1 px-2">
        {navItems.map(({ to, label, icon: Icon }) => (
          <Link
            key={to}
            to={to}
            className={cn(
              'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
              location.pathname === to
                ? 'bg-primary text-primary-foreground'
                : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
            )}
          >
            <Icon className="h-4 w-4" />
            {label}
          </Link>
        ))}
      </nav>
    </aside>
  )
}
