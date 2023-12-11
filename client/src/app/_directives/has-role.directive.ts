import {
  Directive,
  Input,
  OnInit,
  TemplateRef,
  ViewContainerRef,
} from '@angular/core';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';
import { take } from 'rxjs';

@Directive({
  selector: '[appHasRole]', // use in html as e.g. *appHasRole='["Admin", "fooo"]'
})

// Creates a custom directive to be used in html as '*appHasRole='
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[] = [];
  user: User = {} as User;

  constructor(
    private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private accountService: AccountService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });
  }

  ngOnInit(): void {
    // if SOME of the users roles are included in the 'appHasRole' array (.some checks for matching values)
    if (this.user.roles.some((r) => this.appHasRole.includes(r))) {
      this.viewContainerRef.createEmbeddedView(this.templateRef); // Adds the html element if conditions are met
    } else {
      this.viewContainerRef.clear(); // .viewContainerRef.clear() removes the html element '*appHasRole=' is specified on
    }
  }
}
