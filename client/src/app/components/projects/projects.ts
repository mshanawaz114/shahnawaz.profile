import { Component, inject, signal } from '@angular/core';
import { ResumeService } from '../../services/resume.service';

@Component({
  selector: 'app-projects',
  template: `
    <section id="projects" class="section" aria-labelledby="projects-heading">
      <div class="container">
        <p class="section-eyebrow">Case Studies</p>
        <h2 id="projects-heading" class="section-title">Selected engagements.</h2>
        <p class="section-desc">
          A representative cross-section of ASP.NET Core, .NET microservices, and Angular programmes
          delivered across federal, state, healthcare, and financial institutions.
        </p>

        @if (svc.projects(); as list) {
          <div class="projects-grid">
            @for (p of list; track p.slug) {
              <article class="card proj-card">
                <span class="proj-client">↗ {{ p.client }}</span>
                <h3 class="proj-name">{{ p.name }}</h3>
                <p class="proj-headline">{{ p.headline }}</p>

                <button type="button" class="proj-toggle"
                        [attr.aria-expanded]="isOpen(p.slug)"
                        (click)="toggle(p.slug)">
                  {{ isOpen(p.slug) ? 'Hide details' : 'Read details' }}
                  <span aria-hidden="true">{{ isOpen(p.slug) ? '▾' : '▸' }}</span>
                </button>

                @if (isOpen(p.slug)) {
                  <div class="proj-details">
                    <h5>Problem</h5>
                    <p>{{ p.problem }}</p>
                    <h5>Impact</h5>
                    <ul class="proj-impact">
                      @for (x of p.impact; track x) {
                        <li>{{ x }}</li>
                      }
                    </ul>
                  </div>
                }

                <div class="proj-stack">
                  @for (t of p.stack.slice(0, 6); track t) {
                    <span class="chip">{{ t }}</span>
                  }
                </div>
                @if (p.tags) {
                  <div class="proj-tags">
                    @for (t of p.tags; track t) {
                      <span class="chip-brand">#{{ t }}</span>
                    }
                  </div>
                }
              </article>
            }
          </div>
        } @else if (svc.projectsError(); as err) {
          <div class="state-card error" role="alert">{{ err }}</div>
        } @else {
          <div class="state-card skeleton">Loading case studies…</div>
        }
      </div>
    </section>
  `
})
export class ProjectsComponent {
  protected svc = inject(ResumeService);

  private openSlugs = signal<Set<string>>(new Set());

  protected isOpen(slug: string): boolean {
    return this.openSlugs().has(slug);
  }

  protected toggle(slug: string): void {
    this.openSlugs.update(s => {
      const next = new Set(s);
      if (next.has(slug)) next.delete(slug); else next.add(slug);
      return next;
    });
  }
}
