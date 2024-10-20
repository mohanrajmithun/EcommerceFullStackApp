import { TestBed } from '@angular/core/testing';

import { SaleOrderService } from './sale-order.service';

describe('SaleOrderService', () => {
  let service: SaleOrderService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SaleOrderService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
