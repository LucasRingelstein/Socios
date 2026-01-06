import { useEffect, useState } from 'react';

const publicKey = import.meta.env.VITE_MERCADO_PAGO_PUBLIC_KEY;

export default function useMercadoPago() {
  const [loading, setLoading] = useState(Boolean(publicKey));
  const [error, setError] = useState(null);
  const [mercadoPago, setMercadoPago] = useState(null);

  useEffect(() => {
    if (!publicKey) {
      setLoading(false);
      setError('Missing Mercado Pago public key.');
      return undefined;
    }

    let isCancelled = false;
    const sdkGlobal = window.MercadoPago;

    const onLoad = () => {
      if (isCancelled) {
        return;
      }

      if (!window.MercadoPago) {
        setError('Mercado Pago SDK not available after loading.');
        setLoading(false);
        return;
      }

      setMercadoPago(new window.MercadoPago(publicKey));
      setLoading(false);
    };

    if (sdkGlobal) {
      onLoad();
      return undefined;
    }

    const script = document.createElement('script');
    script.src = 'https://sdk.mercadopago.com/js/v2';
    script.async = true;
    script.onload = onLoad;
    script.onerror = () => {
      if (!isCancelled) {
        setError('Unable to load Mercado Pago SDK.');
        setLoading(false);
      }
    };

    document.head.appendChild(script);

    return () => {
      isCancelled = true;
      script.remove();
    };
  }, []);

  return { mercadoPago, loading, error, publicKey };
}
