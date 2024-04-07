import { NavigationService } from './../services/navigation.service';
import { Component, OnInit } from '@angular/core';
import { Contact } from '../models/models';

@Component({
  selector: 'app-admin-contact',
  templateUrl: './admin-contact.component.html',
  styleUrls: ['./admin-contact.component.css']
})
export class AdminContactComponent implements OnInit {

  contacts: Contact[] = [];
  constructor(private contactService: NavigationService) { }

  ngOnInit(): void {
    this.loadContacts();
  }

  loadContacts(): void {
    this.contactService.getAllContacts().subscribe(
      (data: Contact[]) => {
        this.contacts = data;
      },
      error => {
        console.error('Error fetching contacts:', error);
      }
    );
  }

  sortContacts(): void {
    // Sort the contacts array based on the "sentAt" property in descending order
    this.contacts.sort((a, b) => {
      return new Date(b.sentAt).getTime() - new Date(a.sentAt).getTime();
    });
  }


}
