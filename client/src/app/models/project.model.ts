export interface Project {
  slug: string;
  name: string;
  client: string;
  headline: string;
  problem: string;
  impact: string[];
  stack: string[];
  tags?: string[];
}
