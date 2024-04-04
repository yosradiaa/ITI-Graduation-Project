import { Component, OnInit, Input } from '@angular/core';
import { SuggestedProduct } from '../models/models';
import { NavigationService } from '../services/navigation.service';
import { UtilityService } from '../services/utility.service';
import { Category, Product } from '../models/models';
import { ActivatedRoute } from '@angular/router';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnInit {
  
  constructor() {}

  ngOnInit(): void {
  }

}
