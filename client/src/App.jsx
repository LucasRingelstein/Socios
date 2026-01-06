import { useCallback, useEffect, useMemo, useState } from 'react';
import SubscriptionForm from './components/SubscriptionForm';
import SubscriptionStatus from './components/SubscriptionStatus';
import {
  createSubscription,
  getBaseUrl,
  getStatusStreamUrl,
  getSubscriptionStatus,
} from './api/subscriptions';
import useMercadoPago from './hooks/useMercadoPago';

const POLL_INTERVAL = 5000;

export default function App() {
  const [subscriptionId, setSubscriptionId] = useState('');
  const [subscriptionStatus, setSubscriptionStatus] = useState(null);
  const [creating, setCreating] = useState(false);
  const [statusLoading, setStatusLoading] = useState(false);
  const [statusError, setStatusError] = useState('');
  const [checkoutError, setCheckoutError] = useState('');
  const [live, setLive] = useState(false);

  const { mercadoPago, loading: sdkLoading, error: sdkError, publicKey } = useMercadoPago();

  const apiBaseUrl = useMemo(() => getBaseUrl() || window.location.origin, []);

  const refreshStatus = useCallback(async () => {
    if (!subscriptionId) {
      return;
    }

    setStatusLoading(true);
    try {
      const status = await getSubscriptionStatus(subscriptionId);
      setSubscriptionStatus(status);
      setStatusError('');
    } catch (error) {
      setStatusError(error?.message || 'Unable to load subscription status');
    } finally {
      setStatusLoading(false);
    }
  }, [subscriptionId]);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const existingId = params.get('subscriptionId');
    if (existingId) {
      setSubscriptionId(existingId);
    }
  }, []);

  useEffect(() => {
    if (!subscriptionId) {
      return undefined;
    }

    let pollTimer;
    let eventSource;
    let cancelled = false;

    const startPolling = () => {
      refreshStatus();
      pollTimer = setInterval(refreshStatus, POLL_INTERVAL);
      setLive(false);
    };

    const streamUrl = getStatusStreamUrl(subscriptionId);
    if (streamUrl && 'EventSource' in window) {
      try {
        eventSource = new EventSource(streamUrl);
        setLive(true);

        eventSource.onmessage = (event) => {
          if (cancelled || !event?.data) {
            return;
          }

          try {
            const payload = JSON.parse(event.data);
            setSubscriptionStatus(payload);
          } catch (parseError) {
            setStatusError(parseError.message);
          }
        };

        eventSource.onerror = () => {
          setLive(false);
          eventSource?.close();
          if (!pollTimer) {
            startPolling();
          }
        };
      } catch (streamError) {
        setStatusError(streamError?.message || 'Unable to open live updates');
        startPolling();
      }
    } else {
      startPolling();
    }

    return () => {
      cancelled = true;
      if (pollTimer) {
        clearInterval(pollTimer);
      }
      eventSource?.close();
    };
  }, [subscriptionId, refreshStatus]);

  const launchCheckout = useCallback(
    (checkoutPayload, redirectUrl) => {
      if (mercadoPago && checkoutPayload?.preferenceId) {
        mercadoPago.checkout({
          preference: { id: checkoutPayload.preferenceId },
          autoOpen: true,
        });
        return;
      }

      if (mercadoPago && checkoutPayload?.preapprovalId) {
        const checkoutUrl = `https://www.mercadopago.com/subscriptions/checkout?preapproval_id=${checkoutPayload.preapprovalId}`;
        window.open(checkoutUrl, '_blank');
        return;
      }

      if (redirectUrl) {
        window.open(redirectUrl, '_blank');
        return;
      }

      setCheckoutError('No checkout URL or preference id received.');
    },
    [mercadoPago],
  );

  const handleCreateSubscription = useCallback(
    async (payload) => {
      setCreating(true);
      setCheckoutError('');
      setSubscriptionStatus(null);

      try {
        const response = await createSubscription(payload);
        const newId =
          response.subscriptionId || response.id || response.preapproval_id || response.preapprovalId;

        if (!newId) {
          throw new Error('Backend did not return a subscription id.');
        }

        setSubscriptionId(newId);

        const preferenceId = response.preferenceId || response.preference_id;
        const preapprovalId = response.preapprovalId || response.preapproval_id;
        const redirectUrl = response.init_point || response.sandbox_init_point || response.checkoutUrl;

        launchCheckout({ preferenceId, preapprovalId }, redirectUrl);
      } catch (error) {
        setCheckoutError(error?.message || 'Unable to create the subscription');
      } finally {
        setCreating(false);
      }
    },
    [launchCheckout],
  );

  return (
    <div className="app-shell">
      <header className="header">
        <h1>Subscriptions</h1>
        <p className="muted">
          Mercado Pago checkout demo. Public key:{' '}
          <strong>{publicKey || 'not set'}</strong> • API base: <strong>{apiBaseUrl}</strong>
        </p>
      </header>

      <div className="grid">
        <SubscriptionForm
          loading={creating}
          onCreateSubscription={handleCreateSubscription}
          sdkLoading={sdkLoading}
          sdkError={sdkError}
        />

        <div className="stack" style={{ gap: '1rem' }}>
          {checkoutError ? <div className="callout error">{checkoutError}</div> : null}

          {subscriptionId ? (
            <SubscriptionStatus
              error={statusError}
              live={live}
              loading={statusLoading}
              onRefresh={refreshStatus}
              status={subscriptionStatus}
              subscriptionId={subscriptionId}
            />
          ) : (
            <div className="card">
              <div className="stack">
                <div className="muted">Status will appear after creating a subscription.</div>
                <div className="callout">
                  The app polls <code>/subscriptions/:id</code> every {POLL_INTERVAL / 1000}s and
                  attempts server-sent events at <code>/subscriptions/:id/events</code> when
                  available.
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
