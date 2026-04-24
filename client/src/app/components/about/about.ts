import { Component, computed, inject } from '@angular/core';
import { ResumeService } from '../../services/resume.service';

@Component({
  selector: 'app-about',
  template: `
    <section id="about" class="section" aria-labelledby="about-heading">
      <div class="container">
        <p class="section-eyebrow">Profile</p>
        <h2 id="about-heading" class="section-title">A senior .NET architect's perspective on enterprise software.</h2>
        <p class="section-desc">
          Fourteen years designing, building, and operating mission-critical ASP.NET Core and Angular platforms across
          federal, state, healthcare, and financial sectors.
        </p>

        @if (svc.resume(); as r) {
          <div class="about-grid">
            <div class="about-summary">
              @for (paragraph of r.summary.slice(0, 4); track paragraph) {
                <p>{{ paragraph }}</p>
              }
            </div>

            <aside aria-label="At a glance">
              <div class="card info-card">
                <span class="info-card-label">Current Engagement</span>
                <div>{{ r.experience[0].role }} at <strong>{{ r.experience[0].company }}</strong>.</div>
              </div>
              <div class="card info-card">
                <span class="info-card-label">Location</span>
                <div>{{ r.location }}</div>
              </div>
              @if (topEducation(); as ed) {
                <div class="card info-card">
                  <span class="info-card-label">Education</span>
                  <div>{{ ed.degree }}</div>
                  <div style="margin-top: 0.3rem; font-size: 0.82rem; color: var(--ink-500);">
                    {{ ed.school }} · {{ ed.location }}
                  </div>
                </div>
              }
            </aside>
          </div>
        } @else if (svc.resumeError(); as err) {
          <div class="state-card error" role="alert">{{ err }}</div>
        } @else {
          <div class="state-card skeleton">Loading profile…</div>
        }
      </div>
    </section>
  `
})
export class AboutComponent {
  protected svc = inject(ResumeService);
  protected topEducation = computed(() => {
    const r = this.svc.resume();
    return r ? (r.education[1] ?? r.education[0]) : undefined;
  });
}
