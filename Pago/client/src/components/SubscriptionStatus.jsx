import { useMemo } from 'react';

function formatDate(value) {
  if (!value) {
    return '—';
  }

  const date = typeof value === 'string' ? new Date(value) : value;
  if (Number.isNaN(date.getTime())) {
    return String(value);
  }

  return date.toLocaleString();
}

function variantFromStatus(status) {
  if (!status) return 'pending';
  const normalized = status.toLowerCase();
  if (['authorized', 'active', 'approved', 'paid'].includes(normalized)) return 'active';
  if (['pending', 'in_process', 'processing'].includes(normalized)) return 'pending';
  return 'failed';
}

export default function SubscriptionStatus({
  subscriptionId,
  status,
  loading,
  error,
  live,
  onRefresh,
}) {
  const badgeVariant = useMemo(
    () => variantFromStatus(status?.status || status?.payment_status),
    [status],
  );

  if (!subscriptionId) {
    return null;
  }

  return (
    <div className="card">
      <div className="header">
        <h2>Subscription status</h2>
        <p className="muted small">
          Tracking subscription <strong>#{subscriptionId}</strong>.
        </p>
      </div>

      <div className="stack">
        {error ? <div className="callout error">{error}</div> : null}

        <div className="stack">
          <div className="inline" style={{ gap: '0.75rem', alignItems: 'center' }}>
            <span className="status-badge" data-variant={badgeVariant}>
              {status?.status || status?.payment_status || 'pending'}
            </span>
            {live ? <span className="tag">Live updates</span> : <span className="tag">Polling</span>}
            {loading ? <span className="muted small">Refreshing...</span> : null}
            {onRefresh ? (
              <button className="button" type="button" onClick={onRefresh} disabled={loading}>
                Refresh now
              </button>
            ) : null}
          </div>

          <div className="grid">
            <div className="stack">
              <div className="muted small">Plan</div>
              <strong>{status?.plan || status?.plan_name || '—'}</strong>
              <div className="muted small">{status?.description || status?.reason || 'Recurring payment'}</div>
            </div>
            <div className="stack">
              <div className="muted small">Next charge</div>
              <strong>{formatDate(status?.next_payment_date || status?.next_charge_date)}</strong>
            </div>
            <div className="stack">
              <div className="muted small">Payer</div>
              <strong>{status?.payer_email || status?.payer?.email || '—'}</strong>
            </div>
            <div className="stack">
              <div className="muted small">External ID</div>
              <strong>{status?.external_reference || status?.id || '—'}</strong>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
