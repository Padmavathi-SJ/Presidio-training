import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ObservationForm } from './observation-form';

describe('ObservationForm', () => {
  let component: ObservationForm;
  let fixture: ComponentFixture<ObservationForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ObservationForm],
    }).compileComponents();

    fixture = TestBed.createComponent(ObservationForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
