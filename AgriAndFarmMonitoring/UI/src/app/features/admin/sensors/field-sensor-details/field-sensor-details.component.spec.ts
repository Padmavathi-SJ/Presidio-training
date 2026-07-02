import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FieldSensorDetailsComponent } from './field-sensor-details.component';

describe('FieldSensorDetailsComponent', () => {
  let component: FieldSensorDetailsComponent;
  let fixture: ComponentFixture<FieldSensorDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FieldSensorDetailsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FieldSensorDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
