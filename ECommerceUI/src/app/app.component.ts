import { Component } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  title = 'ECommerceUI';
  sideNavStatus = false;

  constructor(private router: Router) {}

  isInAdminDashboard(): boolean {
    return this.router.url.startsWith('/admin');
  }
}

