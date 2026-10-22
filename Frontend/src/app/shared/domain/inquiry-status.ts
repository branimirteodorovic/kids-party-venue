import { BadgeTone } from '../ui/badge/badge';

export type InquiryStatus =
  | 'New'
  | 'Seen'
  | 'AwaitingDeposit'
  | 'DepositPaid'
  | 'PaidInFull'
  | 'Completed'
  | 'CancelledRefunded'
  | 'CancelledForfeited'
  | 'Declined'
  | 'Expired'
  | 'Archived';

export interface InquiryStatusMeta {
  /** Shown in the provider panel. */
  label: string;
  /** Shown to the parent on the tracking page. */
  parentLabel: string;
  tone: BadgeTone;
  icon: string;
}

export const INQUIRY_STATUS_META: Record<InquiryStatus, InquiryStatusMeta> = {
  New: { label: 'Novi upit', parentLabel: 'Upit je poslat', tone: 'info', icon: 'inbox' },
  Seen: { label: 'Pročitan', parentLabel: 'Vlasnik je pročitao upit', tone: 'info', icon: 'eye' },
  AwaitingDeposit: {
    label: 'Čeka se kapara',
    parentLabel: 'Čeka se vaša kapara',
    tone: 'warning',
    icon: 'hourglass',
  },
  DepositPaid: {
    label: 'Potvrđeno',
    parentLabel: 'Potvrđeno',
    tone: 'success',
    icon: 'circle-check',
  },
  PaidInFull: {
    label: 'Plaćeno u celosti',
    parentLabel: 'Plaćeno u celosti',
    tone: 'paid',
    icon: 'banknote',
  },
  Completed: { label: 'Održano', parentLabel: 'Održano', tone: 'completed', icon: 'flag' },
  CancelledRefunded: {
    label: 'Otkazano · vraćeno',
    parentLabel: 'Otkazano · kapara vraćena',
    tone: 'refunded',
    icon: 'undo-2',
  },
  CancelledForfeited: {
    label: 'Otkazano · kapara zadržana',
    parentLabel: 'Otkazano · kapara zadržana',
    tone: 'danger',
    icon: 'circle-x',
  },
  Declined: { label: 'Odbijeno', parentLabel: 'Termin nije dostupan', tone: 'muted', icon: 'ban' },
  Expired: { label: 'Isteklo', parentLabel: 'Isteklo', tone: 'muted', icon: 'timer-off' },
  Archived: { label: 'Arhivirano', parentLabel: 'Arhivirano', tone: 'muted', icon: 'archive' },
};
