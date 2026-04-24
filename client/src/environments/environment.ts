export const environment = {
  production: false,
  // Local dev: Azure Functions Core Tools (`func start` from /api) runs on http://localhost:7071.
  // In production this is empty so calls go to /api/* against the SWA origin (same-origin).
  apiBase: 'http://localhost:7071'
};
