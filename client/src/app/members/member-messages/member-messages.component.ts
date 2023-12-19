import { CommonModule } from '@angular/common';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  imports: [CommonModule, TimeagoModule, FormsModule], // 'CommonModule' allows the use of *ngFor in the html. 'TimeagoModule' for | timeago. FormsModule to use '#messageForm="ngForm"'
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm?: NgForm;
  @Input() username?: string; // Brings username passed from the 'member-details.component'
  messageContent = '';

  constructor(public messageService: MessageService) {}

  ngOnInit(): void {}

  sendMessage() {
    // messageThread observable handles sending the message, only need to reset the form here
    if (!this.username) return;
    this.messageService
      .sendMessage(this.username, this.messageContent)
      .then(() => {
        this.messageForm?.reset();
      });
  }
}
