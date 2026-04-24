import { Component, inject } from '@angular/core';
import { ResumeService } from '../../services/resume.service';

@Component({
  selector: 'app-experience',
  template: `
    <section id="experience" class="section" aria-labelledby="experience-heading">
      <div class="container">
        <p class="section-eyebrow">Experience</p>
        <h2 id="experience-heading" class="section-title">Engagements &amp; roles.</h2>
        <p class="section-desc">
          Tech Lead, Solutions Architect, and Release Manager appointments delivering ASP.NET Core and Angular
          workloads across federal, state, healthcare, and financial organizations. Listed reverse-chronologically.
        </p>

        @if (svc.resume(); as r) {
          <ol class="exp-list">
            @for (job of r.experience; track job.company + job.start; let i = $index) {
              <li class="exp-item">
                <span class="exp-dot" aria-hidden="true"></span>
                <article class="card">
                  <h3 class="exp-company">🏢 {{ job.company }}</h3>
                  <p class="exp-role">
                    <span class="role-name">{{ job.role }}</span>
                    <span> · {{ job.location }}</span>
                  </p>
                  @if (i < 6) {
                    <ul class="exp-highlights">
                      @for (h of job.highlights.slice(0, 4); track h) {
                        <li>{{ h }}</li>
                      }
                    </ul>
                  }
                  @if (job.stack) {
                    <div class="exp-stack">
                      @for (t of job.stack.slice(0, 8); track t) {
                        <span class="chip">{{ t }}</span>
                      }
                    </div>
                  }
                </article>
              </li>
            }
          </ol>

          <p class="exp-note">
            The complete work history is available in the
            <a href="/resume.pdf" target="_blank" rel="noopener noreferrer" class="gradient-text" style="font-weight: 600;">
              résumé (PDF)
            </a>.
          </p>
        } @else if (svc.resumeError(); as err) {
          <div class="state-card error" role="alert">{{ err }}</div>
        } @else {
          <div class="state-card skeleton">Loading experience…</div>
        }
      </div>
    </section>
  `
})
export class ExperienceComponent {
  protected svc = inject(ResumeService);
}
