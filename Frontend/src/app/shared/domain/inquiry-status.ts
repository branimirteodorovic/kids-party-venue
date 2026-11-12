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
  New: {
    label: $localize`:@@inquiryStatus.new:Novi upit`,
    parentLabel: $localize`:@@inquiryStatus.parentNew:Upit je poslat`,
    tone: 'info',
    icon: 'inbox',
  },
  Seen: {
    label: $localize`:@@inquiryStatus.seen:Pročitan`,
    parentLabel: $localize`:@@inquiryStatus.parentSeen:Vlasnik je pročitao upit`,
    tone: 'info',
    icon: 'eye',
  },
  AwaitingDeposit: {
    label: $localize`:@@inquiryStatus.awaitingDeposit:Čeka se kapara`,
    parentLabel: $localize`:@@inquiryStatus.parentAwaitingDeposit:Čeka se vaša kapara`,
    tone: 'warning',
    icon: 'hourglass',
  },
  DepositPaid: {
    label: $localize`:@@inquiryStatus.depositPaid:Potvrđeno`,
    parentLabel: $localize`:@@inquiryStatus.parentDepositPaid:Potvrđeno`,
    tone: 'success',
    icon: 'circle-check',
  },
  PaidInFull: {
    label: $localize`:@@inquiryStatus.paidInFull:Plaćeno u celosti`,
    parentLabel: $localize`:@@inquiryStatus.parentPaidInFull:Plaćeno u celosti`,
    tone: 'paid',
    icon: 'banknote',
  },
  Completed: {
    label: $localize`:@@inquiryStatus.completed:Održano`,
    parentLabel: $localize`:@@inquiryStatus.parentCompleted:Održano`,
    tone: 'completed',
    icon: 'flag',
  },
  CancelledRefunded: {
    label: $localize`:@@inquiryStatus.cancelledRefunded:Otkazano · vraćeno`,
    parentLabel: $localize`:@@inquiryStatus.parentCancelledRefunded:Otkazano · kapara vraćena`,
    tone: 'refunded',
    icon: 'undo-2',
  },
  CancelledForfeited: {
    label: $localize`:@@inquiryStatus.cancelledForfeited:Otkazano · kapara zadržana`,
    parentLabel: $localize`:@@inquiryStatus.parentCancelledForfeited:Otkazano · kapara zadržana`,
    tone: 'danger',
    icon: 'circle-x',
  },
  Declined: {
    label: $localize`:@@inquiryStatus.declined:Odbijeno`,
    parentLabel: $localize`:@@inquiryStatus.parentDeclined:Termin nije dostupan`,
    tone: 'muted',
    icon: 'ban',
  },
  Expired: {
    label: $localize`:@@inquiryStatus.expired:Isteklo`,
    parentLabel: $localize`:@@inquiryStatus.parentExpired:Isteklo`,
    tone: 'muted',
    icon: 'timer-off',
  },
  Archived: {
    label: $localize`:@@inquiryStatus.archived:Arhivirano`,
    parentLabel: $localize`:@@inquiryStatus.parentArchived:Arhivirano`,
    tone: 'muted',
    icon: 'archive',
  },
};
