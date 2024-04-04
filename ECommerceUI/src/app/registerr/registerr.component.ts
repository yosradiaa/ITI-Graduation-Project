import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  FormControl,
  AbstractControl,
} from '@angular/forms';
import { Router } from '@angular/router';
import { User } from '../models/models';
import { Routes, RouterModule } from '@angular/router';
import { LoginComponent } from '../login/login.component';
import { HttpClient } from '@angular/common/http';
import { NgModule } from '@angular/core';


import { NavigationService } from '../services/navigation.service';
const routes: Routes = [
  { path: 'login', component: LoginComponent },
  // Other routes...
];
@Component({
  selector: 'app-registerr',
  templateUrl: './registerr.component.html',
  styleUrls: ['./registerr.component.css'],
})
export class RegisterrComponent implements OnInit {
  registerForm!: FormGroup;
  invalidRPWD: boolean = false;
  message = '';
  mobileBlurred: boolean = false;
  mobileValid: boolean = true;
  rpwdBlurred: boolean = false;
  emailBlurred: boolean = false;
  passwordBlurred: boolean = false;
  invalidPassword: boolean = false;

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private fb: FormBuilder,
    private navigationService: NavigationService,
    private http: HttpClient
  ) {}
  ngOnInit(): void {
    this.registerForm = this.fb.group({
      firstName: [
        '',
        [
          Validators.required,
          Validators.minLength(2),
          Validators.pattern('[a-zA-Z].*'),
        ],
      ],
      lastName: [
        '',
        [
          Validators.required,
          Validators.minLength(2),
          Validators.pattern('[a-zA-Z].*'),
          this.startsWithAlphabetValidator(),
        ],
      ],
      email: [
        '',
        [
          Validators.required,
          Validators.pattern('[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+.[a-zA-Z]{2,4}$'),
        ],
      ],
      address: ['', [Validators.required]],
      mobile: [
        '',
        [
          Validators.required,
          Validators.maxLength(11),
          Validators.pattern(/^(011|012|015|010)\d{8}$/),
        ],
      ],
      pwd: [
        '',
        [
          Validators.required,
          Validators.minLength(6),
          Validators.maxLength(15),
        ],
      ],
      rpwd: [''],
    });

    this.registerForm.get('rpwd')?.valueChanges.subscribe((value) => {
      this.rpwdBlurred = true;

      if (this.registerForm.value.pwd === value) {
        this.invalidRPWD = false;
      } else {
        this.invalidRPWD = true;
      }
    });
  }
  startsWithAlphabetValidator() {
    return (control: AbstractControl): { [key: string]: any } | null => {
      const value = control.value;
      if (/^[a-zA-Z]/.test(value)) {
        return null; // Valid, starts with alphabet
      } else {
        return { startsWithAlphabet: true }; // Invalid, doesn't start with alphabet
      }
    };
  }

  register() {
    if (this.registerForm.valid) {
      if (this.registerForm.value.pwd !== this.registerForm.value.rpwd) {
        this.invalidRPWD = true;
        return;
      } else {
        this.invalidRPWD = false;
      }

      const userData = {
        firstName: this.registerForm.value.firstName,
        lastName: this.registerForm.value.lastName,
        email: this.registerForm.value.email,
        address: this.registerForm.value.address,
        mobile: this.registerForm.value.mobile,
        password: this.registerForm.value.pwd,
        createdAt: new Date().toISOString(),
        modifiedAt: new Date().toISOString(),
      };

      this.http
        .post<any>('https://localhost:7149/api/Shopping/RegisterUser', userData)
        .subscribe(
          (response: any) => {
            const user: User = {
              id: response.id,
              ...userData,
            };
            // Navigate to login page
            this.router.navigate(['/login']);

            // Print success message to the user
            alert('Account created successfully. You can now login.');
          },
          (error: any) => {
            console.error('Error registering user:', error);
          }
        );
    } else {
      // Handle form validation errors
    }
  }


  closeTab(): void {
    // window.close();
    this.router.navigate(['/home']);
  }
  navigateToLogin() {}
  onBlurEmail() {
    this.emailBlurred = true;
  }

  onBlurPassword() {
    if (this.registerForm.get('pwd')?.invalid) {
      this.invalidPassword = true;
    } else {
      this.invalidPassword = false;
    }
  }
  onBlurMobile() {
    this.mobileBlurred = true;
    this.mobileValid = this.registerForm.get('mobile')?.valid ?? true;
  }

  onBlurRPWD() {
    this.rpwdBlurred = true;
    if (this.registerForm.value.pwd === this.registerForm.value.rpwd) {
      this.invalidRPWD = false;
    } else {
      this.invalidRPWD = true;
    }
  }

  get FirstName(): FormControl {
    return this.registerForm.get('firstName') as FormControl;
  }
  get LastName(): FormControl {
    return this.registerForm.get('lastName') as FormControl;
  }
  get Email(): FormControl {
    return this.registerForm.get('email') as FormControl;
  }
  get Address(): FormControl {
    return this.registerForm.get('address') as FormControl;
  }
  get Mobile(): FormControl {
    return this.registerForm.get('mobile') as FormControl;
  }
  get PWD(): FormControl {
    return this.registerForm.get('pwd') as FormControl;
  }
  get RPWD(): FormControl {
    return this.registerForm.get('rpwd') as FormControl;
  }
}
