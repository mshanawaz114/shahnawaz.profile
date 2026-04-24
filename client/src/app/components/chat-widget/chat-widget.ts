import { Component, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../services/chat.service';
import { ChatToggleService } from '../../services/chat-toggle.service';
import { ChatMessage } from '../../models/chat.model';

@Component({
  selector: 'app-chat-widget',
  imports: [FormsModule],
  template: `
    <button
      type="button"
      class="chat-launcher"
      aria-label="Open résumé assistant"
      (click)="toggle.toggle()">
      {{ toggle.open() ? '×' : '💬' }}
    </button>

    @if (toggle.open()) {
      <section class="chat-panel" role="dialog" aria-labelledby="chat-title">
        <header class="chat-header">
          <strong id="chat-title">Résumé assistant</strong>
          <button type="button" aria-label="Close chat" (click)="toggle.hide()">×</button>
        </header>

        <div class="chat-body" #body>
          @if (messages().length === 0) {
            <div class="chat-msg assistant">
              Hello. I'm the résumé assistant for Shahnawaz Mohammed. Ask about his .NET engagements,
              certifications, technologies, or availability.
            </div>
          }
          @for (m of messages(); track $index) {
            <div class="chat-msg" [class.user]="m.role === 'user'" [class.assistant]="m.role === 'assistant'">
              {{ m.content }}
            </div>
          }
          @if (loading()) {
            <div class="chat-msg assistant">…</div>
          }
          @if (error()) {
            <div class="chat-msg assistant" role="alert" style="color: var(--pink-500);">
              {{ error() }}
            </div>
          }
        </div>

        <form class="chat-input" (submit)="send($event)">
          <label class="sr-only" for="chat-input-field">Ask about Shahnawaz's résumé</label>
          <input
            id="chat-input-field"
            name="prompt"
            type="text"
            autocomplete="off"
            placeholder="Ask about .NET engagements, certifications, availability…"
            [(ngModel)]="draft"
            [disabled]="loading()">
          <button type="submit" [disabled]="loading() || !draft().trim()">Send</button>
        </form>
      </section>
    }
  `
})
export class ChatWidgetComponent {
  private chatService = inject(ChatService);
  protected toggle = inject(ChatToggleService);
  @ViewChild('body') body?: ElementRef<HTMLDivElement>;

  protected messages = signal<ChatMessage[]>([]);
  protected draft = signal('');
  protected loading = signal(false);
  protected error = signal<string | null>(null);

  send(event: Event) {
    event.preventDefault();
    const text = this.draft().trim();
    if (!text || this.loading()) return;

    const next: ChatMessage[] = [...this.messages(), { role: 'user', content: text }];
    this.messages.set(next);
    this.draft.set('');
    this.loading.set(true);
    this.error.set(null);

    this.chatService.send(next).subscribe({
      next: (res) => {
        this.messages.update(m => [...m, { role: 'assistant', content: res.reply }]);
        this.loading.set(false);
        queueMicrotask(() => this.scrollToBottom());
      },
      error: (err) => {
        console.error('Chat error:', err);
        // Surface the actual server error in dev so config issues are visible.
        // err is an HttpErrorResponse; status === 0 means the request never reached the server.
        const status = err?.status ?? 0;
        const serverMsg = err?.error?.error ?? err?.message ?? 'unknown error';
        let display: string;
        if (status === 0) {
          display = 'Cannot reach the chat API. Confirm the Functions host is running on http://localhost:7071 (cd api && func start).';
        } else if (status === 500) {
          display = `Server error: ${serverMsg}. Check the API terminal — most likely GROQ_API_KEY is not set or the configured model is invalid.`;
        } else if (status === 502) {
          display = `Upstream error: ${serverMsg}. The .NET API reached Groq but Groq returned a failure — see API terminal logs for the exact Groq response (often: deprecated model name or invalid API key).`;
        } else {
          display = `Chat failed (HTTP ${status}): ${serverMsg}. Email mshanawaz.net@gmail.com if this persists.`;
        }
        this.error.set(display);
        this.loading.set(false);
      }
    });
  }

  private scrollToBottom() {
    const el = this.body?.nativeElement;
    if (el) el.scrollTop = el.scrollHeight;
  }
}
