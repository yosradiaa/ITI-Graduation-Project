import { Component, OnInit } from '@angular/core';
import { Product } from '../models/models';
import { NavigationService } from '../services/navigation.service';
import { UtilityService } from '../services/utility.service'; // Import UtilityService

@Component({
  selector: 'app-random-products',
  templateUrl: './random-products.component.html',
  styleUrls: ['./random-products.component.css']
})
export class RandomProductsComponent implements OnInit {
  products: Product[] = [];

  constructor(private navigationService: NavigationService, public utilityService: UtilityService) { } // Make utilityService public

  ngOnInit(): void {
    // Fetch all products not related to specific category or subcategory
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
