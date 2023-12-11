import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css'],
})
export class RolesModalComponent implements OnInit {
  username = '';
  availableRoles: any[] = [];
  selectedRoles: any[] = [];

  constructor(public bsModalRef: BsModalRef) {}

  ngOnInit(): void {}

  updateChecked(checkValue: string) {
    const index = this.selectedRoles.indexOf(checkValue);
    // If the item is in the array then remove it, otherwise add it
    index !== -1
      ? this.selectedRoles.splice(index, 1)
      : this.selectedRoles.push(checkValue);
  }
}
