import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LoginComponent } from '../login/login.component';
import { Routes, RouterModule } from '@angular/router';

import {
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { User } from '../models/models';
import { NgModule } from '@angular/core';

import { NavigationService } from '../services/navigation.service';
const routes: Routes = [
  { path: 'login', component: LoginComponent },
  // Other routes...
];
@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
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
    private fb: FormBuilder,
    private navigationService: NavigationService,
    private router: Router,
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
            this.router.navigate(['/login']);
          },
          (error: any) => {
            console.error('Error registering user:', error);
          }
        );
    } else {
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
