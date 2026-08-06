
import type { LucideIcon } from 'lucide-react'
import {
  Bot,
  Clock,
  KeyRound,
  LayoutDashboard,
  Paperclip,
  Settings2,
} from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useToast } from '../../../components/toast/toast.provider'
import {
  authService,
  type RegistrationCodeResponse,
} from '../../../services/auth.service'
import {
  settingsService,
  type SystemParameterResponse,
} from '../../../services/settings.service'

export const groupOrder = [
  'Auth',
  'Business',
  'Media',
  'Replies',
  'Occurrence',
  'Geral',
]

export const GROUP_LABELS: Record<string, string> = {
  Auth: 'Autenticação',
  Business: 'Horário de funcionamento',
  Media: 'Mídia',
  Replies: 'Respostas automáticas',
  Occurrence: 'Ocorrências',
  Geral: 'Geral',
}

export const GROUP_ICONS: Record<string, LucideIcon> = {
  Auth: KeyRound,
  Business: Clock,
  Media: Paperclip,
  Replies: Bot,
  Occurrence: LayoutDashboard,
  Geral: Settings2,
}

export const TYPE_LABELS: Record<string, string> = {
  String: 'Texto',
  Int: 'Número',
  Bool: 'Ligado/Desligado',
  JsonList: 'Lista',
}

export const PARAM_LABELS: Record<string, string> = {
  'Auth:PasswordMinLength': 'Tamanho mínimo da senha',
  'Auth:RequireRegistrationCode': 'Exigir código para criar usuário',
  'Auth:RegistrationCodeExpiryHours': 'Validade do código de permissão (horas)',
  'Business:Enabled': 'Atender só no horário de funcionamento',
  'Business:OpenTime': 'Abre às',
  'Business:CloseTime': 'Fecha às',
  'Business:WorkingDays': 'Dias de atendimento',
  'Business:OutsideHoursMessage': 'Mensagem fora do horário',
  'Business:Timezone': 'Fuso horário',
  'Media:AllowedTypes': 'Tipos de mídia aceitos',
  'Media:UnsupportedMessage': 'Resposta para mídia não suportada',
  'Replies:SenderName':
    'Nome fictício de quem responde às mensagens automáticas',
  'Occurrence:StatusFlow': 'Ordem dos status das ocorrências',
}

export const PARAM_HINTS: Record<string, string> = {
  'Auth:PasswordMinLength': 'Senhas menores que isso não poderão ser criadas.',
  'Auth:RequireRegistrationCode':
    'Quando ligado, novos usuários precisam de um código de permissão gerado na tela de configurações.',
  'Auth:RegistrationCodeExpiryHours':
    'Depois desse tempo o código expira e não pode mais ser usado.',
  'Business:Enabled':
    'Quando ligado, mensagens fora do horário recebem uma resposta automática.',
  'Business:OpenTime': 'Ex: 08:00',
  'Business:CloseTime': 'Ex: 18:00',
  'Business:WorkingDays':
    'Selecione os dias da semana em que o atendimento está disponível.',
  'Business:OutsideHoursMessage':
    'Use {open}, {close} e {days} para preencher automaticamente.',
  'Business:Timezone':
    'Fuso horário usado para calcular o horário de funcionamento.',
  'Media:AllowedTypes': 'Selecione os tipos de mídia que o sistema aceita.',
  'Media:UnsupportedMessage':
    'Resposta automática enviada quando a mídia recebida não é aceita.',
  'Replies:SenderName':
    'Nome fictício exibido como remetente em todas as respostas automáticas (ex.: mídia não suportada, fora do horário). Deixe vazio para usar o nome do usuário logado.',
  'Occurrence:StatusFlow':
    'Selecione os status na ordem em que devem ser usados.',
}

export const WORKING_DAYS_OPTIONS = [
  { value: 'Monday', label: 'Segunda' },
  { value: 'Tuesday', label: 'Terça' },
  { value: 'Wednesday', label: 'Quarta' },
  { value: 'Thursday', label: 'Quinta' },
  { value: 'Friday', label: 'Sexta' },
  { value: 'Saturday', label: 'Sábado' },
  { value: 'Sunday', label: 'Domingo' },
]

export const MEDIA_TYPE_OPTIONS = [
  { value: 'Image', label: 'Imagem' },
  { value: 'Audio', label: 'Áudio' },
  { value: 'Video', label: 'Vídeo' },
  { value: 'Document', label: 'Documento' },
  { value: 'Sticker', label: 'Figurinha' },
]

