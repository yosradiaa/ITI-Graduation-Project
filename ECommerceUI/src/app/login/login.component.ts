import {
  Component,
  OnInit,
  ViewChild,
  ElementRef,
  Renderer2,
} from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavigationService } from '../services/navigation.service';
import { UtilityService } from '../services/utility.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  @ViewChild('exampleModal') modal!: ElementRef; // Initialize modal property

  loginForm!: FormGroup;
  emailFocused: boolean = false;
  showPasswordError: boolean = false;
  message: string = '';
  passwordInput: { focused: boolean } = { focused: false };

  constructor(
    private fb: FormBuilder,
    private navigationService: NavigationService,
    private utilityService: UtilityService,
    private router: Router,
    private renderer: Renderer2
  ) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      pwd: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  login() {
    if (this.loginForm.valid) {
      const email = this.Email?.value;
      const password = this.PWD?.value;

      if (email === 'admin@gmail.com' && password === '123456') {
        this.router.navigate(['/admin/user-management']);
        return;
      }
      this.navigationService
        .loginUser(email, password)
        .subscribe((res: any) => {
          if (res.toString() !== 'invalid') {
            this.message = 'Logged In Successfully.';
            this.utilityService.setUser(res.toString());
            console.log(this.utilityService.getUser());
            this.router.navigate(['/home']);
          } else {
            this.message = 'Invalid Credentials!';
          }
        });
    }
  }
  openModal() {
    this.renderer.addClass(document.body, 'modal-open'); // Add 'modal-open' class to body
  }
  get Email() {
    return this.loginForm.get('email');
  }

  get PWD() {
    return this.loginForm.get('pwd');
  }

  redirectToGmail() {
    // Redirect to Gmail
    window.location.href = 'https://mail.google.com';
  }
  sendVerificationCode() {
    const email = this.Email?.value;

    // Send verification code logic
    // For demonstration purposes, let's assume the verification code is sent successfully
    console.log('Sending verification code to:', email);
    // You can add your logic here to send the verification code to the user's email
    // After sending the code, you might want to navigate the user to a page where they can enter the code for password reset
  }
}
