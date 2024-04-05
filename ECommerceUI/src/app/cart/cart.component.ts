import { Component, OnInit } from '@angular/core';
import { Cart, Payment } from '../models/models';
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
      });

    // Get Previous Carts
    this.navigationService
      .getAllPreviousCarts(this.utilityService.getUser().id)
      .subscribe((res: any) => {
        this.usersPreviousCarts = res;
      });
  }

  // Assuming these changes are made within the removeCartItem method
  removeCartItem(cartItem: any): void {
    const index = this.usersCart.cartItems.findIndex(item => item === cartItem);

    if (index !== -1) {
      const cartItemId = cartItem.id; // Assuming 'id' is the property representing the ID of the cart item
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
    }
  }


}
