export const environment = {
  production: true,
  // On Azure Static Web Apps with a linked API, /api/* is same-origin.
  // Leaving apiBase empty routes HTTP calls to /api/resume, /api/projects, /api/chat
  // against the same SWA domain.
  apiBase: ''
};
