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

  constructor(private navigationService: NavigationService, public utilityService: UtilityService) { }
  showSuccess: boolean = false;
  addToCartSuccess: boolean = false;


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


  addToCart(product: Product) {
    if (this.utilityService.isLoggedIn()) {
      this.utilityService.addToCart(product);
      this.addToCartSuccess = true;
    setTimeout(() => {
      this.addToCartSuccess = false;
      this.showSuccess = true;
      setTimeout(() => {
        this.showSuccess = false;
      }, 3000);
    }, 0);
    } else {
  
      alert('Please log in to add to cart.');
    }
  }
}
