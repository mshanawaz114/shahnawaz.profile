import { Component, inject } from '@angular/core';
import { ResumeService } from '../../services/resume.service';

@Component({
  selector: 'app-footer',
  template: `
    <footer class="footer">
      <div class="container">
        <p>© {{ year }} Shahnawaz Mohammed · Senior .NET Engineer · Solutions Architect</p>
        @if (svc.resume(); as r) {
          <p style="margin-top: 0.5rem;">
            <a [href]="'mailto:' + r.email">{{ r.email }}</a>
            @if (r.social.linkedin) { · <a [href]="r.social.linkedin" target="_blank" rel="noopener noreferrer">LinkedIn</a> }
            @if (r.social.github) { · <a [href]="r.social.github" target="_blank" rel="noopener noreferrer">GitHub</a> }
          </p>
        }
      </div>
    </footer>
  `
})
export class FooterComponent {
  protected svc = inject(ResumeService);
  protected year = new Date().getFullYear();
}
