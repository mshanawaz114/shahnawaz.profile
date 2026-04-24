import { Component, inject } from '@angular/core';
import { ResumeService } from '../../services/resume.service';

@Component({
  selector: 'app-contact',
  template: `
    <section id="contact" class="section" aria-labelledby="contact-heading">
      <div class="container">
        <div class="card contact-card">
          <p class="section-eyebrow">Contact</p>
          <h2 id="contact-heading">Let's discuss your .NET engagement.</h2>
          <p>
            Open to senior, staff, architect, and tech-lead appointments on ASP.NET Core and Angular
            programmes. Reach out directly or use the in-page résumé assistant to get immediate answers.
          </p>
          @if (svc.resume(); as r) {
            <div class="contact-links">
              <a class="btn-primary" [href]="'mailto:' + r.email">✉ Email {{ r.email }}</a>
              @if (r.social.linkedin) {
                <a class="btn-secondary" [href]="r.social.linkedin" target="_blank" rel="noopener noreferrer">
                  💼 LinkedIn
                </a>
              }
              @if (r.social.github) {
                <a class="btn-secondary" [href]="r.social.github" target="_blank" rel="noopener noreferrer">
                  GitHub
                </a>
              }
              @if (r.phone) {
                <a class="btn-secondary" [href]="'tel:' + r.phone">📞 {{ r.phone }}</a>
              }
            </div>
          } @else if (svc.resumeError(); as err) {
            <div class="state-card error" role="alert">{{ err }}</div>
          } @else {
            <div class="state-card skeleton">Loading contact details…</div>
          }
        </div>
      </div>
    </section>
  `
})
export class ContactComponent {
  protected svc = inject(ResumeService);
}
