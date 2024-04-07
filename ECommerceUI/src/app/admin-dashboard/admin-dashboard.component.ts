import { NavigationService } from './../services/navigation.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit {

  constructor(private NavigationService : NavigationService) { }
  counts: any;
  ngOnInit(): void {
    this.getCounts();
  }
  getCounts(): void {
    this.NavigationService.getCounts()
      .subscribe(
        data => {
          this.counts = data;
          console.log('Counts:', this.counts);
        },
        error => {
          console.error('Error:', error);
        }
      );
  }

}
