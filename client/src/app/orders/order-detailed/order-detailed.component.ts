import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { IOrder } from 'src/app/shared/models/order';
import { BreadcrumbService } from 'xng-breadcrumb';
import { OrdersService } from '../orders.service';

@Component({
  selector: 'app-order-detailed',
  templateUrl: './order-detailed.component.html',
  styleUrls: ['./order-detailed.component.scss']
})
export class OrderDetailedComponent implements OnInit {
  order: IOrder;

  constructor(private route: ActivatedRoute, private breadCrumbService: BreadcrumbService, private orderService: OrdersService) {
    this.breadCrumbService.set('@orderDetailed', '');
  }

  ngOnInit(): void {
    this.loadOrder();
  }
  
  loadOrder() {
    this.orderService.getOrderDetailed(+this.route.snapshot.paramMap.get('id')).subscribe((order: IOrder) => {
      this.order = order;
      this.breadCrumbService.set('@orderDetailed', `Order# ${order.id} - ${order.status}`);
    }, error => {
      console.log(error);
    });
  }
  
}
