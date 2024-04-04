import { Component, Input, OnInit } from '@angular/core';
import { Product } from '../models/models';
import { UtilityService } from '../services/utility.service';

@Component({
  selector: 'app-product',
  templateUrl: './product.component.html',
  styleUrls: ['./product.component.css'],
})
export class ProductComponent implements OnInit {
  @Input() view: 'grid' | 'list' | 'currcartitem' | 'prevcartitem' = 'grid';
  @Input() product: Product = {
    id: 0,
    title: '',
    description: '',
    price: 0,
    quantity: 0,
    productCategory: {
      id: 1,
      category: '',
      subCategory: '',
    },
    offer: {
      id: 1,
      title: '',
      discount: 0,
    },
    image: '',
  };
  showSuccess: boolean = false;
  addToCartSuccess: boolean = false;
  constructor(public utilityService: UtilityService) {}

  ngOnInit(): void {}
  addToCart(product: Product) {
    this.utilityService.addToCart(product);
    this.addToCartSuccess = true;
    setTimeout(() => {
      this.addToCartSuccess = false;
      this.showSuccess = true;
      setTimeout(() => {
        this.showSuccess = false;
      }, 3000);
    }, 0);
  }

}