export const STATUS_FLOW_OPTIONS = [
  { value: 'Open', label: 'Aberta' },
  { value: 'InProgress', label: 'Em andamento' },
  { value: 'Resolved', label: 'Resolvida' },
  { value: 'Closed', label: 'Fechada' },
]

export const LIST_OPTIONS: Record<string, { value: string; label: string }[]> =
  {
    'Business:WorkingDays': WORKING_DAYS_OPTIONS,
    'Media:AllowedTypes': MEDIA_TYPE_OPTIONS,
    'Occurrence:StatusFlow': STATUS_FLOW_OPTIONS,
  }

export const AMERICA_TIMEZONES = [
  'America/Sao_Paulo',
  'America/Manaus',
  'America/Porto_Velho',
  'America/Belem',
  'America/Fortaleza',
  'America/Recife',
  'America/Maceio',
  'America/Bahia',
  'America/Cuiaba',
  'America/Campo_Grande',
  'America/Santarem',
  'America/Araguaina',
  'America/Noronha',
  'America/Buenos_Aires',
  'America/La_Paz',
  'America/Santiago',
  'America/Bogota',
  'America/Lima',
  'America/Caracas',
  'America/Asuncion',
  'America/Montevideo',
  'America/Quito',
  'America/Guayaquil',
  'America/Panama',
  'America/Costa_Rica',
  'America/Guatemala',
  'America/Managua',
  'America/El_Salvador',
  'America/Havana',
  'America/Jamaica',
  'America/Puerto_Rico',
  'America/Port-au-Prince',
  'America/Mexico_City',
  'America/Monterrey',
  'America/New_York',
  'America/Toronto',
  'America/Chicago',
  'America/Denver',
  'America/Los_Angeles',
  'America/Vancouver',
  'America/Phoenix',
  'America/Halifax',
  'America/St_Johns',
  'America/Winnipeg',
  'America/Regina',
  'America/Edmonton',
  'America/Whitehorse',
  'America/Anchorage',
  'America/Adak',
  'Pacific/Honolulu',
]

export function timeZoneLabel(tz: string): string {
  try {
    const parts = new Intl.DateTimeFormat('pt-BR', {
      timeZone: tz,
      timeZoneName: 'shortOffset',
    }).formatToParts(new Date())
    const offset = parts.find((p) => p.type === 'timeZoneName')?.value ?? ''
    return `${tz.replace('America/', '').replaceAll('_', ' ')}${offset ? ` (${offset})` : ''}`
  } catch {
    return tz
  }
}

export function displayValue(param: SystemParameterResponse): string {
  const v = param.value
  if (v === null || v === undefined) return ''
  if (param.type === 'JsonList') {
    try {
      const arr = JSON.parse(v)
      return Array.isArray(arr) ? arr.join(', ') : v
    } catch {
      return v
    }
  }
  return v
}

export function serializeValue(
  param: SystemParameterResponse,
  display: string,
): string {
  if (param.type === 'JsonList') {
    const arr = display
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean)
    return JSON.stringify(arr)
  }
  return display.trim()
}

export function validateValue(
  param: SystemParameterResponse,
  display: string,
): string | null {
  if (param.type === 'Int' && !/^-?\d+$/.test(display.trim())) {
    return 'Informe um número inteiro'
  }
  if (
    (param.key === 'Business:OpenTime' || param.key === 'Business:CloseTime') &&
    !/^\d{2}:\d{2}$/.test(display.trim())
  ) {
    return 'Use o formato HH:mm'
  }
  return null
}

