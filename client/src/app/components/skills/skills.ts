import { Component, computed, inject } from '@angular/core';
import { ResumeService } from '../../services/resume.service';

const ICONS: Record<string, string> = {
  '.NET Stack': '◆',
  Languages: '≡',
  Frontend: '◈',
  'Microservices & Messaging': '⇄',
  'Data & ORM': '⌬',
  Databases: '▤',
  'Relational Databases': '▤',
  'NoSQL & Caching': '⬡',
  'Data Warehousing & ETL': '◧',
  'Cloud — Microsoft Azure': '☁',
  'Cloud — AWS': '☂',
  'DevOps & CI/CD': '⚙',
  'Identity & Security': '⛨',
  'Architecture & Patterns': '▲',
  'Testing & QA': '✓',
  'Reporting & BI': '◫',
  'ERP & Business Systems': '⊞',
  'CMS & Content Platforms': '⌗',
  'Mainframe Bridge': '▦',
  Collaboration: '✦',
  'AI-Augmented Development': '✺'
};

@Component({
  selector: 'app-skills',
  template: `
    <section id="skills" class="section" aria-labelledby="skills-heading">
      <div class="container">
        <p class="section-eyebrow">Capabilities</p>
        <h2 id="skills-heading" class="section-title">Technical competencies.</h2>
        <p class="section-desc">
          Production-tested expertise spanning the .NET stack, Angular front-ends, microservices,
          Microsoft Azure architecture, and release engineering.
        </p>

        @if (svc.resume(); as r) {
          <div class="skills-grid">
            @for (group of skillGroups(); track group.name) {
              <div class="card">
                <div class="skill-header">
                  <span class="skill-icon" aria-hidden="true">{{ icon(group.name) }}</span>
                  <h3 class="skill-group-title">{{ group.name }}</h3>
                </div>
                <div class="skill-chips">
                  @for (item of group.items; track item) {
                    <span class="chip">{{ item }}</span>
                  }
                </div>
              </div>
            }
          </div>

          <div class="cert-section">
            <div class="cert-header">
              <span style="color: var(--brand-400);">🏅</span>
              <h3 style="font-family: var(--font-mono); font-size: 0.7rem; font-weight: 600; letter-spacing: 0.2em; text-transform: uppercase; color: var(--ink-400); margin: 0;">
                Certifications
              </h3>
            </div>
            <div class="cert-list">
              @for (c of r.certifications; track c.name) {
                <span class="chip-brand">
                  @if (c.id) { {{ c.id }} · }{{ c.name }}
                </span>
              }
            </div>
          </div>
        } @else if (svc.resumeError(); as err) {
          <div class="state-card error" role="alert">{{ err }}</div>
        } @else {
          <div class="state-card skeleton">Loading skills…</div>
        }
      </div>
    </section>
  `
})
export class SkillsComponent {
  protected svc = inject(ResumeService);

  protected skillGroups = computed(() => {
    const r = this.svc.resume();
    if (!r) return [];
    return Object.entries(r.skills).map(([name, items]) => ({ name, items }));
  });

  protected icon(name: string): string {
    return ICONS[name] ?? '◆';
  }
}
