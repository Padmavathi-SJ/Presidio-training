import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WeatherAlertDialogComponent } from './weather-alert-dialog.component';

describe('WeatherAlertDialogComponent', () => {
  let component: WeatherAlertDialogComponent;
  let fixture: ComponentFixture<WeatherAlertDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WeatherAlertDialogComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(WeatherAlertDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
