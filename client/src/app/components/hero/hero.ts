import { Component, computed, inject } from '@angular/core';
import { ResumeService } from '../../services/resume.service';
import { ChatToggleService } from '../../services/chat-toggle.service';

@Component({
  selector: 'app-hero',
  template: `
    <section id="top" class="hero" aria-labelledby="hero-heading">
      <div class="container">
        @if (svc.resume(); as r) {
          <div class="hero-monogram" aria-hidden="true">{{ initials() }}</div>

          <p class="hero-status">
            <span class="pulse" aria-hidden="true"></span>
            Available for Senior, Staff &amp; Architect engagements
          </p>

          <h1 id="hero-heading" class="hero-name">{{ r.name }}</h1>
          <p class="hero-title gradient-text">{{ r.title }}</p>
          <p class="hero-tagline">{{ r.tagline }}</p>

          <div class="hero-meta">
            <span>📍 {{ r.location }}</span>
            <a [href]="'mailto:' + r.email">✉ {{ r.email }}</a>
          </div>

          <div class="hero-cta">
            <button type="button" class="btn-primary" (click)="chat.show()">
              Engage the résumé assistant →
            </button>
            <a href="/resume.pdf" target="_blank" rel="noopener noreferrer" class="btn-secondary">
              ⬇ Download résumé (PDF)
            </a>
          </div>

          <dl class="hero-stats" aria-label="Credentials at a glance">
            <div class="hero-stat">
              <dt class="hero-stat-value">14+</dt>
              <dd class="hero-stat-label">Years on .NET</dd>
            </div>
            <div class="hero-stat">
              <dt class="hero-stat-value">11</dt>
              <dd class="hero-stat-label">.NET engagements led</dd>
            </div>
            <div class="hero-stat">
              <dt class="hero-stat-value">5</dt>
              <dd class="hero-stat-label">Microsoft / PMP certs</dd>
            </div>
            <div class="hero-stat">
              <dt class="hero-stat-value">.NET 10</dt>
              <dd class="hero-stat-label">Current target runtime</dd>
            </div>
          </dl>
        } @else if (svc.resumeError(); as err) {
          <h1 id="hero-heading" class="hero-name">Shahnawaz Mohammed</h1>
          <p class="hero-title gradient-text">Senior .NET Engineer · Solutions Architect</p>
          <div class="state-card error" role="alert">
            <strong>Content unavailable</strong>
            <p>{{ err }}</p>
          </div>
        } @else {
          <h1 id="hero-heading" class="hero-name skeleton-line skeleton-line--xl">&nbsp;</h1>
          <p class="hero-title skeleton-line skeleton-line--lg">&nbsp;</p>
          <p class="hero-tagline skeleton-line">&nbsp;</p>
        }
      </div>
    </section>
  `
})
export class HeroComponent {
  protected svc = inject(ResumeService);
  protected chat = inject(ChatToggleService);

  protected initials = computed(() => {
    const r = this.svc.resume();
    if (!r) return '';
    return r.name.split(' ').map(s => s[0]).slice(0, 2).join('');
  });
}
