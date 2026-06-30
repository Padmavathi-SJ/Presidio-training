import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManualWeatherEntryComponent } from './manual-weather-entry.component';

describe('ManualWeatherEntryComponent', () => {
  let component: ManualWeatherEntryComponent;
  let fixture: ComponentFixture<ManualWeatherEntryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManualWeatherEntryComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ManualWeatherEntryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
