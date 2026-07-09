import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WeatherDataHistoryComponent } from './weather-data-history.component';

describe('WeatherDataHistoryComponent', () => {
  let component: WeatherDataHistoryComponent;
  let fixture: ComponentFixture<WeatherDataHistoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WeatherDataHistoryComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(WeatherDataHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
