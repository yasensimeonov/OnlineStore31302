import { ThrowStmt } from '@angular/compiler';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { IBrand } from '../shared/models/brand';
import { IProduct } from '../shared/models/product';
import { IType } from '../shared/models/productType';
import { ShopParams } from '../shared/models/shopparams';
import { ShopService } from './shop.service';

@Component({
  selector: 'app-shop',
  templateUrl: './shop.component.html',
  styleUrls: ['./shop.component.scss']
})
export class ShopComponent implements OnInit {
  @ViewChild('search', {static: false}) searchTerm: ElementRef;
  products: IProduct[];
  brands: IBrand[];
  types: IType[];

  // shopParams = new ShopParams();
  shopParams: ShopParams;

  totalCount: number;
  sortOptions = [
    {name: 'Alphabetical', value: 'name'},
    {name: 'Price: Low to High', value: 'priceAsc'},
    {name: 'Price: High to Low', value: 'priceDesc'},
  ];

  constructor(private shopService: ShopService) {
    this.shopParams = this.shopService.getShopParams();
  }

  ngOnInit(): void {
    this.getProducts(true);
    this.getBrands();
    this.getTypes();
  }

  getProducts(useCache: boolean = false) {
    // this.shopService.getProducts(this.shopParams).subscribe(response => {
    this.shopService.getProducts(useCache).subscribe(response => {
      this.products = response.data;

      // this.shopParams.pageNumber = response.pageIndex;
      // this.shopParams.pageSize = response.pageSize;

      this.totalCount = response.count;
    }, error => {
      console.log(error);
    });
  }

  getBrands() {
    this.shopService.getBrands().subscribe(response => {
      this.brands = [{id: 0, name: 'All'}, ...response];
    }, error => {
      console.log(error);
    });
  }

  getTypes() {
    this.shopService.getTypes().subscribe(response => {
      this.types = [{id: 0, name: 'All'}, ...response];
    }, error => {
      console.log(error);
    });
  }

  onBrandSelected(brandId: number) {
    const params = this.shopService.getShopParams();
    
    // this.shopParams.brandId = brandId;
    // this.shopParams.pageNumber = 1;
    params.brandId = brandId;
    params.pageNumber = 1;
    this.shopService.setShopParams(params);

    this.getProducts();
  }

  onTypeSelected(typeId: number) {
    const params = this.shopService.getShopParams();
    
    // this.shopParams.typeId = typeId;
    // this.shopParams.pageNumber = 1;
    params.typeId = typeId;
    params.pageNumber = 1;
    this.shopService.setShopParams(params);

    this.getProducts();
  }

  onSortSelected(sort: string) {
    const params = this.shopService.getShopParams();
    
    // this.shopParams.sort = sort;
    params.sort = sort;
    this.shopService.setShopParams(params);

    this.getProducts();
  }

  onPageChanged(event: any) {
    const params = this.shopService.getShopParams();
    
    // if (this.shopParams.pageNumber !== event) {
    //   this.shopParams.pageNumber = event;
    //   this.getProducts();
    // }
    if (params.pageNumber !== event) {
      params.pageNumber = event;
      this.shopService.setShopParams(params);

      this.getProducts(true);
    }
  }

  onSearch() {
    const params = this.shopService.getShopParams();
    
    // this.shopParams.search = this.searchTerm.nativeElement.value;
    // this.shopParams.pageNumber = 1;
    params.search = this.searchTerm.nativeElement.value;
    params.pageNumber = 1;
    this.shopService.setShopParams(params);

    this.getProducts();
  }

  onReset() {
    this.searchTerm.nativeElement.value = '';

    // this.shopParams = new ShopParams();
    // const params = new ShopParams();
    this.shopParams = new ShopParams();
    this.shopService.setShopParams(this.shopParams);
    
    this.getProducts();
  }

}
