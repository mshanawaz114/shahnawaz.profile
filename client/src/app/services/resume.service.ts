import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Resume } from '../models/resume.model';
import { Project } from '../models/project.model';

/**
 * Provides résumé and case-study data as Angular signals.
 *
 * Source: bundled static JSON copied from ../api/Data into /data/ at build
 * time (see angular.json `assets` glob). Fetching same-origin static assets
 * means the portfolio renders without any API dependency — the .NET API is
 * only required for the chat endpoint.
 */
@Injectable({ providedIn: 'root' })
export class ResumeService {
  private http = inject(HttpClient);

  // Raw data signals — null while loading.
  private resumeSig = signal<Resume | null>(null);
  private projectsSig = signal<Project[] | null>(null);

  // Error signals — null until something fails.
  private resumeErrSig = signal<string | null>(null);
  private projectsErrSig = signal<string | null>(null);

  // Public readonly views.
  readonly resume = this.resumeSig.asReadonly();
  readonly projects = this.projectsSig.asReadonly();
  readonly resumeError = this.resumeErrSig.asReadonly();
  readonly projectsError = this.projectsErrSig.asReadonly();

  // Convenience: still loading iff no data and no error yet.
  readonly resumeLoading = computed(() => !this.resumeSig() && !this.resumeErrSig());
  readonly projectsLoading = computed(() => !this.projectsSig() && !this.projectsErrSig());

  constructor() {
    this.http.get<Resume>('/data/resume.json').subscribe({
      next: v => this.resumeSig.set(v),
      error: e => {
        console.error('Failed to load /data/resume.json', e);
        this.resumeErrSig.set(
          'Could not load résumé content. Please refresh the page or check your connection.'
        );
      }
    });

    this.http.get<Project[]>('/data/projects.json').subscribe({
      next: v => this.projectsSig.set(v),
      error: e => {
        console.error('Failed to load /data/projects.json', e);
        this.projectsErrSig.set(
          'Could not load case-study content. Please refresh the page or check your connection.'
        );
      }
    });
  }
}
