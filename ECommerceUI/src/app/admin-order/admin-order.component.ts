import { Component, OnInit } from '@angular/core';
import { Order } from '../models/models';
import { NavigationService } from '../services/navigation.service';
import { PendingOrdersService } from '../services/pending-orders.service';

@Component({
  selector: 'app-admin-order',
  templateUrl: './admin-order.component.html',
  styleUrls: ['./admin-order.component.css']
})
export class AdminOrderComponent implements OnInit {
  
  pendingOrders: Order[] = [];
  orderNumbers: number[] = [];

  constructor(private ordersService: PendingOrdersService) { }

  ngOnInit(): void {
    this.getPendingOrders();
  }

  getPendingOrders() {
    this.ordersService.getPendingOrders().subscribe((data: any) => {
      this.pendingOrders = data;
      this.orderNumbers = Array.from({length: this.pendingOrders.length}, (_, index) => index + 1);
    });
  }

  acceptOrder(orderId: number) {
    this.ordersService.acceptOrder(orderId).subscribe(
      (response) => {
        console.log('Order accepted successfully:', response);
        this.updateOrderStatus(orderId, 'Approved');
        this.getPendingOrders();
      },
      (error) => {
        console.error('Error accepting order:', error);
      }
    );
  }

  rejectOrder(orderId: number) {
    this.ordersService.rejectOrder(orderId).subscribe(
      (response) => {
        console.log('Order rejected successfully:', response);
        this.updateOrderStatus(orderId, 'Rejected');
       this.getPendingOrders();
      },
      (error) => {
        console.error('Error rejecting order:', error);
      }
    );
  }

  private updateOrderStatus(orderId: number, status: string) {
    const orderIndex = this.pendingOrders.findIndex(order => order.id === orderId);
    if (orderIndex !== -1) {
      this.pendingOrders[orderIndex].Status = status;
    }
  }
}
