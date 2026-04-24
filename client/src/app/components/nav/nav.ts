import { Component } from '@angular/core';

@Component({
  selector: 'app-nav',
  template: `
    <nav class="nav" aria-label="Primary">
      <div class="container nav-inner">
        <a href="#top" class="nav-brand" aria-label="Back to top">Shahnawaz Mohammed</a>
        <ul class="nav-links">
          <li><a href="#about">About</a></li>
          <li><a href="#skills">Skills</a></li>
          <li><a href="#experience">Experience</a></li>
          <li><a href="#projects">Projects</a></li>
          <li><a href="#contact">Contact</a></li>
        </ul>
      </div>
    </nav>
  `
})
export class NavComponent {}
