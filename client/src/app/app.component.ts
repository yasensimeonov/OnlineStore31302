import { Component, OnInit } from '@angular/core';
import { BasketService } from './basket/basket.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'Ski Net store';

  constructor(private basketService: BasketService) {}

  ngOnInit(): void {
    const basketId = localStorage.getItem('basket_id');
    
    if(basketId !== null) {
      this.basketService.getBasket(basketId).subscribe(() => {
        console.log('initialized basket');
      }, error => {
        console.log(error);
      });
    }
    else {
      console.log('Basket Id NOT found in local storage!');
    }
  }
}
