import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { GalleryModule, ImageItem } from 'ng-gallery';
import { TabDirective, TabsModule, TabsetComponent } from 'ngx-bootstrap/tabs';
import { TimeagoModule } from 'ngx-timeago';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';
import { MessageService } from 'src/app/_services/message.service';
import { Message } from 'src/app/_models/message';
import { PresenceService } from 'src/app/_services/presence.service';
import { AccountService } from 'src/app/_services/account.service';
import { User } from 'src/app/_models/user';
import { take } from 'rxjs';

@Component({
  selector: 'app-member-detail',
  standalone: true, // Standalone comps are not attached to an ngModule i.e. app.module.ts, shared.module.ts. 'GalleryModule' is a standalone comp, therefor this comp needs to be standalone to use it
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
  imports: [
    CommonModule,
    TabsModule,
    GalleryModule,
    TimeagoModule,
    MemberMessagesComponent,
  ], // Imports due to this being standalone comp
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent; // Use @ViewChild to interact with elements in the html. @ViewChild(memberTabs) refers to #memberTabs in the html. ngOnInit() runs before @ViewChild as it runs before the view is initialized
  member: Member = {} as Member; // Initialises 'Member' as an empty obejct but is instantly populated by the root resolver
  images: GalleryModule[] = [];
  activeTab?: TabDirective;
  messages: Message[] = [];
  user: User = {} as User;

  constructor(
    private accountService: AccountService,
    private route: ActivatedRoute,
    private messageService: MessageService,
    public presenceService: PresenceService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });
  }

  ngOnInit(): void {
    this.route.data.subscribe({
      next: (data) => (this.member = data['member']), // Accesses the property specified on the root in 'app-routing.module' - resolve: { member: memberDetailedResolver }
    });

    this.route.queryParams.subscribe({
      // .queryParams is an observable so can be subscribed to
      next: (params) => {
        params['tab'] && this.selectTab(params['tab']); // If the query contains 'tab' and sets the tab to the queryParams (?tab=Messages) 'tab' value of Messages
      },
    });

    this.getImages();
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      this.memberTabs.tabs.find((x) => x.heading === heading)!.active = true;
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    // Only loads the messages once the 'message' tab has been selected
    if (this.activeTab.heading === 'Messages' && this.user) {
      this.messageService.createHubConnection(this.user, this.member.userName);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  loadMessages() {
    if (this.member) {
      this.messageService.getMessageTread(this.member.userName).subscribe({
        next: (messages) => (this.messages = messages),
      });
    }
  }

  getImages() {
    if (!this.member) return;
    for (const photo of this.member?.photos) {
      this.images.push(new ImageItem({ src: photo.url, thumb: photo.url }));
    }
  }
}
