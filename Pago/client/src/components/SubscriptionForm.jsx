import { useMemo, useState } from 'react';

const defaultForm = {
  plan: 'basic',
  payerEmail: '',
  description: 'Monthly membership',
};

const plans = {
  basic: { label: 'Basic Plan', price: 19.99, interval: 'monthly' },
  premium: { label: 'Premium Plan', price: 39.99, interval: 'monthly' },
};

export default function SubscriptionForm({
  onCreateSubscription,
  loading,
  sdkLoading,
  sdkError,
}) {
  const [form, setForm] = useState(defaultForm);
  const [validationError, setValidationError] = useState('');

  const selectedPlan = useMemo(() => plans[form.plan], [form.plan]);

  const handleChange = (event) => {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    setValidationError('');

    if (!form.payerEmail) {
      setValidationError('Please enter an email to create the subscription.');
      return;
    }

    onCreateSubscription?.({
      plan: form.plan,
      payerEmail: form.payerEmail,
      description: form.description,
      price: selectedPlan.price,
      interval: selectedPlan.interval,
    });
  };

  return (
    <div className="card">
      <div className="header">
        <h2>Create a subscription</h2>
        <p className="muted small">
          Configure a recurring payment and start the Mercado Pago checkout flow.
        </p>
      </div>

      <form className="stack" onSubmit={handleSubmit}>
        <div className="field">
          <label htmlFor="plan">Plan</label>
          <select id="plan" name="plan" value={form.plan} onChange={handleChange}>
            {Object.entries(plans).map(([value, plan]) => (
              <option key={value} value={value}>
                {plan.label} — ${plan.price} / {plan.interval}
              </option>
            ))}
          </select>
          <div className="tag-list">
            <span className="tag">${selectedPlan.price.toFixed(2)} / {selectedPlan.interval}</span>
            <span className="tag">{selectedPlan.label}</span>
          </div>
        </div>

        <div className="field">
          <label htmlFor="payerEmail">Payer email</label>
          <input
            id="payerEmail"
            name="payerEmail"
            type="email"
            autoComplete="email"
            placeholder="customer@email.com"
            value={form.payerEmail}
            onChange={handleChange}
          />
        </div>

        <div className="field">
          <label htmlFor="description">Description</label>
          <textarea
            id="description"
            name="description"
            rows={3}
            placeholder="What the subscriber is paying for"
            value={form.description}
            onChange={handleChange}
          />
        </div>

        {validationError ? <div className="callout error">{validationError}</div> : null}
        {sdkError ? <div className="callout error">{sdkError}</div> : null}
        {sdkLoading ? (
          <div className="callout">
            Loading Mercado Pago SDK with your public key...
          </div>
        ) : null}

        <button className="button" type="submit" disabled={loading || sdkLoading}>
          {loading ? 'Creating subscription...' : 'Create subscription & checkout'}
        </button>
      </form>
    </div>
  );
}
