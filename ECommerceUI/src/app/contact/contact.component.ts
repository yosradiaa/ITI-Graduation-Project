import { Component, OnInit } from '@angular/core';
import { User, Contact } from '../models/models';
import { NavigationService } from '../services/navigation.service';
import { UtilityService } from '../services/utility.service';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-contact',
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.css']
})
export class ContactComponent implements OnInit {
  user: User | undefined;
  showLoginMessage: boolean = false;
  namePlaceholder: string = 'FULL NAME';
  emailPlaceholder: string = 'EMAIL';
  message: string = '';
  showSuccessMessage: boolean = false;
  showRequiredValidation: boolean = false;
  hasInteractedWithMessage: boolean = false;
  constructor(private navigationService: NavigationService, private utilityService: UtilityService) { }

  ngOnInit(): void {

    this.user = this.utilityService.getUser();
    if (!this.user) {
      this.showLoginMessage = true;
      this.namePlaceholder = 'Please Login First';
      this.emailPlaceholder = 'Please Login First';
    }
  }

  submitContact(event: Event) {
    event.preventDefault();


    const formElement = event.target as HTMLFormElement;
    const messageElement = formElement.elements.namedItem('message') as HTMLTextAreaElement;


    if (!messageElement || messageElement.value.trim() === '') {
      console.error('Message is required.');
      return;
    }

    if (this.user) {
      const name = `${this.user.firstName} ${this.user.lastName}`;
      const email = this.user.email;


      const message = messageElement.value;


      const contact: Contact = {
        contactId: 0,
        userId: this.user.id,
        message: message,
        sentAt: new Date(),
        user: this.user
      };


      this.navigationService.insertContact(contact).pipe(
        catchError(error => {
          console.error('An error occurred while submitting contact:', error);
          return throwError('An error occurred while submitting contact. Please try again later.');
        })
      ).subscribe(
        response => {
            console.log('Contact submitted successfully:', response);
            this.message = '';
        },

      );
    }
  }


  toggleSuccessMessage() {
    if (!this.message.trim()) {
        console.error('Message is required.');
        return;
    }

    if (!this.user) {
        this.showLoginMessage = true;
        return;
    }
    this.showSuccessMessage = true;
    // this.message = '';
    // this.hasInteractedWithMessage = false;

    setTimeout(() => {
        this.showSuccessMessage = false;
    }, 3000);
}

handleMessageInteraction() {
    this.hasInteractedWithMessage = true;
}


  checkLogin() {
    if (!this.user) {
      this.showLoginMessage = true;
      setTimeout(() => {
        this.showLoginMessage = false;
      }, 3000);
    }
  }

}
