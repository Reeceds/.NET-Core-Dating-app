import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css'],
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  bsModalRef: BsModalRef<RolesModalComponent> =
    new BsModalRef<RolesModalComponent>(); // Creates a new instance of a class and passes data to the 'RolesModalComponent'
  availableRoles = ['Admin', 'Moderator', 'Member'];

  constructor(
    private adminService: AdminService,
    private modalService: BsModalService
  ) {}

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe({
      next: (users) => (this.users = users),
    });
  }

  openRolesModal(user: User) {
    // Make sure the 'initialState' property is used and spelled correctly
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        username: user.username,
        availableRoles: this.availableRoles,
        selectedRoles: [...user.roles],
      },
    };
    this.bsModalRef = this.modalService.show(RolesModalComponent, config); // Populates properties in the 'RolesModalComponent' component with 'initialState' properties

    // When closing the modal
    this.bsModalRef.onHide?.subscribe({
      next: () => {
        const selectedRoles = this.bsModalRef.content?.selectedRoles; // Gets an array of the roles that are checked
        // If the checked roles and users current roles are not the same when the modal is closing, then update users roles
        if (!this.arrayEqual(selectedRoles!, user.roles)) {
          this.adminService
            .updateUserRoles(user.username, selectedRoles)
            .subscribe({
              next: (roles) => (user.roles = roles),
            });
        }
      },
    });
  }

  private arrayEqual(arr1: any[], arr2: any[]) {
    return JSON.stringify(arr1.sort()) === JSON.stringify(arr2.sort());
  }
}
