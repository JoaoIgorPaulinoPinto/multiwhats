'use client'

import { useRouter } from 'next/navigation'
import { useEffect } from 'react'
import { useAuthStore } from '../../stores/auth-store'
import { ReportsView } from '../../components/screens/reports/reports.view'

export default function ReportPage() {
  const user = useAuthStore((s) => s.user)
  const loading = useAuthStore((s) => s.loading)
  const router = useRouter()

  useEffect(() => {
    if (loading) return
    if (user) {
      router.replace('/relatorio')
    }
  }, [user, loading, router])

  if (loading) return null
  if (user) return null
  return <ReportsView />
}