export function useSettings() {
  const { toast } = useToast()

  const [parameters, setParameters] = useState<SystemParameterResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [reloading, setReloading] = useState(false)
  const [values, setValues] = useState<Record<string, string>>({})
  const [dirty, setDirty] = useState<Record<string, boolean>>({})
  const [originalValues, setOriginalValues] = useState<Record<string, string>>(
    {},
  )
  const [query, setQuery] = useState('')
  const [collapsed, setCollapsed] = useState<Set<string>>(new Set())
  const [generatingCode, setGeneratingCode] = useState(false)
  const [generatedCodes, setGeneratedCodes] = useState<
    RegistrationCodeResponse[]
  >([])

  const load = useCallback(
    () =>
      settingsService.list().then((data) => {
        setParameters(data)
        const initial: Record<string, string> = {}
        data.forEach((p) => {
          initial[p.key] = displayValue(p)
        })
        setValues(initial)
        setOriginalValues(initial)
        setDirty({})
      }),
    [],
  )

  useEffect(() => {
    load()
      .catch((e) => console.error('[Settings] erro ao carregar:', e))
      .finally(() => setLoading(false))
  }, [load])

  function setValue(key: string, value: string) {
    setValues((prev) => ({ ...prev, [key]: value }))
    setDirty((prev) => ({ ...prev, [key]: true }))
  }

  function revertValue(key: string) {
    setValues((prev) => ({ ...prev, [key]: originalValues[key] ?? '' }))
    setDirty((prev) => ({ ...prev, [key]: false }))
  }

  function toggleGroup(group: string) {
    setCollapsed((prev) => {
      const next = new Set(prev)
      if (next.has(group)) next.delete(group)
      else next.add(group)
      return next
    })
  }

  const grouped = useMemo(
    () =>
      parameters.reduce<Record<string, SystemParameterResponse[]>>((acc, p) => {
        const group = p.group ?? 'Geral'
        if (!acc[group]) acc[group] = []
        acc[group].push(p)
        return acc
      }, {}),
    [parameters],
  )

  const filteredGroups = useMemo(() => {
    const q = query.trim().toLowerCase()
    const result: Record<string, SystemParameterResponse[]> = {}
    for (const group of groupOrder) {
      const items = grouped[group]
      if (!items || items.length === 0) continue
      const filtered = q
        ? items.filter(
            (p) =>
              p.key.toLowerCase().includes(q) ||
              (PARAM_LABELS[p.key] ?? '').toLowerCase().includes(q) ||
              (p.group ?? '').toLowerCase().includes(q),
          )
        : items
      if (filtered.length > 0) result[group] = filtered
    }
    return result
  }, [grouped, query])

  const visibleGroupNames = Object.keys(filteredGroups)

  function setAllCollapsed(value: boolean) {
    setCollapsed(new Set(value ? visibleGroupNames : []))
  }

  const validationErrors = useMemo(() => {
    const errs: Record<string, string> = {}
    parameters.forEach((p) => {
      if (p.type === 'Bool') return
      const error = validateValue(p, values[p.key] ?? '')
      if (error) errs[p.key] = error
    })
    return errs
  }, [parameters, values])

  const dirtyCount = Object.keys(dirty).filter((k) => dirty[k]).length
  const hasErrors = Object.keys(validationErrors).length > 0
  const hasChanges = dirtyCount > 0

  async function save() {
    const keys = Object.keys(dirty).filter((k) => dirty[k])
    if (keys.length === 0) {
      toast.info('Nenhuma alteração para salvar')
      return
    }
    if (hasErrors) {
      toast.error('Corrija os campos inválidos antes de salvar')
      return
    }

    setSaving(true)
    try {
      for (const key of keys) {
        const param = parameters.find((p) => p.key === key)
        if (!param) continue
        await settingsService.update(
          key,
          serializeValue(param, values[key] ?? ''),
        )
      }
      toast.success('Configurações salvas')
      await load()
    } catch (e) {
      console.error('[Settings] erro ao salvar:', e)
      toast.error('Erro ao salvar configurações')
    } finally {
      setSaving(false)
    }
  }

  async function handleReload() {
    setReloading(true)
    try {
      await settingsService.reload()
      toast.success('Cache recarregado')
      await load()
    } catch (e) {
      console.error('[Settings] erro ao recarregar:', e)
      toast.error('Erro ao recarregar cache')
    } finally {
      setReloading(false)
    }
  }

  async function generateCode() {
    setGeneratingCode(true)
    try {
      const codes = await authService.generateCodes(1)
      const list = Array.isArray(codes) ? codes : []
      setGeneratedCodes((prev) => {
        const existing = new Set(prev.map((c) => c.id))
        return [...list.filter((c) => !existing.has(c.id)), ...prev]
      })
      toast.success('Código de permissão gerado')
    } catch (e) {
      console.error('[Settings] erro ao gerar código:', e)
      toast.error(
        'Erro ao gerar código — apenas Administrador/Dev pode fazer isso.',
      )
    } finally {
      setGeneratingCode(false)
    }
  }

  return {
    parameters,
    grouped,
    filteredGroups,
    groupOrder,
    loading,
    saving,
    reloading,
    values,
    dirty,
    dirtyCount,
    hasChanges,
    validationErrors,
    hasErrors,
    query,
    setQuery,
    collapsed,
    toggleGroup,
    setAllCollapsed,
    setValue,
    revertValue,
    save,
    reload: handleReload,
    generatingCode,
    generatedCodes,
    generateCode,
  }
}
