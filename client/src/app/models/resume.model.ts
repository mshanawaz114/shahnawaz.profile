export interface SocialLinks {
  github?: string;
  linkedin?: string;
  stackoverflow?: string;
}

export interface ExperienceEntry {
  company: string;
  location: string;
  role: string;
  start?: string;
  end?: string;
  highlights: string[];
  stack?: string[];
}

export interface EducationEntry {
  school: string;
  location: string;
  degree: string;
  start?: string;
  end?: string;
  notes?: string;
}

export interface Certification {
  name: string;
  id?: string;
  issuer: string;
}

export interface Resume {
  name: string;
  title: string;
  tagline: string;
  location: string;
  email: string;
  phone?: string;
  social: SocialLinks;
  summary: string[];
  skills: { [group: string]: string[] };
  experience: ExperienceEntry[];
  education: EducationEntry[];
  certifications: Certification[];
  interests?: string[];
}
