import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css'],
})

//! This file holds validation for reactiveForms
export class TextInputComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() type = 'text';

  // @Self creates a new independent instance of whatever it is injecting
  constructor(@Self() public ngControl: NgControl) {
    this.ngControl.valueAccessor = this;
  }

  writeValue(obj: any): void {}

  registerOnChange(fn: any): void {}

  registerOnTouched(fn: any): void {}

  get control(): FormControl {
    // Cast ngControl.control to be type FormControl and then accessed in html as 'control'
    return this.ngControl.control as FormControl;
  }
}
