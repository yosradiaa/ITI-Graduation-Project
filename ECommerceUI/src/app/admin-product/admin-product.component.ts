import { Component, OnInit } from '@angular/core';
import { Product } from '../models/models';
import { NavigationService } from '../services/navigation.service';

@Component({
  selector: 'app-admin-product',
  templateUrl: './admin-product.component.html',
  styleUrls: ['./admin-product.component.css']
})
export class AdminProductComponent implements OnInit {

  products: Product[] = [];

  constructor(private navigationService: NavigationService) { }

  ngOnInit(): void {
    this.loadProducts();
  }
  loadProducts() {
    this.navigationService.getAllProducts().subscribe((products: Product[]) => {
      this.products = products;
    });
  }

  // deleteProduct(productId: number) {
  //   if (confirm('Are you sure you want to delete this product?')) {
  //     this.navigationService.deleteProduct(productId).subscribe(() => {
  //       // Remove the deleted product from the list
  //       this.products = this.products.filter(product => product.id !== productId);
  //     });
  //   }
  // }

}
