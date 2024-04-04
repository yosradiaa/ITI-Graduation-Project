import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class PendingOrdersService {
  private apiUrl = 'https://localhost:7149/api/Shopping/GetPendingOrders';
  private Url = 'https://localhost:7149/api/Shopping';
  private thisWeekUrl = 'https://localhost:7149/api/Shopping/GetOrdersThisWeek';

  constructor(private http: HttpClient) { }

  getPendingOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.apiUrl);
  }

  acceptOrder(orderId: number): Observable<any> {
    return this.http.put(`${this.Url}/${orderId}/accept`, {}, { responseType: 'text' });
  }
  
  rejectOrder(orderId: number): Observable<any> {
    return this.http.put(`${this.Url}/${orderId}/reject`, {}, { responseType: 'text' });
  } 

  getPendingOrdersThisWeek(): Observable<Order[]> {
    return this.http.get<Order[]>(this.thisWeekUrl);
  }
}
  


