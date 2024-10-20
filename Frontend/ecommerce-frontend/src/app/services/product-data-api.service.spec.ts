import { TestBed } from '@angular/core/testing';

import { ProductDataApiService } from './product-data-api.service';

describe('ProductDataApiService', () => {
  let service: ProductDataApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProductDataApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
