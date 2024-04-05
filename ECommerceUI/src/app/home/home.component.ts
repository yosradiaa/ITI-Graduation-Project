import { Component, OnInit, Input } from '@angular/core';// Import UtilityService
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
  products: Product[] = [];

  constructor(private navigationService: NavigationService, public utilityService: UtilityService) { } // Make utilityService public


  ngOnInit(): void {
    this.getAllProductsNotRelatedToCategory();
  }


  getAllProductsNotRelatedToCategory() {
    this.navigationService.getAllProductsNotRelatedToCategory().subscribe(
      (products: Product[]) => {
        this.products = products;
      },
      (error) => {
        console.error('Error fetching products:', error);
      }
    );
  }

  // Method to add product to cart
  addToCart(product: Product) {
    // Check if user is logged in
    if (this.utilityService.isLoggedIn()) {
      // Call addToCart method from utility service
      this.utilityService.addToCart(product);
    } else {
      // Handle case when user is not logged in
      alert('Please log in to add to cart.');
    }
  }
}
