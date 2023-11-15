import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';

@Component({
  selector: 'app-date-picker',
  templateUrl: './date-picker.component.html',
  styleUrls: ['./date-picker.component.css'],
})
export class DatePickerComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() maxDate: Date | undefined;
  bsConfig: Partial<BsDatepickerConfig> | undefined;

  // @Self creates a new independent instance of whatever it is injecting
  constructor(@Self() public ngControl: NgControl) {
    (this.ngControl.valueAccessor = this),
      (this.bsConfig = {
        containerClass: 'theme-red',
        dateInputFormat: ' DD MM YYYY',
      });
  }

  writeValue(obj: any): void {}

  registerOnChange(fn: any): void {}

  registerOnTouched(fn: any): void {}

  get control(): FormControl {
    // Cast ngControl.control to be type FormControl and then accessed in html as 'control'
    return this.ngControl.control as FormControl;
  }
}
