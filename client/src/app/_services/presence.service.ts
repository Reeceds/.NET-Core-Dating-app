import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { BehaviorSubject, take } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService, private router: Router) {}

  //Function to maintain a perminent connection to the signalR hub when a user is logged in
  createHubConnection(user: User) {
    // Builds hub connection
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .build();

    // Starts the hub connection
    this.hubConnection.start().catch((error) => console.log(error));

    // Initialize 'UserIsOnline' function specified in PresenceHub.cs
    this.hubConnection.on('UserIsOnline', (username) => {
      this.onlineUsers$.pipe(take(1)).subscribe({
        next: (usernames) =>
          this.onlineUsersSource.next([...usernames, username]),
      });
    });

    this.hubConnection.on('UserIsOffline', (username) => {
      this.onlineUsers$.pipe(take(1)).subscribe({
        next: (usernames) =>
          this.onlineUsersSource.next(usernames.filter((x) => x !== username)), // Creates new array of usernames without the username passed as a param
      });
    });

    this.hubConnection.on('GetOnlineUsers', (usernames) => {
      this.onlineUsersSource.next(usernames);
    });

    // Takes user to message tab when new message notification is clicked. { username, knownAs } object spicified in MessageHub.cs
    this.hubConnection.on('NewMessageReceived', ({ username, knownAs }) => {
      this.toastr
        .info(knownAs + ' has sent you a new message! Click me to see it')
        .onTap.pipe(take(1))
        .subscribe({
          next: () =>
            this.router.navigateByUrl('/members/' + username + '?tab=Messages'),
        });
    });
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch((error) => console.log(error));
  }
}
