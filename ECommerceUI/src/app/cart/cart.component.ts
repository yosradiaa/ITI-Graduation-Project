import { Component, OnInit } from '@angular/core';
import { Cart, Payment, CartItem } from '../models/models';
import { NavigationService } from '../services/navigation.service';
import { UtilityService } from '../services/utility.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css'],
})
export class CartComponent implements OnInit {
  usersCart: Cart = {
    id: 0,
    user: this.utilityService.getUser(),
    cartItems: [],
    ordered: false,
    orderedOn: '',
  };

  usersPaymentInfo: Payment = {
    id: 0,
    user: this.utilityService.getUser(),
    paymentMethod: {
      id: 0,
      type: '',
      provider: '',
      available: false,
      reason: '',
    },
    totalAmount: 0,
    shipingCharges: 0,
    amountReduced: 0,
    amountPaid: 0,
    createdAt: '',
  };

  usersPreviousCarts: Cart[] = [];

  uniqueCartItems: CartItem[] = []; // Array to store unique cart items

  constructor(
    public utilityService: UtilityService,
    private navigationService: NavigationService
  ) {}

  ngOnInit(): void {
    // Get Cart
    this.navigationService
      .getActiveCartOfUser(this.utilityService.getUser().id)
      .subscribe((res: any) => {
        this.usersCart = res;

        // Calculate Payment
        this.utilityService.calculatePayment(
          this.usersCart,
          this.usersPaymentInfo
        );

        // Group cart items to display unique items with their counts
        this.groupCartItems();
      });

    // Get Previous Carts
    this.navigationService
      .getAllPreviousCarts(this.utilityService.getUser().id)
      .subscribe((res: any) => {
        this.usersPreviousCarts = res;
      });
  }

  // Method to group cart items and display unique items with counts
  // Method to group cart items and display unique items with their counts
groupCartItems(): void {
  const groupedItemsMap = new Map<number, CartItem>();

  this.usersCart.cartItems.forEach(item => {
    const existingItem = groupedItemsMap.get(item.product.id);

    if (existingItem) {
      existingItem.quantity += 1; // Increase quantity if the item already exists
    } else {
      // Create a new CartItem with quantity 1 if it doesn't exist
      groupedItemsMap.set(item.product.id, { ...item, quantity: 1 });
    }
  });

  // Convert the map values back to an array
  this.uniqueCartItems = Array.from(groupedItemsMap.values());
}

  // Remove Cart Item
  removeCartItem(cartItem: CartItem): void {
    const index = this.usersCart.cartItems.findIndex(
      (item) => item.id === cartItem.id
    );

    if (index !== -1) {
      const cartItemId = cartItem.id;
      // Remove the item from the local cart
      this.usersCart.cartItems.splice(index, 1);

      // Remove the item from the backend
      this.navigationService.removeCartItem(cartItemId).subscribe(
        (response: any) => {
          console.log('Item removed successfully from backend.');
        },
        (error: any) => {
          console.error('Error removing item from backend:', error);
        }
      );

      // Recalculate the payment
      this.utilityService.calculatePayment(
        this.usersCart,
        this.usersPaymentInfo
      );

      // Regroup cart items after removing one
      this.groupCartItems();
    }
  }

}
