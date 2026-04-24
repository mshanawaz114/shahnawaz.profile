export const environment = {
  production: true,
  // On Azure Static Web Apps with a linked Functions API, /api/* is same-origin.
  // Leaving apiBase empty routes the chat HTTP call to /api/chat against the SWA domain.
  // (Résumé and project JSON ship as static assets in /data/, not via the API.)
  apiBase: ''
};
