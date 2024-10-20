import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewSaleOrdersComponent } from './view-sale-orders.component';

describe('ViewSaleOrdersComponent', () => {
  let component: ViewSaleOrdersComponent;
  let fixture: ComponentFixture<ViewSaleOrdersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewSaleOrdersComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewSaleOrdersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
