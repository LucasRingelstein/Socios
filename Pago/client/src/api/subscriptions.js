const baseUrl = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '');

function assertId(id) {
  if (!id) {
    throw new Error('Subscription id is required to fetch status.');
  }
}

async function handleJsonResponse(response) {
  const contentType = response.headers.get('content-type') || '';
  const isJson = contentType.includes('application/json');
  const data = isJson ? await response.json() : await response.text();

  if (!response.ok) {
    const message = isJson && data && data.message ? data.message : response.statusText;
    throw new Error(message || 'Request failed');
  }

  return data;
}

export async function createSubscription(body) {
  const response = await fetch(`${baseUrl}/subscriptions`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body || {}),
  });

  return handleJsonResponse(response);
}

export async function getSubscriptionStatus(id) {
  assertId(id);
  const response = await fetch(`${baseUrl}/subscriptions/${id}`);
  return handleJsonResponse(response);
}

export function getStatusStreamUrl(id) {
  assertId(id);
  return `${baseUrl}/subscriptions/${id}/events`;
}

export function getBaseUrl() {
  return baseUrl;
}
